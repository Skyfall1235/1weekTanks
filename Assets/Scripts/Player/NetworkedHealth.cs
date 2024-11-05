using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkedHealth : NetworkBehaviour, IDamagable
{ 
    public UnityEvent<ulong> DeathEvent = new UnityEvent<ulong>();

    public virtual void OnHit(ulong Damager)
    {
        //Debug.Log($"{Damager} hit {NetworkObjectId}");
        DeathEvent.Invoke(Damager);
        DespawnTank();
    }

    //this despawns the tank depending on what client/host it is
    protected virtual void DespawnTank()
    {
        if (!IsOwner) { return; }
        if (IsServer) { OnDespawnObject(); }
        else { OnDespawnObjectServerRPC(); }
    }

    [ServerRpc]//called on the server, not on client
    void OnDespawnObjectServerRPC()
    {
        OnDespawnObject();
    }

    void OnDespawnObject()
    {
        //we need to destroy it on our side only after we tel lthe server that we need to destroy it.
        NetworkObject.Despawn(true);
    }
}
