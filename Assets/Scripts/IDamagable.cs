using UnityEngine;

public interface IDamagable
{
    /// <summary>
    /// if an object can be hit, call this.
    /// </summary>
    public virtual void OnHit() { }
}
