using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class LobbySceneManager : NetworkBehaviour
{
    //level selection SKIPPED
    //map selection SKIPPED
    //migrate all players (maybe call respawn on all of them?)

    //put all.. idk where i was going witht his

    //client stuff

    void NotifyClientOfSceneChange()
    {

    }
    [ClientRpc]
    void sceneChangeClientRPC()
    {
        //tell client to fade out, and wait for fade in call
        
    }

    //server stuff
}
