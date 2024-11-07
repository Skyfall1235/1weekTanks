using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class ClientPlayerManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    NetworkVariable<NetworkObjectReference> currentTank = new();
    NetworkObject CurrentTank
    {
        get
        {
            currentTank.Value.TryGet(out NetworkObject tankNetworkObject, NetworkManager);
            return tankNetworkObject;
        }
        set
        {
            currentTank.Value = value;
        }
    }
    List<GameObject> spawnPositions = new List<GameObject>();
    [SerializeField]Vector3 spawnCheckBoxHalfExtents;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsOwner)
        {
            spawnPositions = GameObject.FindGameObjectsWithTag("Spawn").ToList();
            SpawnTankAtSpawnPoint();
        }
    }

    void SpawnTankAtSpawnPoint()
    {
        //Remove any existing tanks if the tank still exists
        if(CurrentTank != null)
        {
            DespawnTank(CurrentTank);
        }
        if(spawnPositions.Count > 0)
        {
            for(int i = 0; i < spawnPositions.Count; i++)
            {
                if(Physics.BoxCast(spawnPositions[i].transform.position + new Vector3(0, spawnCheckBoxHalfExtents.y, 0), spawnCheckBoxHalfExtents, spawnPositions[i].transform.forward, spawnPositions[i].transform.rotation))
                {
                    SpawnTank(spawnPositions[i].transform.position, spawnPositions[i].transform.rotation);
                }
                else if(i == spawnPositions.Count - 1)
                {
                    SpawnTank(transform.position, transform.rotation);
                }
            }
        }
        else
        {
            SpawnTank(transform.position, transform.rotation);
        }
        if(CurrentTank != null)
        {
            CurrentTank.gameObject.GetComponent<NetworkedHealth>().DeathEvent.AddListener(OnTankDeath);
        }
    }
    void OnTankDeath(ulong damager)
    {
        if(CurrentTank != null)
        {
            DespawnTank(CurrentTank);
        }
    }
    void DespawnTank(NetworkObject tank)
    {
        if(!IsOwner)
        {
            return;
        }
        if(IsServer)
        {
            tank.Despawn();
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
        CurrentTank = spawnedTank;
    }

    #region RPC Calls
    [ServerRpc]
    void DespawnObjectServerRPC()
    {
        NetworkObject.Despawn();
    }

    [ServerRpc]
    void SpawnObjectServerRPC(Vector3 spawnLocation, Quaternion spawnRotation, ulong clientID)
    {
        OnTankSpawn(spawnLocation, spawnRotation, clientID);
    }
    #endregion
}
