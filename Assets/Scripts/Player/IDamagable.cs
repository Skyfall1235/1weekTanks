using UnityEngine;

public interface IDamagable
{
    /// <summary>
    /// Method to pass hit data to the object
    /// </summary>
    /// <param name="Damager">who triggered the hit</param>
    public abstract void OnHit(ulong Damager);
}
