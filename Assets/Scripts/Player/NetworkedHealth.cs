using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkedHealth : NetworkBehaviour, IDamagable
{ 
    public UnityEvent<ulong> DeathEvent = new UnityEvent<ulong>();
    public ClientPlayerManager manager;

    public virtual void OnHit(ulong Damager)
    {
        Debug.Log($"{Damager} hit {OwnerClientId}");
        DeathEvent.Invoke(Damager);
        manager.OnTankDeath(Damager);
    }
}
