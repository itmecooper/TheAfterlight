using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Experimental.GlobalIllumination;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public CameraController camController;
    public GameObject babyVisionCam;

    [Header("Stats")]
    public float health = 100f;
    public float maxHealth = 100f;
    public float regenRate = 1f;
    public bool passiveHeal = true;
    public float attackDamage = 10f;
    public bool canAttack = true;
    public float attackCooldown = 1f;
    public float killcount = 0f;

    [Header("Audio")]
    public EventReference hurtEvent;
    public EventReference jumpEvent;
    public EventReference landEvent;
    private bool wasGroundedLastFrame = false;
    private GooGun gooGun;
    public EventReference fakeGunEquipEvent;
    public EventReference sfxExplorationReward;
    public EventReference sfxCombatReward;
    public EventReference sfxHealthRefill;
    public EventReference sfxAmmoRefill;

    [Header("Move")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float backpedalSpeedMultiplier = .7f;
    public float backStrafeSpeedMultiplier = .85f;
    public float turnSpeedSensitivity = 8f;
    public float verticalLookSpeed = 8f;
    public float accel = 30f;
    public float inputDeadZone = .01f;
    public float decel = 45f;
    public float hardStopSpeed = .05f;
    public Vector3 currMoveVelocity;
    private Vector3 horizontalVelocity;
    public bool wantsToRun = false;
    //public bool canRun = true;
    public bool isRunning = false;
    public bool lockLook = false;

    private Animator anim;

    [Header("Crouch")]
    public bool isCrouching = false;
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchSpeedMultiplier = 0.8f;
    public float UNUSEDcrouchTransitionSpeed = 0.6f;

    [Header("Jump")]
    public bool isGrounded;
    public LayerMask groundMask;
    public Vector3 fallVelocity;
    //public float gravity = -9.81f;
    public float jumpHeight = 1f;
    private float gravityUp = -20f;
    private float gravityDown = -40f;
    private float coyoteTime = 0.1f;
    private float jumpBufferTime = 0.1f;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    [Header("Flashlight")]
    public GameObject torch;
    public GameObject torchLightCone;
    public GameObject playerEyesCam;
    public Vector3 torchRot;
    public Vector3 eyesRot;
    public LayerMask torchRayMask;

    [Header("Weapon")]
    public GameObject weapon;
    public float maxStaMana = 80f;
    public float currStaMana;
    public float playerReach = 3f;
    public float staManaRegenRate = 1f;
    public float staManaRegenDelay = 1f;
    //public Animator weaponAnimator;

    [Header("Lantern")]
    public bool wantsToLantern = false;
    public bool hasLantern = false;
    public float lanternCooldown = 8f;
    public float lanternDuration = 6f;
    private float nextAllowedLanternTime;
    public GameObject placeholderLantern;
    //public ObjectMover bigDoor;
    public ObjectRotator clockHinge;

    [Header("Pickups")]
    public int healthRefillCount;
    public int ammoRefillHeldCount;
    public int explorationRewardCount;
    public int combatRewardCount;
    public float healthDropValue;
    public float ammoDropValue;
    public float explorationRewardValue;
    public float combatRewardValue;
    public TMP_Text ammoDropUICount;
    public TMP_Text healthDropUICount;
    public GameObject pickupPopupContainer;
    public TMP_Text pickupPopupTxt;
    public SubtitleTrigger gunPickupTrigger;
    public SubtitleTrigger lanternPickupTrigger;

    [Header("Other")]
    public GameObject overheadLight;
    public LayerMask clickableRayMask;
    public Image damagedUIEffect;
    public Image healedUIEffect;
    public Slider healthSlider;

    private PlayerUI playerUI;


    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        torchRot = torch.transform.localEulerAngles;
        eyesRot = playerEyesCam.transform.localEulerAngles;
        Cursor.lockState = CursorLockMode.Locked;

        passiveHeal = true; //no more food system
        healthSlider.maxValue = maxHealth;

        anim = GetComponentInChildren<Animator>();
        playerUI = FindAnyObjectByType<PlayerUI>();

        if (weapon != null)
        {
            // if GooGun is on the weapon object:
            gooGun = weapon.GetComponent<GooGun>();
        }

        nextAllowedLanternTime = Time.time;
    }

    void Update()
    {
        //wow controls!! malia look here!!!!!!
        float forward = Input.GetAxisRaw("Vertical");
        float right = Input.GetAxisRaw("Horizontal");
        float mouseXInput = Input.GetAxis("Mouse X");
        float mouseYInput = Input.GetAxis("Mouse Y");
        wantsToRun = Input.GetKey(KeyCode.LeftShift);
        bool jump = Input.GetButtonDown("Jump");
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        bool wantsToCrouchTransition = Input.GetKeyDown(KeyCode.LeftControl);
        wantsToLantern = Input.GetKey(KeyCode.Q);

        //float pitchYaw = Input.GetAxis("PitchYaw");

        ApplyGravity(jump);
        AimingRay();
        ManageHealth();

        if (wantsToCrouchTransition)
        {
            ToggleCrouch();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (healthRefillCount > 0 && health < maxHealth)
            {
                UseHeldHealthDrop();
            }
        }


        if (Input.GetKeyDown(KeyCode.R)) 
        {
            if (ammoRefillHeldCount > 0 && currStaMana < maxStaMana)
            {
                UseHeldAmmoDrop();
            }
        }

        if (wantsToLantern)
        {
            TryLantern();
        }

        HandleMovement(forward, right, wantsToRun);
        Vector3 totalMove = horizontalVelocity + fallVelocity;
        controller.Move(totalMove * Time.deltaTime);


        //left to right visuals (yaw)
        if (!lockLook)
        {
            transform.Rotate(Vector3.up, mouseXInput * turnSpeedSensitivity);
        }
        //transform.Rotate(Vector3.up, mouseXInput * turnSpeedSensitivity * Time.deltaTime);

        //up and down visuals
        //moved to cam controller
        //eyesRot.x += verticalLookSpeed * mouseYInput * -1 * Time.deltaTime;
        //eyesRot.x = Mathf.Clamp(eyesRot.x, -80, 80);
        //playerEyesCam.transform.localEulerAngles = eyesRot;

        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    torchLightCone.SetActive(!torchLightCone.gameObject.activeSelf);
        //}

    }

    public void HandleMovement(float forward, float right, bool wantsToRun)
    {

        Vector3 inputRaw = new Vector3(right, 0f, forward);

        bool noInput = inputRaw.sqrMagnitude < inputDeadZone * inputDeadZone;

        //stop instantly (or it did until I changed it some) //just dont fucking slide okay
        if (noInput)
        {
            //horizontalVelocity = Vector3.zero;
            //currMoveVelocity = Vector3.zero;

            //very quick ease out instead of instant stop
            //movement feels DEAD without this
            float r = 1f - Mathf.Exp(-decel * Time.deltaTime);
            currMoveVelocity = Vector3.Lerp(currMoveVelocity, Vector3.zero, r);

            //hard snap when smol
            if (currMoveVelocity.magnitude < hardStopSpeed)
                currMoveVelocity = Vector3.zero;

            horizontalVelocity = currMoveVelocity;

            anim.SetFloat("Speed", 0f);
            return;
        }

        Vector3 inputDir = inputRaw.normalized;

        bool directlyBackpedaling = (forward < 0f && Mathf.Abs(right) < 0.1f);
        bool backtrackOrBackStrafing = (forward < 0f);
        isRunning = CanRun(forward, right, directlyBackpedaling);

        //no running backwards, but allows strafing, just no straight back
        float targetMoveSpeed;
        
        if (isCrouching) //moved this up bc this is slower
        {
            targetMoveSpeed = walkSpeed * crouchSpeedMultiplier;
        }
        else if (directlyBackpedaling)
        {
            //canRun = false;
            targetMoveSpeed = walkSpeed * backpedalSpeedMultiplier;
            //targetMoveSpeed = walkSpeed * (Mathf.Max(Mathf.Abs(right), backpedalSpeedMultiplier));
        }
        else if (backtrackOrBackStrafing)
        {
            //targetMoveSpeed = walkSpeed * backStrafeSpeedMultiplier;
            //cuts run by 20%, then 20% again. cuts walk by 20%
            targetMoveSpeed = (isRunning ? (runSpeed * backStrafeSpeedMultiplier) : walkSpeed) * backStrafeSpeedMultiplier;

        }
        else
        {
            targetMoveSpeed = isRunning ? runSpeed : walkSpeed;
        }

        Vector3 targetVelocity = transform.TransformDirection(inputDir) * targetMoveSpeed;

        ////currMoveVelocity = Vector3.Lerp(currMoveVelocity, targetVelocity, 0.4f); //.2-.3 is "FPS standard" but it feels too slidy
        ////this actually causes the extra half step by taking so long
        ////yoinked something a bit better
        //if (inputDir.sqrMagnitude < 0.0001f)
        //{
        //    currMoveVelocity = Vector3.zero;   //NO slide only stop
        //}
        //else
        //{
        //    float accel = 30f; //tune
        //    float t = 1f - Mathf.Exp(-accel * Time.deltaTime);
        //    currMoveVelocity = Vector3.Lerp(currMoveVelocity, targetVelocity, t); //preserves smooth start
        //}
        //controller.Move(currMoveVelocity * Time.deltaTime);

        //smooth start
        float t = 1f - Mathf.Exp(-accel * Time.deltaTime);
        currMoveVelocity = Vector3.Lerp(currMoveVelocity, targetVelocity, t);

        //keep zero Y for horizontal velocity
        currMoveVelocity.y = 0f;

        horizontalVelocity = currMoveVelocity;

        //if (Time.frameCount % 2 == 0)
        //    Debug.Log($"Axis H={right:F3} V={forward:F3}");

        //doesnt actually change anything, this is just the trigger for the animator to know to change the animation
        float animVelo;
        if (isRunning)
        {
            animVelo = 3f;
        }
        else if (isCrouching)
        {
            animVelo = .5f;
        }
        else
        {
            animVelo = 1f;
        }
        //anim.SetFloat("Speed", targetVelocity.magnitude > 0 ? (isRunning ? 3 : 1) : 0);
        anim.SetFloat("Speed", targetVelocity.magnitude > 0 ? animVelo : 0);
    }

    public bool CanRun(float forward, float right, bool directlyBackpedaling)
    {
        bool isMoving = Mathf.Abs(forward) > 0.1f || Mathf.Abs(right) > 0.1f;

        if (wantsToRun && isMoving && !directlyBackpedaling)
        {
            if (isCrouching)
            {
                ToggleCrouch();
            }

            if (isGrounded)
            {
                return true;
            }
            else
            {
                //if not grounded but already running....
                return isRunning;
            }
        }
        
        return false;

    }

    public void ApplyGravity(bool jump)
    {
        isGrounded = CheckGrounded();
        wasGroundedLastFrame = isGrounded;

        //STOP: coyote time ("counts down after leaving ground")
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        //jump buffer ("stores jump input for a short time")
        if (jump)
        {
            if (isCrouching)
            {
                ToggleCrouch();
            }

            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        //jump jump jump jump (w/ spicy new internet code)
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            //calculate initial velocity needed to reach jumpHeight
            fallVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityUp);
            jumpBufferCounter = 0f; //"consume buffer"

            if (!jumpEvent.IsNull)
            {
                EventInstance jumpInstance = RuntimeManager.CreateInstance(jumpEvent);
                jumpInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
                jumpInstance.setParameterByName("VoiceGender", (int)VoiceSelectionMenu.SelectedVoiceGender);
                jumpInstance.setParameterByName("VoicePitch", VoiceSelectionMenu.SelectedVoicePitch);
                jumpInstance.start();
                jumpInstance.release();
            }
        }

        //gravity (different for jumping vs falling)
        if (fallVelocity.y > 0) //rising
            fallVelocity.y += gravityUp * Time.deltaTime;
        else //falling
            fallVelocity.y += gravityDown * Time.deltaTime;

        //set fall velocity on touchdown
        if (isGrounded && fallVelocity.y < 0)
        {
            fallVelocity.y = -2f; //little push to ground force
        }

        //controller.Move(fallVelocity * Time.deltaTime);
        //keep xz zero (gravity only here)
        fallVelocity.x = 0f;
        fallVelocity.z = 0f;
    }

    public bool CheckGrounded()
    {
        //    float radius = controller.radius * 0.9f;
        //    float groundedOffset = 0.1f;

        //    Vector3 point1 = transform.position + Vector3.up * 0.1f;
        //    Vector3 point2 = transform.position + Vector3.down * (controller.height / 2f - radius + groundedOffset);

        //    return Physics.CheckCapsule(point1, point2, radius, groundMask, QueryTriggerInteraction.Ignore);

        float radius = controller.radius * 0.9f;

        // World-space position of the bottom sphere's center of the CharacterController
        // Works even if controller.center is not zero.
        Vector3 bottom = transform.position
                       + Vector3.up * (controller.center.y - controller.height / 2f + radius);

        // Start a little bit above the bottom
        Vector3 origin = bottom + Vector3.up * 0.05f;

        // How far down we probe for ground
        float groundCheckDistance = controller.skinWidth + 0.2f;

        RaycastHit hit;
        bool hasHit = Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out hit,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (!hasHit)
            return false;

        // Optional: ignore very steep slopes
        float maxSlopeAngle = controller.slopeLimit; // in degrees
        float angle = Vector3.Angle(hit.normal, Vector3.up);

        return angle <= maxSlopeAngle;
    }

    void ToggleCrouch()
    {
        //Debug.Log("Can stand: " + CanStand());

        if (!isCrouching)
        {

            float centerOffset = (standHeight - crouchHeight) / 2f;
            controller.center -= new Vector3(0, centerOffset, 0);
            controller.height = crouchHeight;
            //controller.height = Mathf.Lerp(standHeight, crouchHeight, Time.deltaTime * crouchTransitionSpeed);
            //controller.center -= new Vector3(0, crouchHeight, 0);

            //moving to cam controller, testing:
            //playerEyesCam.transform.position -= new Vector3(0, .3f, 0);
            anim.speed = crouchSpeedMultiplier;
            isCrouching = true;
            //Debug.Log("Squat!");

            return;
        }
        
        if (CanStand())
        {
            float centerOffset = (standHeight - crouchHeight) / 2f;
            controller.center += new Vector3(0, centerOffset, 0);
            controller.height = standHeight;
            //controller.center += new Vector3(0, crouchHeight, 0);

            //moving to cam controller, testing:

            //playerEyesCam.transform.position += new Vector3(0, .3f, 0);
            anim.speed = 1f;
            isCrouching = false;
            //Debug.Log("Stood up!");
        }

    }

    bool CanStand()
    {
        //dont touch our own collider
        //float radius = Mathf.Max(0.0f, controller.radius - 0.02f);
        //float standDiff = standHeight - crouchHeight;

        //when controller.center.y = height/2, - oh no this is NOT true, the center is at 0!!!!
        //transform.position is at bottom of capsule (FEET) (this might be a lie!!!)
        //Vector3 bottom = transform.position + Vector3.up * radius;
        //Vector3 top = transform.position + Vector3.up * (standHeight - radius);

        Vector3 bottom = transform.position + controller.center - Vector3.up * (controller.height / 2f) + Vector3.up * controller.radius;
        Vector3 top = bottom + Vector3.up * (standHeight - controller.radius * 2f);

        //true if head space clear
        bool blocked = Physics.CheckCapsule(bottom, top, controller.radius, groundMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }
    
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //grounded check gizmos
        //Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, controller.radius * 0.9f);
        //Gizmos.DrawWireSphere(transform.position + Vector3.down * (controller.height / 2f - (controller.radius * 0.9f) + .01f), controller.radius * 0.9f);
        //Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.down * (controller.height / 2f - (controller.radius * 0.9f) + .01f));

        //new grounded check gizmos

        float radius = controller.radius * 0.9f;

        // Bottom of the controller's capsule (sphere center)
        Vector3 bottom = transform.position
                       + Vector3.up * (controller.center.y - controller.height / 2f + radius);

        // Origin and end of the ground check
        Vector3 origin = bottom + Vector3.up * 0.05f;
        float groundCheckDistance = controller.skinWidth + 0.2f;
        Vector3 end = origin + Vector3.down * groundCheckDistance;

        Gizmos.color = Color.yellow;

        // Draw the cast line
        Gizmos.DrawLine(origin, end);

        // Sphere where the cast starts
        Gizmos.DrawWireSphere(origin, radius);

        // Sphere at the furthest point we consider for ground
        Gizmos.DrawWireSphere(end, radius);

        //Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.51f * playerScale, 0.5f * playerScale);
    }

    public void AimingRay()
    {
        Ray ray;
        Vector3 hitPoint;

        var cam = camController.worldCamera;
        ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, torchRayMask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = ray.GetPoint(hit.distance);
        }
        else
        {
            hitPoint = ray.GetPoint(100);
        }

        //moved E check inside....
        Ray clickableRay;
        clickableRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit clickableHit;

        if (Physics.Raycast(clickableRay, out clickableHit, playerReach, clickableRayMask, QueryTriggerInteraction.Ignore))
        {
            Pickup pickup = clickableHit.collider.GetComponent<Pickup>();

            if (pickup)
            {
                playerUI.ShowButtonPrompt(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("Player hit E on: " + clickableHit.collider.gameObject.name);

                    if (pickup != null)
                    {
                        CollectPickup(pickup);
                    }
                }
            }
            else
            {
                playerUI.ShowButtonPrompt(false);
            }
        }
        else
        {
            playerUI.ShowButtonPrompt(false);
        }
    }

    public void CollectPickup(Pickup pickup)
    {
        switch (pickup.pickupType)
        {
            case Pickup.PickupType.SceneExit:
                SceneExitDestination sceneExit = pickup.GetComponent<SceneExitDestination>();

                if (sceneExit == null)
                {
                    Debug.LogError("SceneExit pickup triggered, but no SceneExitDestination component was found!");
                    break;
                }

                if (SceneLoader.Instance == null)
                {
                    Debug.LogError("SceneLoader.Instance is null — did you forget to put a SceneLoader in the starting scene?");
                    break;
                }

                SceneLoader.Instance.LoadScene(sceneExit.nextSceneName);
                return;

            case Pickup.PickupType.HealthRefill:
                healthRefillCount++;
                break;

            case Pickup.PickupType.AmmoRefill:
                ammoRefillHeldCount++;
                break;

            case Pickup.PickupType.ExplorationReward:
                explorationRewardCount++;
                ApplyExplorationReward();
                if (!sfxExplorationReward.IsNull)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(sfxExplorationReward, transform.position);
                }
                break;

            case Pickup.PickupType.CombatReward:
                combatRewardCount++;
                ApplyCombatReward();
                if (!sfxCombatReward.IsNull)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(sfxCombatReward, transform.position);
                }
                break;

            case Pickup.PickupType.FakeGun:
                weapon.SetActive(true);
                gunPickupTrigger.StartSequence();

                if (!fakeGunEquipEvent.IsNull)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(fakeGunEquipEvent, transform.position);
                }
                break;

            case Pickup.PickupType.Lantern:
                hasLantern = true;
                //bigDoor.Move();
                if (clockHinge != null)
                {
                    clockHinge.Open();
                    Light clockLight = clockHinge.GetComponentInChildren<Light>();
                    clockLight.intensity = 0;
                }

                if (lanternPickupTrigger != null)
                {
                    lanternPickupTrigger.StartSequence();
                }

                //if (!fakeGunEquipEvent.IsNull)
                //{
                //    FMODUnity.RuntimeManager.PlayOneShot(fakeGunEquipEvent, transform.position);
                //}
                break;

            default:
                Debug.Log("Messed up your switch statement for the pickups probably");
                break;
        }

        if (pickup != null && !pickup.sfxOnPickup.IsNull)
        {
            FMODUnity.RuntimeManager.PlayOneShot(pickup.sfxOnPickup, pickup.transform.position);
        }

        UpdatePickupUI();
        Destroy(pickup.gameObject);

    }

    public void UpdatePickupUI()
    {
        healthDropUICount.text = healthRefillCount.ToString();
        ammoDropUICount.text = ammoRefillHeldCount.ToString();
    }

    public void TryLantern()
    {
        if(hasLantern && Time.time >= nextAllowedLanternTime)
        {
            placeholderLantern.SetActive(true);
            babyVisionCam.SetActive(true);

            nextAllowedLanternTime = Time.time + lanternCooldown;

            Invoke(nameof(TurnOffLantern), lanternDuration);
        }
    }

    public void TurnOffLantern()
    {
        placeholderLantern.SetActive(false);
        babyVisionCam.SetActive(false);
    }

    public void UseHeldHealthDrop()
    {
        if (!sfxHealthRefill.IsNull)
        {
            RuntimeManager.PlayOneShot(sfxHealthRefill, transform.position);
        }

        health = Mathf.Min(maxHealth, health + healthDropValue);
        healthRefillCount--;
        UpdatePickupUI();
        StartCoroutine(HealUIEffect());

    }

    public void UseHeldAmmoDrop()
    {
        if (!sfxAmmoRefill.IsNull)
        {
            RuntimeManager.PlayOneShot(sfxAmmoRefill, transform.position);
        }

        currStaMana = Mathf.Min(maxStaMana, currStaMana + ammoDropValue);
        ammoRefillHeldCount--;
        UpdatePickupUI();
    }

    // (A green canister will make your TORCH’s resin-due refill faster,
    //while blue ones will increase your TORCH’s maximum resin-due reservoir.)

    public void ApplyExplorationReward()
    {
        staManaRegenRate += explorationRewardValue;
        pickupPopupContainer.SetActive(true);
        pickupPopupTxt.text = "Resin Regen Increased";
    }

    public void ApplyCombatReward()
    {
        maxStaMana += combatRewardValue;
        pickupPopupContainer.SetActive(true);
        pickupPopupTxt.text = "Max Regen Increased";
        //currStaMana += Mathf.Min(maxStaMana, currStaMana + blueCanisterValue); //give them that little boost?
    }

    public void ManageHealth()
    {
        //float damageDebug = 5f;
        //float healDebug = 15f;

        //debug damage
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    if(health <= damageDebug)
        //    {
        //        health = 0;
        //        Destroy(gameObject);
        //    } 
        //    else
        //    {
        //        health -= damageDebug;
        //        StartCoroutine(DamageUIEffect());
        //    }
        //}

        ////debug heal
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    if (health >= (maxHealth - healDebug))
        //    {
        //        health = maxHealth;
        //    }
        //    else
        //    {
        //        health += healDebug;
        //    }
        //}

        if (passiveHeal)
        {
            if (health < maxHealth)
            {
                health += (regenRate * Time.deltaTime);
            }
            else
            {
                health = maxHealth;
            }
        }

        //if (healthTextUI != null)
        //{
        //    string displayedHealth = health.ToString("F0");
        //    healthTextUI.text = "Health: " + displayedHealth;
        //}

        healthSlider.value = health;
    }

    public void TakeDamage(float damageAmtReceived)
    {
        health -= damageAmtReceived;

        // Play hurt sound
        if (!hurtEvent.IsNull)
        {
            EventInstance instance = RuntimeManager.CreateInstance(hurtEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            instance.setParameterByName("VoiceGender", (int)VoiceSelectionMenu.SelectedVoiceGender);
            instance.setParameterByName("VoicePitch", VoiceSelectionMenu.SelectedVoicePitch);
            instance.start();
            instance.release();
        }

        if (health <= 0)
        {
            if (gooGun != null) gooGun.HandlePlayerDeath();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //Destroy(gameObject);
        }

        //show player was hurt?
        //id like the screen to darken, then turn red and fade back
        //for now itll just flash red.

        StartCoroutine(DamageUIEffect());
    }

    public void HealFromDamage(float damageAmtHealed)
    {
        health += damageAmtHealed;

        if (health >= maxHealth)
        {
            health = maxHealth;
        }

        //show player was healed?

        StartCoroutine(HealUIEffect());
    }

    private IEnumerator DamageUIEffect()
    {
        damagedUIEffect.gameObject.SetActive(true);

        yield return new WaitForSeconds(.2f);

        damagedUIEffect.gameObject.SetActive(false);
    }

    private IEnumerator HealUIEffect()
    {
        healedUIEffect.gameObject.SetActive(true);

        yield return new WaitForSeconds(.2f);

        healedUIEffect.gameObject.SetActive(false);
    }

}
