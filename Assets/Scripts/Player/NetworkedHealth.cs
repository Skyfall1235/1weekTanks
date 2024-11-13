using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkedHealth : NetworkBehaviour, IDamagable
{
    public UnityEvent<ulong, ulong> DeathEvent = new UnityEvent<ulong, ulong>();
    public UnityEvent DeathVFX = new UnityEvent();
    public AudioClip deathExplosion;

    public virtual void OnHit(ulong damager)
    {
        OnHitEventRpc(damager);
        OnDeathFxRPC();
    }

    /// <summary>
    /// event to tell server that a client tank has died
    /// </summary>
    /// <param name="damager">who killed this client</param>
    [Rpc(SendTo.Owner)]
    public void OnHitEventRpc(ulong damager)
    {
        Debug.Log(damager);
        DeathEvent.Invoke(damager, OwnerClientId);
    }

    /// <summary>
    /// events for sound fx and vfx on client side
    /// </summary>
    [Rpc(SendTo.Everyone)]
    public void OnDeathFxRPC()
    {
        DeathVFX.Invoke();
        Destroy(this);
        this.GetComponent<Collider>().enabled = false;
        SoundManager.instance?.PlaySound(transform.position, SoundManager.instance.FindSoundInfoByName("Death"));
    }
}

