using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Elevator : MonoBehaviour
{
    //somewhat stolen. merged with my other script like this that was shittier

    [Header("Stops (in order)")]
    [Tooltip("World-space positions the elevator will stop at, e.g. [bottom, mid, top].")]
    public Transform[] stops;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float waitTimeAtStops = 1f;

    [Header("State")]
    [Tooltip("If true, elevator will not move until Unlock() is called.")]
    public bool startLocked = true;

    [Header("FMOD Audio")]
    public EventReference moveLoopEvent;
    public EventReference stopEvent;

    private bool _isLocked;
    private Coroutine _moveRoutine;
    private int _currentIndex = 0;
    private int _direction = 1; // 1 = up in array, -1 = down

    private EventInstance _moveInstance;
    //[Header("Rider Handling")]
    //public string playerTag = "Player";

    //private Transform _originalPlayerParent;

    private void Awake()
    {
        _isLocked = startLocked;

        if (stops == null || stops.Length == 0)
        {
            Debug.LogWarning("ElevatorMultiStop has no stops assigned.", this);
            return;
        }

        // Start at the closest stop to wherever the platform is placed
        _currentIndex = FindClosestStopIndex(transform.position);
        transform.position = stops[_currentIndex].position;
    }

    private void OnEnable()
    {
        if (!_isLocked && stops != null && stops.Length > 1)
        {
            //StartMoving();
        }
    }

    public void Unlock()
    {
        if (!_isLocked) return;
        _isLocked = false;

        if (stops != null && stops.Length > 1 && gameObject.activeInHierarchy)
        {
            StartMoving();
        }
    }

    private void StartMoving()
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
        }

        _moveRoutine = StartCoroutine(MoveLoop());
    }

    private void StartMoveSound()
    {
        if (!moveLoopEvent.IsNull)
        {
            // Stop previous instance if any
            if (_moveInstance.isValid())
            {
                _moveInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _moveInstance.release();
            }

            _moveInstance = RuntimeManager.CreateInstance(moveLoopEvent);
            RuntimeManager.AttachInstanceToGameObject(_moveInstance, transform, GetComponent<Rigidbody>());
            _moveInstance.start();
        }
    }

    private void StopMoveSound()
    {
        if (_moveInstance.isValid())
        {
            _moveInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _moveInstance.release();
            _moveInstance.clearHandle();
        }
    }

    private void PlayStopSound()
    {
        if (!stopEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(stopEvent, transform.position);
        }
    }

    private IEnumerator MoveLoop()
    {
        {
            if (stops == null || stops.Length < 2)
                yield break;

            // When we first unlock, pretend we're "arriving" at the current stop
            // and wait there before moving to the next.
            yield return new WaitForSeconds(waitTimeAtStops);

            // Simple ping-pong: 0 ? 1 ? 2 ? 1 ? 0 ? ...
            while (true)
            {
                int nextIndex = _currentIndex + _direction;

                // If we hit an end, flip direction and recompute
                if (nextIndex < 0 || nextIndex >= stops.Length)
                {
                    _direction *= -1; // reverse
                    nextIndex = _currentIndex + _direction;
                }

                Vector3 targetPos = stops[nextIndex].position;

                StartMoveSound();

                // Move toward the next stop
                while (Vector3.Distance(transform.position, targetPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        targetPos,
                        moveSpeed * Time.deltaTime
                    );

                    yield return null;
                }

                // Snap exactly to the stop
                transform.position = targetPos;
                _currentIndex = nextIndex;

                StopMoveSound();
                PlayStopSound();

                // Wait at this stop before heading to the next
                yield return new WaitForSeconds(waitTimeAtStops);
            }


            //// Simple ping-pong: 0 ? 1 ? 2 ? 1 ? 0 ? 1 ? 2 ? ...
            //while (true)
            //{
            //    int nextIndex = _currentIndex + _direction;

            //    // If we hit an end, flip direction and recompute
            //    if (nextIndex < 0 || nextIndex >= stops.Length)
            //    {
            //        _direction *= -1; // reverse
            //        nextIndex = _currentIndex + _direction;
            //    }

            //    Vector3 targetPos = stops[nextIndex].position;

            //    // Move toward the next stop
            //    while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            //    {
            //        transform.position = Vector3.MoveTowards(
            //            transform.position,
            //            targetPos,
            //            moveSpeed * Time.deltaTime
            //        );

            //        yield return null;
            //    }

            //    // Snap exactly to the stop to avoid float jitter
            //    transform.position = targetPos;
            //    _currentIndex = nextIndex;

            //    // Wait at this stop before heading to the next
            //    yield return new WaitForSeconds(waitTimeAtStops);
        }
    }

        private int FindClosestStopIndex(Vector3 position)
    {
        if (stops == null || stops.Length == 0)
            return 0;

        int closestIndex = 0;
        float closestDist = Vector3.Distance(position, stops[0].position);

        for (int i = 1; i < stops.Length; i++)
        {
            float dist = Vector3.Distance(position, stops[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!other.CompareTag("Player"))
    //        return;

    //    // Cache whatever the player's parent was, then parent them to the elevator
    //    _originalPlayerParent = other.transform.parent;
    //    other.transform.SetParent(transform, true); // keep world position
    //    Debug.Log("Parent set, I think"); //works but isn't preventing jittering
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!other.CompareTag("Player"))
    //        return;

    //    // Restore the original parent when leaving the platform
    //    other.transform.SetParent(_originalPlayerParent, true);
    //    _originalPlayerParent = null;
    //}

    // Optional: draw gizmos so you can see the path in the editor
    private void OnDrawGizmosSelected()
    {
        if (stops == null || stops.Length == 0) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < stops.Length; i++)
        {
            if (stops[i] == null) continue;

            Gizmos.DrawWireSphere(stops[i].position, 0.2f);

            if (i < stops.Length - 1 && stops[i + 1] != null)
            {
                Gizmos.DrawLine(stops[i].position, stops[i + 1].position);
            }
        }
    }
}
