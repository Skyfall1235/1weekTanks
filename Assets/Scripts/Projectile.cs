using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class Projectile : NetworkBehaviour, IDamagable
{
    [SerializeField] List<string> hitLayers = new List<string>();
    [SerializeField] float speed = 10f;
    [SerializeField] Rigidbody body;

    public override void OnNetworkSpawn()
    {
        //when spawning, we call the base spawn and then propel the object forward
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            body = GetComponent<Rigidbody>();
            body.AddForce(transform.forward * speed);
        }
    }

    protected virtual void OnHit()
    {
        //play a sound and blow up
        Despawn();
    }

    protected void Despawn()
    {
        //just for despawning and any other closouts
        NetworkObject.Despawn(); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if the current user isnt the owner, we dont need to handle collision on this end.
        if (!IsOwner)
        {
            return;
        }

        //cogs turned so slow this was written by gemini
        string collisionLayerName = LayerMask.LayerToName(collision.gameObject.layer);
        foreach (string targetLayer in hitLayers)
        {
            if (collisionLayerName == targetLayer)
            {
                OnHit();
                break; 
            }
        }
    }
}
