using UnityEngine;

[CreateAssetMenu(menuName = "Subtitles/Subtitle Line", fileName = "NewSubtitle")]
public class SubtitleData : ScriptableObject
{
    //vibed upgrade of old script, this is.. cleaner
    //this is a scriptable object asset!
    //create -> subtitles -> subtitleLine (put in the right folder plz)

    [Header("Unique ID for code. If left empty, the asset's name is used.")]
    //reading the tooltip bro - leave empty
    public string id;

    [Header("Speaker Name")]
    public string speakerName;

    [TextArea]
    public string subtitleText; //the text to display

    public float displayDuration = 3f; //seconds to display

    [Header("Audio, currently untested! Might still work!")]
    public AudioClip voiceClip; //empty for now

    //for convenience - always gives a usable ID
    public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
}