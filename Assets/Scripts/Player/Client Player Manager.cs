using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class ClientPlayerManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    NetworkVariable<NetworkObjectReference> currentTank = new(writePerm: NetworkVariableWritePermission.Server);
    List<GameObject> spawnPositions = new List<GameObject>();
    [SerializeField] float respawnTime =  2;
    bool canRespawn = true;
    [SerializeField]Vector3 spawnCheckBoxHalfExtents;

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
            if(!Physics.BoxCast(uncheckedSpawnPositions[spawnIndex].transform.position + new Vector3(0, spawnCheckBoxHalfExtents.y, 0), spawnCheckBoxHalfExtents, uncheckedSpawnPositions[spawnIndex].transform.forward))
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
        foundObject.gameObject.GetComponent<NetworkedHealth>().DeathEvent.AddListener(OnTankDeath);
    }
    void OnTankDeath(ulong damager)
    {
        DespawnTank();
        StartCoroutine(RespawnTankAfterTime());
    }

    #region Respawning Tank
    void RespawnTank()
    {
        CheckCanRespawn(RespawnTankAfterTime());
    }
    IEnumerator RespawnTankAfterTime()
    {
        
        yield return new WaitForSeconds(respawnTime);
        yield return SpawnTankAtRandomSpawnpoint();
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
