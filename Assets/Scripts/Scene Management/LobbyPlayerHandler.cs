using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayerHandler : NetworkBehaviour
{
    //store the incoming connections
    static public NetworkVariable<Dictionary<ulong, string>> PlayerData = new NetworkVariable<Dictionary<ulong, string>>(new Dictionary<ulong, string>());

    public override void OnNetworkSpawn()
    {
        
    }

    void AddUserToList(ulong userID)
    {

    }

    void RemoveUserFromList(ulong userID)
    {

    }

    [ServerRpc]
    static void SetUserNameToClient(string userName, ulong clientId) 
    {
        if(PlayerData.Value.ContainsKey(clientId))
        {

        }
    }
}
