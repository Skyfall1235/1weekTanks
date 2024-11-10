using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

//this script is to keep track of who kills who, in the event of us wanting to keep track of kills for teams, or FFA
public class KillTracker : NetworkBehaviour
{
    ClientPlayerManager playerManager;
    NetworkVariable<List<KillData>> playerKillData = new NetworkVariable<List<KillData>>();


    //tells the server theres a new kill and to add it to the kill track
    [ServerRpc]
    void SendServerKillServerRPC(ulong inflictor, ulong inflictee)
    {
        Debug.Log("sending the server rpc to kill myself");
        playerKillData.Value.Add(new KillData { inflictor = inflictor, inflictee = inflictee });
    }

    void Start()
    {
        //anon method to update ui based on list
        playerKillData.OnValueChanged += (previousValue, currentValue) =>
        {
            UpdateUI(currentValue);
        };
        if(playerManager.currentTank != null) 
        {
            playerManager.currentTank.Value.TryGet(out NetworkObject foundObject);
            foundObject.gameObject.GetComponent<NetworkedHealth>().DeathEvent.AddListener(SendServerKillServerRPC);
        }
        
    }

    void OnTankSpawn(ulong damager)
    {
        //hook up to network health of the local player events. we can assume I am the inflictee and the incoming value is the damager
    }

    //in the need for a reset of UI, call this method to set the script up approriately client side
    [ClientRpc]
    void OnSceneLoadClientRPC()
    {
        // Clear the UI or reset the kill data
        UpdateUI(new List<KillData>());
    }

    //this should handle the ui script. that can be a dif object but it will need a link here.
    void UpdateUI(List<KillData> currentKillData)
    {

    }

    //who died, who killed them. we can compute team kills, deaths, and other stuff from this datum
    public struct KillData
    {
        public ulong inflictor;
        public ulong inflictee;
    }
}
