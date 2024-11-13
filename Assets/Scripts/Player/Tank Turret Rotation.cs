using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System;

public class TankTurretRotation : NetworkBehaviour
{
    [SerializeField] float rotationSpeed = 6;
    PlayerInputActions playerInputActions;
    InputAction cursorLook;
    NetworkVariable<short> m_rotation = new(writePerm: NetworkVariableWritePermission.Owner);

    float rotationalVelocity;
    short Rotation
    {
        get
        {
            return m_rotation.Value;
        }
        set
        {

            m_rotation.Value = value;
            transform.rotation = Quaternion.Euler(0, m_rotation.Value, 0);
        }
    }

    short lastAngle = 0;

    Camera cameraForAim;
    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        if(IsOwner)
        {
            lastAngle = (short)transform.rotation.eulerAngles.y;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        cameraForAim = Camera.main;
    }



    private void FixedUpdate()
    {
        if(IsOwner)
        {
            Quaternion desiredRotation = transform.rotation;
            if(cameraForAim != null)
            {
                Vector3 worldAimPoint = cameraForAim.ScreenToWorldPoint
                (
                    new Vector3(cursorLook.ReadValue<Vector2>().x, cursorLook.ReadValue<Vector2>().y, Mathf.Abs(cameraForAim.transform.position.y - transform.position.y))
                );
                worldAimPoint.y = transform.position.y;
                desiredRotation = Quaternion.LookRotation(worldAimPoint - transform.position, transform.up);
            }
            short newRotation = (short)Quaternion.RotateTowards(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime).eulerAngles.y;
            if(newRotation != lastAngle)
            {
                Rotation = newRotation;
                lastAngle = Rotation;
            }
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, Rotation, ref rotationalVelocity, 0.1f), 0);
        }
    }
    private void OnEnable()
    {
        cursorLook = playerInputActions.Player.Look;
        cursorLook.Enable();
    }
    private void OnDisable() 
    {
        cursorLook.Disable();
    }
}
