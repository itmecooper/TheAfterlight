using UnityEngine;

[RequireComponent(typeof(Light))]
public class PickupGlowLight : MonoBehaviour
{
    public float baseIntensity = 3f;   // base brightness
    public float pulseAmount = 1f;     // how much it varies
    public float pulseSpeed = 2f;      // speed of pulsing

    private Light _light;

    void Awake()
    {
        _light = GetComponent<Light>();
        _light.intensity = baseIntensity;
    }

    void Update()
    {
        float t = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        _light.intensity = Mathf.Max(0f, t);
    }
}
