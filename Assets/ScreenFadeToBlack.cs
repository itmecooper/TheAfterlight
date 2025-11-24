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

        canvasGroup = GetComponent<CanvasGroup>();
        DontDestroyOnLoad(gameObject);
    }

    public Tween FadeOut(float duration = 0.5f)
    {
        canvasGroup.blocksRaycasts = true; // block clicks during fade
        return canvasGroup.DOFade(1f, duration);
    }

    public Tween FadeIn(float duration = 0.5f)
    {
        return canvasGroup.DOFade(0f, duration)
            .OnComplete(() => canvasGroup.blocksRaycasts = false);
    }
}
