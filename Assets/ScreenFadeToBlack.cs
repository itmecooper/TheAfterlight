using UnityEngine;
using DG.Tweening;

public class ScreenFadeToBlack : MonoBehaviour
{
    public static ScreenFadeToBlack Instance;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Find the fade CanvasGroup — the only one on this canvas
        canvasGroup = GetComponentInChildren<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogError("ScreenFadeToBlack: No CanvasGroup found on FadeCanvas!");
        }

        // Make the WHOLE fade canvas persist
        DontDestroyOnLoad(gameObject);
    }

    public Tween FadeOut(float duration = 0.5f)
    {
        canvasGroup.blocksRaycasts = true;
        return canvasGroup.DOFade(1f, duration);
    }

    public Tween FadeIn(float duration = 0.5f)
    {
        return canvasGroup.DOFade(0f, duration)
            .OnComplete(() => canvasGroup.blocksRaycasts = false);
    }
}