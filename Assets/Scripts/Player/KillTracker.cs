using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//this script is to keep track of who kills who, in the event of us wanting to keep track of kills for teams, or FFA
public class KillTracker : NetworkBehaviour
{
    NetworkList<KillData> playerKillData = new NetworkList<KillData>();


    //tells the server theres a new kill and to add it to the kill track
    [ServerRpc]
    public void SendKillServerRPC(ulong inflictor, ulong inflictee)
    {
        Debug.Log("sending the server rpc to kill myself");
        Debug.Log($"{inflictor} killed {inflictee}");
        playerKillData.Add(new KillData { inflictor = inflictor, inflictee = inflictee });
    }

    void Start()
    {
        //anon method to update ui based on list
        //playerKillData.OnValueChanged += (previousValue, currentValue) => { UpdateUI(currentValue); };
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
    public struct KillData : INetworkSerializable, IEquatable<KillData>
    {
        public ulong inflictor;
        public ulong inflictee;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref inflictor);
            serializer.SerializeValue(ref inflictee);
        }

        public bool Equals(KillData other)
        {
            return inflictor == other.inflictor && inflictee == other.inflictee;
        }
    }
}
