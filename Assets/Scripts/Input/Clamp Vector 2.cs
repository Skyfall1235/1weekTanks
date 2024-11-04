using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class ClampVector2 : InputProcessor<Vector2>
{
    #if UNITY_EDITOR
    static ClampVector2()
    {
        Initialize();
    }
    #endif
    [Tooltip("The minimum of the X value")]
    public float xMin;
    [Tooltip("The minimum of the X value")]
    public float xMax;
    [Tooltip("The minimum of the Y value")]
    public float yMin;
    [Tooltip("The minimum of the Y value")]
    public float yMax;

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        return new Vector2(Mathf.Clamp(value.x, xMin, xMax), Mathf.Clamp(value.y, yMin, yMax));
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<ClampVector2>();
    }
}
