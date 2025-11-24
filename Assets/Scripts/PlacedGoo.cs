using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class PlacedGoo : MonoBehaviour
{
    private GameObject player;
    private PlayerController playCont;

    public float growTime;
    private Vector3 finalScale;

    public float gooRefundAmt = 5f;

    [Header("FMOD Events")]
    public EventReference impactSound;
    public EventReference destroySound;

    //dropping this in from the enemy script, lets see if it works!
    [Header("Beam Overcharge")]
    public float maxBeamCharge = 1f;      // how long to beam before it pops
    public float beamChargeRate = 1f;     // charge per second
    private float currentBeamCharge = 0f;
    private float chargePercent = 0f;

    [Header("Glow Settings")]
    public float usualGlowIntensity = 0f;
    public float maxGlowIntensity = 6f;
    public float glowCurveExponent = 2f;
    public float glowDecayRate = 0.4f;

    private Renderer[] renderers;
    private bool isBeingBeamed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //to refund staMana
        player = GameObject.Find("Player");
        playCont = player.GetComponent<PlayerController>();

        finalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(Grow());

        renderers = GetComponentsInChildren<Renderer>();
        UpdateGlow(0f);
    }

    private void Update()
    {
        UpdateGlowDecay();
    }

    private IEnumerator Grow()
    {
        float timer = 0f;
        Vector3 startScale = Vector3.zero;

        while (timer < growTime)
        {
            timer += Time.deltaTime;
            float t = timer / growTime;
            transform.localScale = Vector3.Lerp(startScale, finalScale, t);
            yield return null;
        }

        transform.localScale = finalScale;
    }

    private void OnDisable()
    {
        if (!destroySound.IsNull)
        {
            RuntimeManager.PlayOneShot(destroySound, transform.position);
        }

        playCont.currStaMana = Mathf.Min(playCont.maxStaMana, playCont.currStaMana + gooRefundAmt);
    }

    public void ApplyBeam(float deltaTime)
    {
        isBeingBeamed = true;

        if (currentBeamCharge >= maxBeamCharge) return;

        currentBeamCharge += beamChargeRate * deltaTime;
        currentBeamCharge = Mathf.Min(currentBeamCharge, maxBeamCharge);

        //the visual update is centralized in UpdateGlowDecay()

        if (currentBeamCharge >= maxBeamCharge)
        {
            //overcharged! pop
            Health h = GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(h.maxHealth);   // use Health flow if present
            }
            else
            {
                Destroy(gameObject);         // fallback
            }
        }
    }

    private void UpdateGlowDecay()
    {
        //decay like enemies when not currently beamed
        if (!isBeingBeamed && currentBeamCharge > 0f)
        {
            currentBeamCharge -= glowDecayRate * Time.deltaTime;
            currentBeamCharge = Mathf.Max(currentBeamCharge, 0f);
        }

        chargePercent = (maxBeamCharge > 0f) ? currentBeamCharge / maxBeamCharge : 0f;
        UpdateGlow(chargePercent);
        isBeingBeamed = false;
    }

    private void UpdateGlow(float glowChargePercent)
    {
        if (renderers == null || renderers.Length == 0) return;

        float curved = Mathf.Pow(glowChargePercent, glowCurveExponent);
        float intensity = Mathf.Lerp(usualGlowIntensity, maxGlowIntensity, curved);

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                if (!mat.HasProperty("_GlowIntensity")) continue;
                mat.SetFloat("_GlowIntensity", intensity);
            }
        }
    }
}
