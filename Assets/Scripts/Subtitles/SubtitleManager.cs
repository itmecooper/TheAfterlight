using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using static SubtitleData;

public class SubtitleManager : MonoBehaviour
{
    //94% stolen, I have 24% of an idea of how this works.

    public static SubtitleManager Instance;

    private Dictionary<string, SubtitleData> subtitleLookup = new();

    [Header("Ui Refs")]
    public GameObject subtitleTextContainer;  //assigned in inspector
    public TMP_Text speakerNameText; //assigned in inspector
    public TMP_Text subtitleText; //assigned in inspector

    [Header("Speaker Style Presets")]
    public Color defaultColor = new Color(1f, 0.8156863f, 0.1294118f);
    public Color ghostColor = new Color(0.4712394f, 1f, 0.1294118f);

    private Coroutine currentCoroutine;

    public float delayBetweenSubtitles = .25f;

    [Header("Not sure if this works with fmod leo!")]
    public AudioSource voiceAudioSource;

    private void Awake()
    {
        //singleton moment
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        //haha not anymore! tsv sucked major ass
        //LoadSubtitlesFromTSV("Subtitles/tutorial_subtitles");

        LoadSubtitlesFromResources();
    }

    private void LoadSubtitlesFromResources()
    {
        //loads all SubtitleData assets under Resources/Subtitles
        SubtitleData[] allSubtitles = Resources.LoadAll<SubtitleData>("Subtitles");

        subtitleLookup.Clear();

        foreach (var data in allSubtitles)
        {
            if (data == null) continue;

            string key = data.Id;

            if (subtitleLookup.ContainsKey(key))
            {
                Debug.LogWarning($"Duplicate subtitle ID detected: {key} from asset {data.name}");
                continue;
            }

            subtitleLookup[key] = data;
        }

        Debug.Log($"SubtitleManager: Loaded {subtitleLookup.Count} subtitles from Resources/Subtitles.");
    }

    // next two are for id based, single line play. shouldnt really be used i think
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

    private IEnumerator DisplaySubtitle(SubtitleData data)
    {
        if (data == null)
        {
            yield break;
        }

        if (speakerNameText != null)
            speakerNameText.text = data.speakerName;

        subtitleText.text = data.subtitleText;

        ApplySpeakerStyle(data);
        subtitleTextContainer.gameObject.SetActive(true);

        float duration;

        if (data.voiceClip != null && voiceAudioSource != null)
        {
            voiceAudioSource.PlayOneShot(data.voiceClip);
        }

        if (data.voiceClip != null && data.voiceClip.length > 0f)
        {
            duration = data.voiceClip.length;
        }
        else
        {
            duration = data.displayDuration;
        }

        yield return new WaitForSeconds(duration);

        subtitleText.text = "";
        subtitleTextContainer.gameObject.SetActive(false);
    }

    public void PlaySubtitleSequence(List<SubtitleLine> lines, System.Action onComplete = null)
    {
        if (currentCoroutine != null) { StopCoroutine(currentCoroutine); }
        currentCoroutine = StartCoroutine(DisplaySubtitleSequence(lines, onComplete));
    }

    private IEnumerator DisplaySubtitleSequence(List<SubtitleLine> lines, System.Action onComplete)
    {
        if (lines == null || lines.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        foreach (var line in lines)
        {
            var data = line.subtitle;

            if (data == null)
            {
                Debug.LogWarning("SubtitleLine has no SubtitleData assigned.");
                continue;
            }

            if (speakerNameText != null)
                speakerNameText.text = data.speakerName;

            subtitleText.text = data.subtitleText;

            ApplySpeakerStyle(data);
            subtitleTextContainer.gameObject.SetActive(true);

            if (data.voiceClip != null && voiceAudioSource != null)
            {
                voiceAudioSource.PlayOneShot(data.voiceClip);
            }

            float duration;

            //override, then voice clip length, then the display duration in engine
            if (line.overrideDurationEnabled && line.customDuration > 0f)
            {
                duration = line.customDuration;
            }
            else if (data.voiceClip != null && data.voiceClip.length > 0f)
            {
                duration = data.voiceClip.length;
            }
            else
            {
                duration = data.displayDuration;
            }

            if (duration <= 0f)
            {
                Debug.LogWarning("duration is 0 or less of this voiceline bro," +
                    "defaulting to display duration. bad boi");
                duration = data.displayDuration;
            }

            yield return new WaitForSeconds(duration);

            subtitleText.text = "";
            subtitleTextContainer.gameObject.SetActive(false);

            yield return new WaitForSeconds(delayBetweenSubtitles);
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

    private void ApplySpeakerStyle(SubtitleData data)
    {
        if (speakerNameText == null || subtitleText == null || data == null)
            return;

        Color c = defaultColor;
        bool italic = false;

        switch (data.stylePreset)
        {
            case SpeakerStylePreset.Default:
                c = defaultColor;
                italic = false;
                break;

            case SpeakerStylePreset.Ghost:
                c = ghostColor;
                italic = true;
                break;
        }

        //apply color
        speakerNameText.color = c;
        //subtitleText.color = c;

        //apply style
        if (italic)
        {
            //speakerNameText.fontStyle = FontStyles.Italic;
            subtitleText.fontStyle = FontStyles.Italic;
        }
        else
        {
            //speakerNameText.fontStyle = FontStyles.Normal;
            subtitleText.fontStyle = FontStyles.Normal;
        }
    }
}
