using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TankController : NetworkBehaviour
{
    PlayerInputActions playerInputActions;
    [SerializeField] InputAction move;
    [SerializeField] InputAction fire;
    Rigidbody rb;
    [SerializeField] float m_speed;
    [SerializeField] float m_rotationSpeed;
    [SerializeField] GameObject instantPoint;
    [SerializeField] GameObject bullet;
    
    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody>();
    }
    void OnEnable()
    {
        move = playerInputActions.Player.Move;
        fire = playerInputActions.Player.Attack;
        move.Enable();  
        fire.Enable();
    }
    void OnDisable()
    {
        move.Disable();
        fire.Disable();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsOwner)
        {
            Camera.main.GetComponent<CameraFollow>().objectToFollow = gameObject.transform;
        }
    }
    void Shoot()
    {
        
    }

    private void FixedUpdate() 
    {
        if(!IsOwner)
        {
            return;
        }
        Vector3 newPosition = transform.position;
        Quaternion newRotation = transform.rotation;

        Vector3 desiredMoveDirection = new Vector3(move.ReadValue<Vector2>().x,0,move.ReadValue<Vector2>().y);
        if(desiredMoveDirection.magnitude > 0)
        {
            newRotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), Time.deltaTime * m_rotationSpeed);
            newPosition += transform.forward * desiredMoveDirection.magnitude * Time.deltaTime * m_speed;    
        }
        rb.Move(newPosition, newRotation);
    }

    private void Update()
    {
        if(!IsOwner) { return; }
        if(fire.triggered)
        {
            FireWeapon();
            return;
            if (IsServer)
            {
                SpawnShell();
            }
            else
            {
                SpawnShellServerRpc();
            }
        }
    }

    [ServerRpc]
    private void SpawnShellServerRpc()
    {
        SpawnShell();
    }

    private void SpawnShell()
    {
        // Instantiate the object on the server
        var spawnedObject = Instantiate(bullet, instantPoint.transform.position, instantPoint.transform.rotation);
        // Spawn the NetworkObject on all clients
        spawnedObject.GetComponent<NetworkObject>().Spawn();
    }

    public void FireWeapon()
    {
        if (!IsOwner)
        {
            return;
        }


        if (IsServer)
        {
            OnFireWeapon();
        }
        else
        {
            OnFireWeaponServerRpc();
        }
    }


    [ServerRpc]
    private void OnFireWeaponServerRpc()
    {
        OnFireWeapon();
    }


    private void OnFireWeapon()
    {
        SpawnShell();
    }

}
