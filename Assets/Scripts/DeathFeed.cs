using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static KillTracker;

public class DeathFeed : NetworkBehaviour
{
    [SerializeField]
    GameObject canvas;

    [SerializeField]
    GameObject KillFeedPrefab;

    [SerializeField]
    LobbyPlayerHandler playerHandler;

    [SerializeField]
    float feedItemDestroyDelay = 3f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        canvas = GameObject.FindGameObjectWithTag("KillFeed");
    }

    public void UpdateStack(KillData incomingData)
    {
        //move the items upward to the next place
        //create the object, and place it as a child at the bottom. set it to destroy after a period.
        GameObject newItem = CreateKillFeedObject(incomingData);
        newItem.transform.SetParent(canvas.transform, false); //set the parent and let the layout do the rest
        StartCoroutine(DestroyFeedItemAfterDelay(newItem));
    }

    IEnumerator DestroyFeedItemAfterDelay(GameObject objectToDestroy)
    {
        yield return new WaitForSeconds(feedItemDestroyDelay);
        GameObject.Destroy(objectToDestroy); //i specify cause original said obj not GO
    }

    //struct to contain info about a kill feed
    public struct KillFeedItem
    {
        public string Killer;
        public string Victim;
        public GameObject KillFeedObject;

        public KillFeedItem(string killer, string victim, GameObject killFeedObject)
        {
            Killer = killer;
            Victim = victim;
            KillFeedObject = killFeedObject;
        }
    }

    /// <summary>
    /// Creates a kill feed object and populates both is varaibles and its prefab with the incoming data needed to display.
    /// </summary>
    /// <param name="incomingData"> incoming <see cref="KillData"/> struct from the <seealso cref="KillTracker"/></param>
    /// <returns>A kill feed item to be used in the vertical layout group.</returns>
    public GameObject CreateKillFeedObject(KillData incomingData)
    {
        //setup prefab
        GameObject kfObject = Instantiate(KillFeedPrefab);
        Transform parentOfTexts = kfObject.transform.GetChild(0);
        TMP_Text killerTitle = parentOfTexts.GetChild(0).GetComponent<TMP_Text>();
        TMP_Text victimTitle = parentOfTexts.GetChild(1).GetComponent<TMP_Text>();

        //getting strings from the dictionary
        FixedString32Bytes killerUserName;
        playerHandler.PlayerData.Value.TryGetValue(incomingData.inflictor, out killerUserName);
        FixedString32Bytes victimUserName;       
        playerHandler.PlayerData.Value.TryGetValue(incomingData.inflictee, out victimUserName);

        //converting fixed strings to strings
        string killerUser = killerUserName.ToString();
        string victimUser = victimUserName.ToString();

        //setting text of prefab
        killerTitle.text = killerUser;
        victimTitle.text = victimUser;

        //create the prefab
        
        return kfObject;
    }
}
