using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SubtitleTrigger : MonoBehaviour
{
    [Header("Trigger Behaviour")]
    [Tooltip("If true, the sequence auto-starts when the player enters this trigger's collider.")]
    public bool triggerOnEnter = true;

    [Tooltip("Destroy this GameObject after the sequence (and events) finish.")]
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
            StartSequence();
        }
    }

    /// <summary>
    /// Starts this subtitle sequence manually.
    /// Can be called from UnityEvents, buttons, animation events, etc.
    /// </summary>
    public void StartSequence()
    {
        if (hasTriggered) return;         // prevent double-start
        hasTriggered = true;

        if (subtitleLines == null || subtitleLines.Count == 0)
        {
            // Nothing to play, just finish immediately
            OnSubtitlesComplete();
            return;
        }

        SubtitleManager.Instance?.PlaySubtitleSequence(subtitleLines, OnSubtitlesComplete);
        PlayVoiceLines();
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
