using UnityEngine;

[System.Serializable]
public struct SubtitleLine
{
    public SubtitleData subtitle;

    [Tooltip("Change the asset instead of overriding!")]
    public bool overrideDurationEnabled;
    public float customDuration; //0 to ignore, or a negative number
}
