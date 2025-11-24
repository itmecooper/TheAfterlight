using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SubtitleTrigger : MonoBehaviour
{
    public bool destroyAfterUse = true;

    [Header("Subtitles")]
    public List<SubtitleLine> subtitleLines;

    //herm I don't think this goes here now. I think it gets attached to the data
    [Header("FMOD Voice Events")]
    public List<EventReference> voiceLineEvents;
    private EventReference evt;

    [Header("Event After Sequence")]
    public UnityEngine.Events.UnityEvent onSequenceComplete;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            SubtitleManager.Instance?.PlaySubtitleSequence(subtitleLines, OnSubtitlesComplete);
            PlayVoiceLines();
        }
    }

    private void PlayVoiceLines()
    {
        foreach (var evt in voiceLineEvents)
        {
            if (!evt.IsNull)
            {
                Debug.LogWarning("I don't think this is the best way to play the voice lines," +
                    " maybe check out the spot I marked for you to work in SubtitleManager.cs");
                RuntimeManager.PlayOneShot(evt);
            }
        }
    }

    private void OnSubtitlesComplete()
    {
        onSequenceComplete?.Invoke();
        if (destroyAfterUse) Destroy(gameObject);
    }
}
