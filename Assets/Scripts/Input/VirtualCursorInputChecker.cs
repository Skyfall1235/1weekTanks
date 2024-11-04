using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class VirtualCursorInputChecker : MonoBehaviour
{
    Image cursorImage ;
    PlayerInputActions playerInputActions;
    void Awake()
    {
        cursorImage = GetComponent<Image>();        
    }
    void OnControlsChanged(PlayerInput playerInput)
    {
        if(playerInput.currentControlScheme.Equals("Gamepad"))
        {
            cursorImage.enabled = true;
        }
        else
        {
            cursorImage.enabled = false;
        }
    }
}
