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
    void DespawnProjectile()
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

    [ServerRpc]
    void OnDespawnObjectServerRPC()
    {
        OnDespawnObject();
    }
    void OnDespawnObject()
    {
        NetworkObject.Despawn();
        Destroy(gameObject);
    }
}
