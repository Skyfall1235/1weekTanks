using System;
using Unity.Netcode;
using UnityEngine;

//this script is to keep track of who kills who, in the event of us wanting to keep track of kills for teams, or FFA
public class KillTracker : NetworkBehaviour
{
    NetworkList<KillData> playerKillData = new NetworkList<KillData>();
    DeathFeed DeathFeed;


    //tells the server theres a new kill and to add it to the kill track
    [ServerRpc]
    public void SendKillServerRPC(ulong inflictor, ulong inflictee)
    {
        Debug.Log("sending the server rpc to kill myself");
        Debug.Log($"{inflictor} killed {inflictee}");
        KillData killData = new KillData(inflictor, inflictee);
        playerKillData.Add(killData);
        UpdateUIClientRPC(killData);
    }

    //in the need for a reset of UI, call this method to set the script up approriately client side
    [ClientRpc]
    void OnSceneLoadClientRPC()
    {

    }

    //this should handle the ui script. that can be a dif object but it will need a link here.
    [ClientRpc]
    void UpdateUIClientRPC(KillData LastKillData)
    {
        DeathFeed.UpdateStack(LastKillData);
    }

    //who died, who killed them. we can compute team kills, deaths, and other stuff from this datum
    public struct KillData : INetworkSerializable, IEquatable<KillData>
    {
        public ulong inflictor;
        public ulong inflictee;

        public KillData(ulong inflictor, ulong inflictee)
        {
            this.inflictor = inflictor;
            this.inflictee = inflictee;
        }

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
