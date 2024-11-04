using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class OffsetVector2 : InputProcessor<Vector2>
{
    #if UNITY_EDITOR
    static OffsetVector2()
    {
        Initialize();
    }
    #endif
    [Tooltip("Offset in the X direction")]
    public float xOffset;
    [Tooltip("Offset in the Y direction")]
    public float yOffset;

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        return new Vector2(value.x + xOffset, value.y + yOffset);
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<OffsetVector2>();
    }
}
