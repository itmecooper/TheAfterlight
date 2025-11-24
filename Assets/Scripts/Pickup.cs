using TMPro;
using UnityEngine;
using FMODUnity;

public class Pickup : MonoBehaviour
{
    //this is just.. a label. for now
    public enum PickupType { HealthRefill, AmmoRefill, CombatReward, ExplorationReward, FakeGun, Lantern, SceneExit }
    public PickupType pickupType;
    //public float amount = 15f;


    //public GameObject pickupUIPrompt;
    //was for the canister "E" but will now be the text on the canvas

    //private Camera mainCamera;

    [Header("Audio")]
    public EventReference sfxOnPickup;
    public GameObject glowRingPrefab;

    private void Start()
    {
        {
            if (glowRingPrefab != null)
            {
                GameObject ring = Instantiate(glowRingPrefab, transform);
                ring.transform.localPosition = Vector3.zero;   // center
                ring.transform.localRotation = Quaternion.identity;
            }
        }
        //mainCamera = Camera.main;
    }

    //private void LateUpdate()
    //{
    //    if (pickupUIPrompt.activeSelf)
    //    {
    //        if (mainCamera != null)
    //        {
    //            pickupUIPrompt.transform.LookAt(mainCamera.transform);
    //            //pickupUIPrompt.transform.LookAt(mainCamera.transform.rotation * Vector3.forward,
    //            //                 mainCamera.transform.rotation * Vector3.up);
    //        }
    //    }
    //}

    //public void ShowButtonPrompt(bool setActive)
    //{
    //    pickupUIPrompt.SetActive(setActive);
    //}

}
