using System.Collections;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float openAngle = 90f;
    public float rotateDuration = 1.0f;

    [Tooltip("0?1 time, 0?1 rotation. Can overshoot for a 'swing' effect.")]
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Chaining")]
    [Tooltip("Optional: another hinge to open after this one finishes opening.")]
    public ObjectRotator nextToOpen;
    public bool triggerNextOnOpen = false;

    [Header("State")]
    public bool startOpen = false;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private bool _isOpen = false;
    private bool _isRotating = false;

    private void Awake()
    {
        _closedRotation = transform.localRotation;
        _openRotation = _closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        if (startOpen)
        {
            _isOpen = true;
            transform.localRotation = _openRotation;
        }
    }

    public void Toggle()
    {
        if (_isRotating) return;
        StartCoroutine(RotateTo(!_isOpen));
    }

    public void Open()
    {
        if (_isOpen || _isRotating) return;
        StartCoroutine(RotateTo(true));
    }

    public void Close()
    {
        if (!_isOpen || _isRotating) return;
        StartCoroutine(RotateTo(false));
    }

    private IEnumerator RotateTo(bool targetOpen)
    {
        _isRotating = true;

        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = targetOpen ? _openRotation : _closedRotation;

        float elapsed = 0f;

        while (elapsed < rotateDuration)
        {
            float t = elapsed / rotateDuration;
            float curvedT = rotationCurve.Evaluate(t);  // <- non-linear, can swing/overshoot

            transform.localRotation = Quaternion.Slerp(startRot, targetRot, curvedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = targetRot;
        _isOpen = targetOpen;
        _isRotating = false;

        // If we *just finished opening* and we have a chained hinge, open it now
        if (_isOpen && triggerNextOnOpen && nextToOpen != null)
        {
            nextToOpen.Open();
        }
    }
}