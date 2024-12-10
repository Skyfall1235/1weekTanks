using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody))]
public class TankController : NetworkBehaviour
{
    PlayerInputActions playerInputActions;
    InputAction move;
    InputAction fire;
    Rigidbody rb;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform firePoint;
    [SerializeField] float m_speed;
    [SerializeField] float m_rotationSpeed;
    
    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        move = playerInputActions.Player.Move;
        move.Enable();

        fire = playerInputActions.Player.Attack;
        fire.Enable();
        fire.performed += Shoot;
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

    void Shoot(InputAction.CallbackContext context)
    {
        Fire();
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

    [ServerRpc]
    private void OnFireShellServerRPC()
    {
        OnFireShell();
    }

    void OnFireShell()
    {
        NetworkObject newProjectile = Instantiate(projectile, firePoint.position, firePoint.rotation).GetComponent<NetworkObject>();
        newProjectile.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
    }

    void Fire()
    {
        if(!IsOwner)
        {
            return;
        }
        if(IsServer)
        {
            OnFireShell();
        }
        else
        {
            OnFireShellServerRPC();
        }
    }
}
