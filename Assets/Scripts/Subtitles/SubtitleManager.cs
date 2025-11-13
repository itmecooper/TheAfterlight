using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubtitleManager : MonoBehaviour
{
    //94% stolen, I have 24% of an idea of how this works.

    public static SubtitleManager Instance;

    private Dictionary<string, SubtitleData> subtitleLookup = new();

    public GameObject subtitleTextContainer;  //assigned in inspector
    public TMP_Text subtitleText; //assigned in inspector
    private Coroutine currentCoroutine;

    public float delayBetweenSubtitles = .25f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadSubtitlesFromTSV("Subtitles/tutorial_subtitles");
    }

    private void LoadSubtitlesFromTSV(string path)
    {
        TextAsset tsvFile = Resources.Load<TextAsset>(path);
        if (!tsvFile)
        {
            Debug.LogError("Subtitle TSV not found!");
            return;
        }

        var lines = tsvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) //skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cells = lines[i].Split('\t');

            var data = new SubtitleData
            {
                id = cells[0].Trim(),
                subtitleText = cells[1].Trim(),
                displayDuration = float.Parse(cells[2].Trim())
            };

            subtitleLookup[data.id] = data;
        }
    }

    public void PlaySubtitle(string id)
    {
        if (!subtitleLookup.TryGetValue(id, out SubtitleData data))
        {
            Debug.LogWarning($"Subtitle ID not found: {id}");
            return;
        }

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(DisplaySubtitle(data));
    }

    private System.Collections.IEnumerator DisplaySubtitle(SubtitleData data)
    {
        subtitleText.text = data.subtitleText;
        subtitleTextContainer.gameObject.SetActive(true);

        yield return new WaitForSeconds(data.displayDuration);

        subtitleText.text = "";
        subtitleTextContainer.gameObject.SetActive(false);
    }

    public void PlaySubtitleSequence(List<SubtitleLine> lines, System.Action onComplete = null)
    {
        if (currentCoroutine != null) { StopCoroutine(currentCoroutine); }
        currentCoroutine = StartCoroutine(DisplaySubtitleSequence(lines, onComplete));
    }

    private System.Collections.IEnumerator DisplaySubtitleSequence(List<SubtitleLine> lines, System.Action onComplete)
    {
        foreach (var line in lines)
        {
            if (subtitleLookup.TryGetValue(line.id, out SubtitleData data))
            {
                subtitleText.text = data.subtitleText;
                subtitleTextContainer.gameObject.SetActive(true);

                float duration = line.overrideDurationEnabled ? line.customDuration : data.displayDuration;

                yield return new WaitForSeconds(duration);

                subtitleText.text = "";
                subtitleTextContainer.gameObject.SetActive(false);

                //small delay between subtitles
                yield return new WaitForSeconds(delayBetweenSubtitles);
            }
            else
            {
                Debug.LogWarning($"Subtitle ID not found: {line.id}");
            }
        }

        onComplete?.Invoke();
    }

    public SubtitleData GetSubtitleData(string id)
    {
        if (subtitleLookup.TryGetValue(id, out SubtitleData data))
        {
            return data;
        }

        Debug.LogWarning($"Subtitle ID not found: {id}");
        return null;
    }
}
