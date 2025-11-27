using UnityEngine;

public class GooRemoverTrigger : MonoBehaviour
{
    //vibed better than it was. ugh

    [Tooltip("Root transform of the elevator this cleanup zone belongs to.")]
    public Transform elevatorRoot;

    private void Awake()
    {
        //If not assigned, assume the elevator root is this object’s root
        if (elevatorRoot == null)
        {
            elevatorRoot = transform.root;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Elevator clean up trigger hit: " + other.name);

        //Find any goo associated with this collider
        PlacedGoo goo = other.GetComponentInParent<PlacedGoo>();
        if (goo == null) return;

        //If this goo is a child of the elevator, we KEEP it
        if (goo.transform.IsChildOf(elevatorRoot))
        {
            //This is goo deliberately shot onto the elevator.
            //Let it ride.
            //Debug.Log("Elevator clean up trigger hit goo that's a child of the elevator");
            return;
        }

        //Otherwise, it's goo on walls / floor, so delete it
        //Debug.Log("Elevator clean up is destroying: " + goo.name);
        Destroy(goo.gameObject);
    }
}
