using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

//this script is to keep track of who kills who, in the event of us wanting to keep track of kills for teams, or FFA
public class KillTracker : NetworkBehaviour
{
    NetworkVariable<List<KillData>> playerKillData = new NetworkVariable<List<KillData>>();

    //tells the server theres a new kill and to add it to the kill track
    [ServerRpc]
    void SendServerKill(ulong inflictor, ulong inflictee)
    {

    }

    void Start()
    {
        //anon method to update ui based on list
        playerKillData.OnValueChanged += (previousValue, currentValue) =>
        {
            UpdateUI(currentValue);
        };
        
    }

    void OnTankSpawn(ulong damager)
    {
        //hook up to network health of the local player events. we can assume I am the inflictee and the incoming value is the damager
    }

    //in the need for a reset of UI, call this method to set the script up approriately client side
    [ClientRpc]
    void OnSceneLoadClientRPC()
    {
        //clear the network variable :) or maybe the UI, idk
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
