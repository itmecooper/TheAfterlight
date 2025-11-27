// Highlightable.cs
using UnityEngine;

[DisallowMultipleComponent]
public class Highlightable : MonoBehaviour
{
    [Header("Glow Appearance")]
    public Color glowColor = Color.cyan;   // HDR allowed
    [Min(0f)] public float maxIntensity = 5f; // how bright at peak
    [Tooltip("If a renderer isn't set, we'll grab the first Renderer on this GameObject.")]
    public Renderer targetRenderer;

    MaterialPropertyBlock _mpb;
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    const string EMISSION_KEYWORD = "_EMISSION";

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        // Make sure the material supports emission (URP/Lit does).
        // We safely enable the keyword on sharedMaterial once.
        if (targetRenderer && targetRenderer.sharedMaterial)
            targetRenderer.sharedMaterial.EnableKeyword(EMISSION_KEYWORD);
    }

    /// <summary>
    /// Set 0..1 where 0 = off, 1 = full glow.
    /// </summary>
    public void SetHighlight01(float t)
    {
        if (!targetRenderer) return;

        // Get current MPB, update emission color only.
        targetRenderer.GetPropertyBlock(_mpb);

        // Scale HDR emission by t.
        Color hdr = glowColor * (maxIntensity * Mathf.Max(0f, t));
        _mpb.SetColor(EmissionColorID, hdr);

        targetRenderer.SetPropertyBlock(_mpb);
    }
}
