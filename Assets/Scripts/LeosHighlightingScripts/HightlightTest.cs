using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HighlightDebugAlwaysOn : MonoBehaviour
{
    static readonly int ID_H = Shader.PropertyToID("_GlobalHighlight");
    static readonly int ID_C = Shader.PropertyToID("_GlobalHighlightColor");

    [Range(0, 30)] public float intensity = 12f;
    public Color color = Color.white;

    void Update()
    {
        // Hold Q to max it, otherwise keep it on a steady value
        bool q = Input.GetKey(KeyCode.Q);
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current?.qKey?.isPressed == true) q = true;
#endif

        Shader.SetGlobalColor(ID_C, color);
        Shader.SetGlobalFloat(ID_H, q ? intensity : intensity);

        if (Time.frameCount % 30 == 0)
            Debug.Log($"[HighlightDebug] H={Shader.GetGlobalFloat(ID_H)} C={Shader.GetGlobalColor(ID_C)}");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 30), $"_GlobalHighlight: {Shader.GetGlobalFloat(ID_H):0.00}");
    }
}
