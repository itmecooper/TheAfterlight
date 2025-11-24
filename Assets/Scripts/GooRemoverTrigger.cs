using UnityEngine;

public class GooRemoverTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlacedGoo goo = other.GetComponent<PlacedGoo>();
        if (goo != null)
        {
            Destroy(goo.gameObject);
        }
    }
}
