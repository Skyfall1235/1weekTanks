using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayerHandler : NetworkBehaviour
{
    // Store the incoming connections
    public NetworkVariable<Dictionary<ulong, FixedString32Bytes>> PlayerData = new NetworkVariable<Dictionary<ulong, FixedString32Bytes>>(new Dictionary<ulong, FixedString32Bytes>());
    public string PlayerName = "Default";

    private static bool eventsRegistered = false;

    public override void OnNetworkSpawn()
    {
        //allows registration of events only once while confirming registrar
        if (IsServer && !eventsRegistered)
        {
            NetworkManager.Singleton.OnConnectionEvent += OnClientConnected;
            NetworkManager.Singleton.OnConnectionEvent += OnClientDisconnected;
            eventsRegistered = true;
        }
    }

    private void OnClientConnected(NetworkManager manager, ConnectionEventData connectionData)
    {
        //dont know if this is needed
        if (!IsServer)
        {
            return;
        }

        // Add the client to the PlayerData dictionary
        if (connectionData.EventType == ConnectionEvent.ClientConnected)
        {
            AddClientUserName(PlayerName, connectionData.ClientId);
        }
    }

    private void OnClientDisconnected(NetworkManager manager, ConnectionEventData connectionData)
    {
        //dont know if this is needed
        if (!IsServer)
        {
            return;
        }

        if (connectionData.EventType == ConnectionEvent.ClientDisconnected)
        {
            // Remove the client from the PlayerData dictionary
            RemoveClientUserName(connectionData.ClientId);
        } 
    }
    private void AddClientUserName(string userName, ulong clientId)
    {
        //dont know if this is needed
        if (!IsServer)
        {
            return;
        }

        //debug to see what client is joining
        Debug.Log($"{OwnerClientId} called this method!");
        //convert a string that we get from other code into a fixed string to pass later
        FixedString32Bytes convertedString = new FixedString32Bytes(userName);
            
        //check if the client number doesnt already exist
        if (!PlayerData.Value.ContainsKey(clientId))
        {
            PlayerData.Value.Add(clientId, convertedString);
            //collection requires me to do this after modification
            PlayerData.CheckDirtyState();
        }
        else
        {
            Debug.LogWarning("Client with ID " + clientId + " already exists.");
        }
            
        //this is another debug to ensure i am getting the string into the dictionary and getting it appropriately
        PlayerData.Value.TryGetValue(clientId, out convertedString);
        Debug.Log(convertedString.ToString());
    }

    private void RemoveClientUserName(ulong clientId)
    {
        //just remove the thing :)
        PlayerData.Value.Remove(clientId);
    }
}
