using UnityEngine;

public class ResindueLock : MonoBehaviour
{
    //hardcoded for now...
    //could just do with turning on a componenet that does on awake nonsense maybe
    public Elevator releasedObject;

    private bool _triggered = false;

    private void OnDestroy()
    {
        //prevents multiple calls in case unity is insane and wants to count to 2 by adding one. again
        if (_triggered) return;
        _triggered = true;

        if (releasedObject != null)
        {
            releasedObject.Unlock();
        }
        //else
        //{
            //Debug.LogWarning("ResindueLock destroyed but no elevator assigned");
        //}
    }
}
