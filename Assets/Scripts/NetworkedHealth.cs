using Unity.Netcode;
using UnityEngine;

public class NetworkedHealth : NetworkBehaviour
{
    [SerializeField] int baseHealth = 100;
    NetworkVariable<int> health = new(writePerm: NetworkVariableWritePermission.Owner);
    public bool isAlive => health.Value > 0; 

    void Start()
    {
        if(IsOwner)
        {
            health.Value = baseHealth;
        }
    }
    
    [Rpc(SendTo.Owner)]
    public void DamageHealthRPC(int damageToDeal)
    {
        baseHealth -= damageToDeal;
    }


}
