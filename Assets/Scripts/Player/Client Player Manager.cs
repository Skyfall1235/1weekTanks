using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class ClientPlayerManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    public NetworkVariable<NetworkObjectReference> currentTank = new(writePerm: NetworkVariableWritePermission.Server);
    List<GameObject> spawnPositions = new List<GameObject>();
    [SerializeField] float DestroyDelay = 2f;
    bool canRespawn = true;
    [SerializeField]Vector3 spawnCheckBoxHalfExtents = new Vector3(.5f, .25f, .5f);


    #region network specific
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsOwner)
        {
            spawnPositions = GameObject.FindGameObjectsWithTag("Spawn").ToList();
            StartCoroutine(SpawnTankAtRandomSpawnpoint());
        }
    }
    bool CheckForUpdateNetworkObject()
    {
        currentTank.Value.TryGet(out NetworkObject foundObject);
        return foundObject != null;
    }
    #endregion

    public void HandleSceneTransition()
    {
        canRespawn = false;
    }

    IEnumerator SpawnTankAtRandomSpawnpoint()
    {
        Vector3 currentSpawnPosition = transform.position;
        Quaternion currentSpawnRotation = transform.rotation;
        List<GameObject> uncheckedSpawnPositions = new List<GameObject>(spawnPositions);
        while(uncheckedSpawnPositions.Count > 0)
        {
            int spawnIndex = Random.Range(0, uncheckedSpawnPositions.Count);
            if (!Physics.BoxCast(uncheckedSpawnPositions[spawnIndex].transform.position + new Vector3(0, spawnCheckBoxHalfExtents.y, 0), spawnCheckBoxHalfExtents, uncheckedSpawnPositions[spawnIndex].transform.forward, Quaternion.identity, 0f, LayerMask.GetMask("Tank")))
            {
                currentSpawnPosition = uncheckedSpawnPositions[spawnIndex].transform.position;
                currentSpawnRotation = uncheckedSpawnPositions[spawnIndex].transform.rotation;
                break;
            }
            else
            {
                uncheckedSpawnPositions.RemoveAt(spawnIndex);
            }
        } 
        SpawnTank(currentSpawnPosition, currentSpawnRotation);
        yield return new WaitUntil(CheckForUpdateNetworkObject);
        currentTank.Value.TryGet(out NetworkObject foundObject);
        KillTracker tracker = GetComponent<KillTracker>();
        foundObject.gameObject.GetComponent<NetworkedHealth>().DeathEvent.AddListener(OnTankDeath);
        foundObject.gameObject.GetComponent<NetworkedHealth>().DeathEvent.AddListener(tracker.SendKillServerRPC);
    }
    void OnTankDeath(ulong inflictor, ulong inflictee)
    {
        StartCoroutine(KillTankAfterDelay(inflictee));
    }

    #region Respawning Tank

    IEnumerator KillTankAfterDelay(ulong inflictee)
    {
        yield return new WaitForSeconds(DestroyDelay);
        DespawnTank();
        StartCoroutine(RespawnTankAfterTime());
    }

    IEnumerator RespawnTankAfterTime()
    {
        
        const float DEATH_DELAY = 2.5f;
        yield return new WaitForSeconds(DEATH_DELAY); // :)
        yield return SpawnTankAtRandomSpawnpoint();
    }

    void RespawnTank()
    {
        CheckCanRespawn(RespawnTankAfterTime());
    }

    IEnumerator CheckCanRespawn(IEnumerator continuation)
    {
        while (!canRespawn)
        {
            yield return null;
        }
        StartCoroutine(continuation);
    }

    #endregion

    #region Spawn of tank
    void DespawnTank()
    {
        if(!IsOwner)
        {
            return;
        }
        if(IsServer)
        {
            currentTank.Value.TryGet(out NetworkObject foundObject);
            foundObject.Despawn();
        }
        else
        {
            DespawnObjectServerRPC();
        }
    }
    void SpawnTank(Vector3 spawnLocation, Quaternion spawnRotation)
    {
        if(!IsOwner)
        {
            return;
        }
        if(IsServer)
        {
            OnTankSpawn(spawnLocation, spawnRotation, OwnerClientId);
        }
        else
        {
            SpawnObjectServerRPC(spawnLocation, spawnRotation, OwnerClientId);
        }
    }
    void OnTankSpawn(Vector3 spawnLocation, Quaternion spawnRotation, ulong clientID)
    {
        NetworkObject spawnedTank = Instantiate(playerPrefab, spawnLocation, spawnRotation).GetComponent<NetworkObject>();
        spawnedTank.SpawnWithOwnership(clientID);
        currentTank.Value = new NetworkObjectReference(spawnedTank);
    }
    #endregion

    #region RPC Calls

    [ServerRpc]
    void DespawnObjectServerRPC()
    {
        currentTank.Value.TryGet(out NetworkObject foundObject);
        foundObject.Despawn();
    }

    [ServerRpc]
    void SpawnObjectServerRPC(Vector3 spawnLocation, Quaternion spawnRotation, ulong clientID)
    {
        OnTankSpawn(spawnLocation, spawnRotation, clientID);
    }

    #endregion
}
