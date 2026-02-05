using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Hookups")]
    public Camera worldCamera;       // Main camera
    public Camera weaponCamera;      // Overlay camera for gun
    public Camera babyVisionCam;
    public PlayerController controller;
    public Transform cameraRig;

    [Header("FOV")]
    public float baseFOV = 60f;
    public float sprintFOVOffset = +8f;
    public float crouchFOVOffset = -3f;
    public float fovSmooth = 8f;
    public float weaponCameraOffset = 8f;
    public bool isRunning = false;

    [Header("Tilt")]
    public float maxTilt = 5f;
    public float tiltSmooth = 6f;
    public float leanInSpeed = 10f;
    public float returnSpeed = 16f;

    [Header("Crouch")]
    public bool isCrouching;
    public float standCameraHeight = 0f;
    public float crouchCameraHeight = -.2f;
    public float crouchSmoothSpeed = 8f;
    private float targetHeight;

    [Header("Crouch Dip Animation")]
    public float crouchDipAnimAmount = 0.05f;
    public float crouchDipAnimSpeed = 8f;
    public float crouchPitchAnimAmount = 2f;
    public float crouchDipAnimOffset = 0.5f;
    public float crouchPitchAnimOffset = 5f;

    [Header("Headbob")]
    public float bobFrequency = 6f;
    public float bobAmplitude = 0.05f;
    public float bobSmooth = 10f;
    //public float bobRunMultiplier = 2f;
    public float runBobAmplitudeMultiplier = 1.5f; //1.5-2 ish
    public float crouchBobAmplitudeMultiplier = .5f;
    public float idleBobAmplitudeMultiplier = .3f;
    public float currAmplitude;
    private float bobTimer;
    private float currBobOffset;
    private bool isBobbing;
    private float currAmplitudeMultiplier = 1f;

    [Header("Landing Dip")]
    public float landDipAmount = 0.2f;
    public float landDipSpeed = 6f;
    private float landDipTarget;

    private Vector3 originalLocalPos;
    private float targetFOV;
    private float currentTilt;
    private float landDipOffset;
    private bool wasGroundedLastFrame; //i replaced this var in the player controller...

    private float pitch = 0f;
    private float camSmoothSpeed = 8f;
    private Vector3 cameraVelocity = Vector3.zero;

    void Start()
    {
        //Debug.Log($"CameraController started on {name}, ID {GetInstanceID()}");
        if (!worldCamera) worldCamera = GetComponent<Camera>();
        //if (!weaponCamera) weaponCamera = GetComponentInChildren<Camera>();
        if (!weaponCamera)
        {
            Debug.LogError("Unassigned weapon cam bro!");
        }
        if (!babyVisionCam)
        {
            Debug.LogWarning("Unassigned babyvis cam bro!");
        }
        if (!controller) controller = GetComponentInParent<PlayerController>();
        if (cameraRig == null) cameraRig = transform;

        if (weaponCamera)
        {
            //"Sync FOV at start"
            weaponCamera.fieldOfView = worldCamera.fieldOfView + weaponCameraOffset;
        }

        originalLocalPos = cameraRig.localPosition;
        targetFOV = baseFOV;
        targetHeight = standCameraHeight;
    }

    void LateUpdate()
    {
        //moved from player controller...
        HandleRotation();

        //no longer internet code as it has been rewritten, again

        HandleFOV();
        HandleTilt();
        HandleCrouch();
        HandleCrouchDip();
        HandleLandingDip();

        //Vector3 targetPos = originalLocalPos;

        ////crouch
        //targetPos.y += targetHeight;

        ////crouchDip
        //targetPos.y += crouchDipAnimOffset;

        ////landDip
        //targetPos.y += landDipOffset;

        ////headbob
        //targetPos += GetHeadbobOffset();

        ////apply everything to cam transform
        //cameraRig.localPosition = targetPos;
        //cameraRig.localRotation = Quaternion.Euler(pitch, 0, currentTilt);

        float targetY = targetHeight + crouchDipAnimOffset + landDipTarget;
        Vector3 lightAdjustmentsTargetPos = new Vector3(originalLocalPos.x, originalLocalPos.y + targetY, originalLocalPos.z);

        //smooth camera pos
        //Vector3 smoothedBasePos = Vector3.Lerp(cameraRig.localPosition, lightAdjustmentsPos, Time.deltaTime * camSmoothSpeed);
        //Vector3 smoothedBasePos = Vector3.Lerp(
        //    cameraRig.localPosition - GetHeadbobOffset(),
        //    lightAdjustmentsTargetPos,
        //    Time.deltaTime * camSmoothSpeed);

        Vector3 bob = GetHeadbobOffset();

        Vector3 smoothedBasePos = Vector3.SmoothDamp(
            cameraRig.localPosition - bob,
            lightAdjustmentsTargetPos,
            ref cameraVelocity, // Vector3 cameraVelocity; at class scope
            1f / camSmoothSpeed
        );


        //add the head bob as it is a big adjustment (high-frequency) and not a little one
        cameraRig.localPosition = smoothedBasePos + bob;

        //apply cam rotation (pitch & tilt)
        cameraRig.localRotation = Quaternion.Euler(pitch, 0f, currentTilt);


        //BUGFIXING
        ////snap isn't coming from the camera, ugh
        //if (Vector3.Distance(cameraRig.localPosition, targetPos) > 0.01f)
        //{
        //    Debug.LogWarning($"Camera snap detected! {cameraRig.localPosition} -> {targetPos}");
        //}

        //Quaternion lastRot = cameraRig.localRotation;

        //if (Quaternion.Angle(lastRot, cameraRig.localRotation) > 1f)
        //{
        //    Debug.LogWarning($"Camera snap ROT {Quaternion.Angle(lastRot, cameraRig.localRotation)}°");
        //}

    }

    void HandleRotation()
    {
        if (controller.lockLook) return;

        float mouseYInput = Input.GetAxis("Mouse Y");
        //pitch += mouseYInput * -controller.verticalLookSpeed * Time.deltaTime;
        pitch += mouseYInput * -controller.verticalLookSpeed;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
    }

    void HandleFOV()
    {
        //I dont think I need to check move velo here due to the CanRun method but im leaving it for now
        isRunning = controller.isRunning && controller.currMoveVelocity.magnitude > 0.1f;
        float fov = baseFOV;

        if (isCrouching)
        {
            fov += crouchFOVOffset;
        }
        else if (isRunning)
        {
            fov += sprintFOVOffset;
        }

        targetFOV = fov;

        //targetFOV = isRunning ? (baseFOV + sprintFOVOffset) : baseFOV;
        //targetFOV = isCrouching ? (baseFOV + crouchFOVOffset) : baseFOV;

        worldCamera.fieldOfView = Mathf.Lerp(worldCamera.fieldOfView, targetFOV, Time.deltaTime * fovSmooth);
        if (babyVisionCam != null)
        {
            babyVisionCam.fieldOfView = Mathf.Lerp(worldCamera.fieldOfView, targetFOV, Time.deltaTime * fovSmooth);
        }
    }

    void HandleTilt()
    {
        //float strafe = Input.GetAxis("Horizontal");
        float strafe = Input.GetAxisRaw("Horizontal");
        float targetTilt = -strafe * maxTilt;
        //currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmooth);


        float speed = (Mathf.Abs(strafe) > 0.01f) ? leanInSpeed : returnSpeed;
        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, t);
    }

    void HandleCrouch()
    {
        isCrouching = controller.isCrouching;

        //crouch
        float crouchTarget = isCrouching ? crouchCameraHeight : standCameraHeight;
        targetHeight = Mathf.Lerp(targetHeight, crouchTarget, Time.deltaTime * crouchSmoothSpeed);
    }

    void HandleCrouchDip()
    {
        float targetDip = controller.isCrouching ? -crouchDipAnimAmount : 0f;
        float targetPitch = controller.isCrouching ? crouchPitchAnimAmount : 0f;

        crouchDipAnimOffset = Mathf.Lerp(crouchDipAnimOffset, targetDip, Time.deltaTime * crouchDipAnimSpeed);
        crouchPitchAnimOffset = Mathf.Lerp(crouchPitchAnimOffset, targetPitch, Time.deltaTime * crouchDipAnimSpeed);
    }

    Vector3 GetHeadbobOffset()
    {
        //if (isBobbing)
        //{
        //    //internet code interjection
        //    float horizontalSpeed = new Vector3(controller.currMoveVelocity.x, 0, controller.currMoveVelocity.z).magnitude;
        //    float speedMultiplier = horizontalSpeed / controller.walkSpeed; //ratio of current speed to walk speed

        //    bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;
        //    isBobbing = false; //only advance on trigger
        //}
        //else if (controller.currMoveVelocity.magnitude > 0.1f && controller.isGrounded)
        //{
        //    //"subtle sway between steps" in theory an idle bob
        //    //I thought i knew what I was doing, but removing this breaks the shit,
        //    //so i guess it is the only functional line here
        //    bobTimer += Time.deltaTime * bobFrequency * 0.2f;
        //}

        //currAmplitude = bobAmplitude;
        //if (isRunning) currAmplitude *= runBobAmplitudeMultiplier;
        //if (isCrouching) currAmplitude *= crouchBobAmplitudeMultiplier;

        //float yOffset = Mathf.Sin(bobTimer * Mathf.PI * 2f) * currAmplitude;
        //currBobOffset = Mathf.Lerp(currBobOffset, yOffset, Time.deltaTime * 10f);
        //return new Vector3(0, currBobOffset, 0);


        //MY CODE IS BROKEN
        //INTERNET CODE:

        //finds horizontal speed and normalized speed in [0,1]
        float horizontalSpeed = new Vector3(controller.currMoveVelocity.x, 0f, controller.currMoveVelocity.z).magnitude;
        //using walkSpeed as the baseline for "1 step per bob"
        //clamped to avoid >1 causing extreme freq and make the dude vibrate up n down
        float speedRatio = (controller.walkSpeed > 0f) ? Mathf.Clamp01(horizontalSpeed / controller.walkSpeed) : 0f;

        //bobTimer counts up smoothly only while grounded (idle progress when near zero)
        float minIdleFactor = 0.12f; //how much the bob moves when almost stopped //the idle ish
        float timeScale = Mathf.Lerp(minIdleFactor, 1f, speedRatio); //0.12 -> 1.0 % based on moving speed clamped above
        bobTimer += Time.deltaTime * bobFrequency * timeScale;

        //reset bobtimer so things dont eventually explode
        //could this be causing the jerkiness?
        //if (bobTimer > 1000000f) bobTimer %= 1f;

        //creating amplitude multiplier, blending to avoid snap
        float targetAmpMult = bobAmplitude;
        //prioritize running before crouching
        if (controller.isRunning && horizontalSpeed > 0.1f)
        {
            targetAmpMult *= runBobAmplitudeMultiplier;
        }
        else if (controller.isCrouching && horizontalSpeed > 0.01f)
        {
            targetAmpMult *= crouchBobAmplitudeMultiplier;
        }
        else if (horizontalSpeed < 0.01f)//idle
        {
            //targetAmpMult = idleBobAmplitudeMultiplier;
            targetAmpMult *= idleBobAmplitudeMultiplier;
        }

        //targetAmpMult *= speedRatio;
        //caped to prevent explosions
        targetAmpMult = Mathf.Clamp(targetAmpMult, 0f, 1f);

        currAmplitudeMultiplier = Mathf.Lerp(currAmplitudeMultiplier, targetAmpMult, Time.deltaTime * 6f);

        //find target bob (a Sine Wave!! ahh scary) and smooth towards it
        float yTarget = Mathf.Sin(bobTimer * Mathf.PI * 2f) * targetAmpMult;
        currBobOffset = Mathf.Lerp(currBobOffset, yTarget, Time.deltaTime * bobSmooth);

        return new Vector3(0f, currBobOffset, 0f);
    }

    public void TriggerFootstep()
    {
        //Debug.Log("Triggered Footsteps from Cam Controller!"); //this is finally getting called
        //isBobbing = true;
        //bobTimer = 0f;

        //in order to be moving downwards on step... blending towards down
        float desiredPhase = 0.75f; //.75 of the sine cycle = downward motion
        float currentPhase = bobTimer % 1f;
        float phaseDifference = Mathf.DeltaAngle(currentPhase * 360f, desiredPhase * 360f) / 360f;
        bobTimer += phaseDifference * 0.2f; //small correction 20% towards alignment
    }

    void HandleLandingDip()
    {
        bool isGrounded = controller.isGrounded;

        //if (isGrounded && !wasGroundedLastFrame)
        //{
        //    //just landed
        //    //StopAllCoroutines(); //evil!! why did I leave this?
        //    StartCoroutine(DoLandingDip());
        //    Debug.Log("Landing Dip played... was it supposed to? If you were crouching, probably not!!!!");
        //}

        if (!wasGroundedLastFrame && isGrounded)
        {
            //just landed
            //landDipTarget = -landDipAmount;
            landDipTarget = Mathf.Clamp(landDipTarget, -landDipAmount, 0f);
        }
        else
        {
            //back to zero smoothly
            landDipTarget = Mathf.Lerp(landDipTarget, 0f, Time.deltaTime * landDipSpeed);
        }

        wasGroundedLastFrame = isGrounded;
    }

    //System.Collections.IEnumerator DoLandingDip()
    //{
    //    float t = 0f;
    //    while (t < 1f)
    //    {
    //        landDipOffset = -Mathf.Sin(t * Mathf.PI) * dipAmount;
    //        t += Time.deltaTime * dipSpeed;
    //        yield return null;
    //    }
    //    landDipOffset = 0f;
    //}
}
