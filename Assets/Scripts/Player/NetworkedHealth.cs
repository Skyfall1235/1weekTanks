using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkedHealth : NetworkBehaviour, IDamagable
{
    public UnityEvent<ulong, ulong> DeathEvent = new UnityEvent<ulong, ulong>();
    public UnityEvent DeathSFX = new UnityEvent();
    public virtual void OnHit(ulong damager)
    {
        OnHitEventRpc(damager);
        OnDeathFxRPC();
    }

    [Rpc(SendTo.Owner)]
    public void OnHitEventRpc(ulong damager)
    {
        Debug.Log(damager);
        DeathEvent.Invoke(damager, OwnerClientId);
    }

    [Rpc(SendTo.Everyone)]
    public void OnDeathFxRPC()
    {
        DeathSFX.Invoke();
    }


}
