using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private void Awake()
    {
        //singleton moment
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        //later: trigger fade-out here
        //like with a big solid black image or something
        // e.x. UITransition.Instance.FadeOut();
        //later is NOW

        //Fade out
        if (ScreenFadeToBlack.Instance != null)
        {
            yield return ScreenFadeToBlack.Instance.FadeOut(0.5f).WaitForCompletion();
        }


        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        //Ready, activate scene bro
        op.allowSceneActivation = true;

        //Wait 1 frame for scene to fully appear
        yield return null;

        //Fade in
        if (ScreenFadeToBlack.Instance != null)
        {
            ScreenFadeToBlack.Instance.FadeIn(0.5f);
        }
    }
}