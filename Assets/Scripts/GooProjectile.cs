using UnityEngine;
using FMODUnity;

public class GooProjectile : MonoBehaviour
{
    public GameObject gooPlacedPrefab;
    public float lifetime = 5f;
    public float spawnOffset = .3f;

    [Header("Goo Refund")]
    private GameObject player;
    private PlayerController playCont;
    public float gooRefundAmt = 3f;


    [Header("FMOD Events")]
    public EventReference impactSound;
    public EventReference destroySound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //god this is so much cleaner than what I had before. bruh
        Destroy(gameObject, lifetime);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("ProjectileGhostFoam"), LayerMask.NameToLayer("Player"));
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("ProjectileGhostFoam"), LayerMask.NameToLayer("Weapon"));

        player = GameObject.Find("Player");
        playCont = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        //herm. I don't know about this. I think making it a trigger could be better...
        ContactPoint contact = collision.contacts[0];

        if (collision.gameObject.tag == "Enemy")
        {
            if (collision.gameObject.TryGetComponent<EnemyController>(out EnemyController Enemy))
            {
                Enemy.ApplyGoo();
            }

            Debug.Log("GhsotFoam hit gameobject: " + collision.gameObject.name);

        }
        else if (collision.gameObject.tag != "GhostFoam") //this could also be a layer thing.
        {
            if (!impactSound.IsNull)
            {
                RuntimeManager.PlayOneShot(impactSound, transform.position);
            }

            //Quaternion gooRotation = Quaternion.LookRotation(contact.normal);
            Vector3 spawnPosition = contact.point + (contact.normal * spawnOffset);

            //no infinite foam towers, sorry
            //Instantiate(gooPlacedPrefab, contact.point, gooRotation);
            //Instantiate(gooPlacedPrefab, contact.point, Quaternion.identity);
            GameObject gooInstace = Instantiate(gooPlacedPrefab, spawnPosition, Quaternion.identity);

            if (collision.collider.transform.CompareTag("MovingObject"))
            {
                gooInstace.transform.SetParent(collision.collider.transform, true);
            }

        }
        else if (collision.gameObject.tag == "GhostFoam")
        {
            playCont.currStaMana = Mathf.Min(playCont.maxStaMana, playCont.currStaMana + gooRefundAmt);
        }

        Destroy(gameObject);
    }
}
