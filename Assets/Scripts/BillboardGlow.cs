using UnityEngine;

public class BillboardGlow : MonoBehaviour
{
    public float rotateSpeed = 50f;
    public float pulseSpeed = 6f;
    public float pulseAmount = 6f;       // how much extra brightness
    public float baseIntensity = 4f;     // starting brightness

    private Material _mat;
    private Color _baseColor;

    void Start()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            _mat = r.material;

            // Whatever you set in the material inspector
            _baseColor = _mat.GetColor("_BaseColor");
        }
    }

    void LateUpdate()
    {
        // Billboard toward camera
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);

        // Slow spin
        transform.Rotate(Vector3.up * rotateSpeed * Time.unscaledDeltaTime, Space.World);

        // Pulse brightness
        if (_mat != null)
        {
            float intensity = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

            // Don’t let it go negative
            intensity = Mathf.Max(0f, intensity);

            Color c = _baseColor * intensity;
            // keep same alpha as original so it stays visible
            c.a = _baseColor.a;
            _mat.SetColor("_BaseColor", c);
        }
    }
}
