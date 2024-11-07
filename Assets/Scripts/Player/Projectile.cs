using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour 
{
    Rigidbody rb;
    float timeBeforeDespawn = 2;
    float projectileSpeed = 10;
    public ulong spawnerID = 1000;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();    
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb.AddForce(transform.forward * projectileSpeed, ForceMode.Impulse);
        StartCoroutine(DespawnAfterTime());
    }

    IEnumerator DespawnAfterTime()
    {
        yield return new WaitForSeconds(timeBeforeDespawn);
        DespawnProjectile();
    }

    protected virtual void DespawnProjectile()
    {
        if(!IsOwner)
        {
            return;
        }
        if(IsServer)
        {
            OnDespawnObject();
        }
        else
        {
            OnDespawnObjectServerRPC();
        }
    }

    [ServerRpc]//called on the server, not on client
    void OnDespawnObjectServerRPC()
    {
        OnDespawnObject();
    }

    void OnDespawnObject()
    {
        NetworkObject.Despawn(true);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (IsOwner) return;

        IDamagable collided = collision.gameObject.GetComponent<IDamagable>(); 
        if(collided != null)
        {
            collided.OnHit(spawnerID);
        }
    }
}