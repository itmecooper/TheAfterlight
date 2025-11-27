// HighlightFlashManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // for Keyboard.current
#endif

public class HighlightFlashManager : MonoBehaviour
{
    [Header("Input")]
    public KeyCode triggerKey = KeyCode.Q;

    [Header("Timing")]
    public float duration = 1.0f;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

    [Header("Target Filters (optional)")]
    public bool restrictByLayers = false;
    public LayerMask layers = ~0; // default to Everything

    Coroutine _running;

    void Awake()
    {
        if (intensityCurve.length <= 2 || intensityCurve.Evaluate(0f) == 0f && intensityCurve.Evaluate(1f) == 0f)
        {
            // Nice pulse by default
            intensityCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 2f, 2f),
                new Keyframe(0.15f, 1f, 0f, 0f),
                new Keyframe(1f, 0f, 0f, 0f)
            );
        }

        Debug.Log("[HighlightFlashManager] Awake. InputSystem enabled: " +
#if ENABLE_INPUT_SYSTEM
            "YES"
#else
            "NO"
#endif
        + ", restrictByLayers=" + restrictByLayers + ", mask=" + layers.value);
    }

    void Update()
    {
        bool pressed = false;

        // Old Input Manager
        if (Input.GetKeyDown(triggerKey)) pressed = true;

        // New Input System (works even if old input is disabled)
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var q = Keyboard.current.qKey;
            if (q != null && q.wasPressedThisFrame) pressed = true;
        }
#endif

        if (pressed)
        {
            Debug.Log("[HighlightFlashManager] Q pressed. Starting flash.");
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(FlashOnce());
        }
    }

    IEnumerator FlashOnce()
    {
        Highlightable[] all = FindObjectsByType<Highlightable>(FindObjectsSortMode.None);

        List<Highlightable> filtered = new List<Highlightable>(all.Length);
        if (restrictByLayers)
        {
            foreach (var h in all)
            {
                if (h && h.gameObject.activeInHierarchy)
                {
                    if (((1 << h.gameObject.layer) & layers.value) != 0)
                        filtered.Add(h);
                }
            }
        }
        else
        {
            foreach (var h in all)
                if (h && h.gameObject.activeInHierarchy) filtered.Add(h);
        }

        Debug.Log("[HighlightFlashManager] Targets found: " + filtered.Count);

        float t = 0f;
        while (t < duration)
        {
            float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, duration));
            float k = Mathf.Clamp01(intensityCurve.Evaluate(u));

            foreach (var h in filtered) h.SetHighlight01(k);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach (var h in filtered) h.SetHighlight01(0f);
        _running = null;
        Debug.Log("[HighlightFlashManager] Flash complete.");
    }
}
