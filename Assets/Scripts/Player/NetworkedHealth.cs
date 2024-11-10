using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkedHealth : NetworkBehaviour, IDamagable
{
    public UnityEvent<ulong, ulong> DeathEvent = new UnityEvent<ulong, ulong>();
    public virtual void OnHit(ulong damager)
    {
        OnHitEventRpc(damager);
    }

    [Rpc(SendTo.Owner)]
    public void OnHitEventRpc(ulong damager)
    {
        Debug.Log(damager);
        DeathEvent.Invoke(damager, OwnerClientId);
    }
}
