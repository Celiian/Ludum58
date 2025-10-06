using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlaneController : MonoBehaviour
{

    public AudioSource audioSource;
    public Animator animator;

    [Header("Input")]
    [Tooltip("The Input Action Asset containing plane controls")]
    public InputActionAsset inputActions;
    
    [Header("Plane Stats")]
    [Tooltip("The amount the throttle ramps up or down.")]
    public float throttleIncrement = 0.1f;

    [Tooltip("The maximum throttle the plane can reach")]
    public float maxThrottle = 1f;

    [Tooltip("How responsive the plane is when rolling, pitching, and yawing")]
    public float responsiveness = 10f;

    [Tooltip("How much the plane banks when yawing (0 = no banking, 1 = full banking)")]
    public float bankingFactor = 0.6f;

    [Tooltip("How quickly the plane returns to level flight")]
    public float levelReturnSpeed = 2f;

    [Tooltip("Maximum roll angle in degrees")]
    public float maxRollAngle = 30f;

    [Tooltip("How much overshoot is allowed before correction kicks in")]
    public float rollOvershootTolerance = 10f;

    [Tooltip("How strong the roll correction force is")]
    public float rollCorrectionStrength = 0.5f;

    [Header("Control Options")]
    [Tooltip("Reverse pitch controls (up becomes down, down becomes up)")]
    public bool reversePitch = false;

    [Header("Crash System")]
    [Tooltip("Fade screen component for crash transitions")]
    public FadeScreen fadeScreen;
    
    [Tooltip("Water respawn position")]
    public Vector3 waterRespawnPosition = new Vector3(-4.53f, 1.4f, 20.47f);
    
    [Tooltip("Fade duration in seconds")]
    public float fadeDuration = 1f;
    
    [Tooltip("How far ahead to predict crash (in seconds)")]
    public float crashPredictionTime = 1f;
    
    [Tooltip("How often to check for crash prediction (in seconds)")]
    public float crashCheckInterval = 0.1f;
    
    [Tooltip("Ground layer mask for crash detection")]
    public LayerMask groundLayerMask = 1 << 6;

    private float currentThrottle = 0f;

    private float currentPitch = 0f;

    private float currentRoll = 0f;

    private float currentYaw = 0f;

    // Game state control
    private bool isGameplayActive = false;
    private bool isCrashing = false;
    private bool isCrashPredicted = false;
    private float lastCrashCheckTime = 0f;

    // Input Actions
    private InputAction rollAction;
    private InputAction pitchAction;
    private InputAction yawAction;
    private InputAction throttleUpAction;
    private InputAction throttleDownAction;

    private float responseModifier {
        get{
            return ( rb.mass / 10f) * responsiveness;
        }
    }

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Initialize input actions
        if (inputActions != null)
        {
            rollAction = inputActions.FindAction("Roll");
            pitchAction = inputActions.FindAction("Pitch");
            yawAction = inputActions.FindAction("Yaw");
            throttleUpAction = inputActions.FindAction("ThrottleUp");
            throttleDownAction = inputActions.FindAction("ThrottleDown");
        }
    }

    private void HandleInputs(){
        // Only handle inputs if gameplay is active and not crashing
        if (!isGameplayActive || isCrashing) return;
        
        // Get input values from Input System
        if (pitchAction != null) 
        {
            float rawPitch = pitchAction.ReadValue<float>();
            currentPitch = reversePitch ? -rawPitch : rawPitch;
        }
        if (yawAction != null) currentYaw = yawAction.ReadValue<float>();
        
        // Calculate automatic banking based on yaw input
        float targetRoll = -currentYaw * bankingFactor; // Negative because left yaw should cause left roll
        
        // Smoothly transition to target roll or return to level
        if (Mathf.Abs(currentYaw) > 0.1f) {
            // Banking when yawing
            currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * responsiveness);
        } else {
            // Return to level flight when not yawing
            //doesn't actually do anything despite the logic being correct for some reason ? ??
            currentRoll = Mathf.Lerp(currentRoll, 0f, Time.deltaTime * levelReturnSpeed);
        }

        // Clamp roll to maximum angle
        currentRoll = Mathf.Clamp(currentRoll, -maxRollAngle, maxRollAngle);
        
        // Handle throttle input
        if (throttleUpAction != null && throttleUpAction.IsPressed()){
            print("Throttle Up");
            currentThrottle += throttleIncrement;
        }
        else if (throttleDownAction != null && throttleDownAction.IsPressed()){
            print("Throttle Down");
            currentThrottle -= throttleIncrement;
        }

        currentThrottle = Mathf.Clamp(currentThrottle, 0f, maxThrottle); // this prevents the plane if maxThrottle is too low
    }

    private void OnEnable()
    {
        // Enable the Plane action map
        if (inputActions != null)
        {
            inputActions.FindActionMap("Plane")?.Enable();
        }
    }

    private void OnDisable()
    {
        // Disable the Plane action map
        if (inputActions != null)
        {
            inputActions.FindActionMap("Plane")?.Disable();
        }
    }

    // Public method to start gameplay (called by MenuManager)
    public void StartGameplay()
    {
        isGameplayActive = true;
    }

    // Public getters for tutorial system
    public float GetCurrentThrottle()
    {
        return currentThrottle;
    }

    public float GetCurrentPitch()
    {
        return currentPitch;
    }

    public float GetCurrentYaw()
    {
        return currentYaw;
    }

    // Public method to toggle pitch reversal
    public void TogglePitchReversal()
    {
        reversePitch = !reversePitch;
    }

    // Public method to set pitch reversal state
    public void SetPitchReversal(bool reversed)
    {
        reversePitch = reversed;
    }

    // Public getter for pitch reversal state
    public bool IsPitchReversed()
    {
        return reversePitch;
    }

    // Collision detection for terrain
    private void OnCollisionEnter(Collision collision)
    {
        // Check if we collided with terrain layer (layer 6 based on the scene data)
        if (collision.gameObject.layer == 6 && !isCrashing)
        {
            StartCoroutine(HandleCrash());
        }
    }

    // Handle crash sequence
    private IEnumerator HandleCrash()
    {
        isCrashing = true;
        
        // Stop plane physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // If crash was already predicted, we're already fading - just wait for actual collision
        if (isCrashPredicted)
        {
            // Wait for the predicted crash time to complete
            yield return new WaitForSeconds(crashPredictionTime);
        }
        else
        {
            // Start fade to black immediately
            yield return StartCoroutine(FadeToBlack());
        }
        
        // Teleport to water position
        TeleportToWater();
        
        // Reset plane state
        ResetPlaneState();
        
        // Fade back in
        yield return StartCoroutine(FadeFromBlack());
        
        isCrashing = false;
        isCrashPredicted = false;
    }

    // Fade to black
    private IEnumerator FadeToBlack()
    {
        if (fadeScreen != null)
        {
            fadeScreen.FadeToBlack(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }
    }

    // Fade from black
    private IEnumerator FadeFromBlack()
    {
        if (fadeScreen != null)
        {
            fadeScreen.FadeFromBlack(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }
    }

    // Teleport plane to water position
    private void TeleportToWater()
    {
        transform.position = waterRespawnPosition;
        transform.rotation = Quaternion.identity;
        
        // Reset rigidbody
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Reset plane state after crash
    private void ResetPlaneState()
    {
        currentThrottle = 0f;
        currentPitch = 0f;
        currentRoll = 0f;
        currentYaw = 0f;
    }

    // Predict if a crash is unavoidable
    private bool PredictCrash()
    {
        if (isCrashing || isCrashPredicted) return false;
        
        // Calculate future position based on current velocity and trajectory
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 predictedPosition = transform.position + (currentVelocity * crashPredictionTime);
        
        // Raycast from current position towards predicted position
        Vector3 rayDirection = (predictedPosition - transform.position).normalized;
        float rayDistance = Vector3.Distance(transform.position, predictedPosition);
        
        // Also check current trajectory
        Vector3 currentDirection = currentVelocity.normalized;
        
        // Cast multiple rays to check for ground collision
        RaycastHit hit;
        
        // Primary raycast in current movement direction
        if (Physics.Raycast(transform.position, currentDirection, out hit, rayDistance, groundLayerMask))
        {
            float timeToCollision = hit.distance / currentVelocity.magnitude;
            
            // If collision will happen within prediction time, crash is unavoidable
            if (timeToCollision <= crashPredictionTime && currentVelocity.magnitude > 0.1f)
            {
                return true;
            }
        }
        
        // Secondary raycast downward to check for ground proximity
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f, groundLayerMask))
        {
            float timeToGround = hit.distance / Mathf.Abs(currentVelocity.y);
            
            // If plane is moving downward and will hit ground within prediction time
            if (currentVelocity.y < -0.1f && timeToGround <= crashPredictionTime)
            {
                return true;
            }
        }
        
        return false;
    }

    // Start early crash sequence when crash is predicted
    private void StartPredictedCrash()
    {
        if (isCrashPredicted || isCrashing) return;
        
        isCrashPredicted = true;
        StartCoroutine(FadeToBlack());
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputs();
        
        // Check for crash prediction at regular intervals
        if (isGameplayActive && !isCrashing && !isCrashPredicted)
        {
            if (Time.time - lastCrashCheckTime >= crashCheckInterval)
            {
                lastCrashCheckTime = Time.time;
                
                if (PredictCrash())
                {
                    StartPredictedCrash();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // Only apply physics if gameplay is active and not crashing
        if (!isGameplayActive || isCrashing) return;
        
        // Apply forward thrust
        rb.AddForce(transform.forward * (maxThrottle * currentThrottle));
        
        // Apply rotational forces
        // rb.AddTorque(transform.right * (currentPitch * responseModifier)); // Pitch (nose up/down)
        rb.AddTorque(transform.up * (currentYaw * responseModifier));      // Yaw (left/right)
        rb.AddTorque(transform.forward * (currentRoll * responseModifier)); // Roll (banking)
        rb.AddTorque(transform.right * (currentPitch * responseModifier)); // Pitch (nose up/down)
        
        // Prevent barrel rolls by applying gradual counteracting torque when roll angle exceeds threshold
        Vector3 currentRotation = transform.eulerAngles;
        float currentRollAngle = currentRotation.z;
        if (currentRollAngle > 180f) currentRollAngle -= 360f; // Convert to -180 to 180 range
        
        // Apply gradual counteracting torque if roll angle exceeds maxRollAngle + tolerance
        float correctionThreshold = maxRollAngle + rollOvershootTolerance;
        if (Mathf.Abs(currentRollAngle) > correctionThreshold)
        {
            float overshoot = Mathf.Abs(currentRollAngle) - correctionThreshold;
            float rollCorrection = -Mathf.Sign(currentRollAngle) * overshoot * responseModifier * rollCorrectionStrength;
            rb.AddTorque(transform.forward * rollCorrection);
        }
        
        // Update animation speed based on plane speed
        if (animator != null)
        {
            float planeSpeed = rb.linearVelocity.magnitude;
            animator.SetFloat("FlySpeed", planeSpeed );
            audioSource.volume = planeSpeed / 100f / 4;
        }

        // Add stabilization forces to return to horizontal when no yaw input
        //Abs of currentYaw in't enough apparently for the condition to be true despite being correct when logged
        if (Mathf.Abs(currentYaw) <= 0.1f && yawAction?.IsPressed() == false) {
            if (currentRollAngle > 180f) currentRollAngle -= 360f;
            
            // Gradually reduce the roll angle instead of applying direct torque
            float targetRollAngle = 0f; // Horizontal
            float newRollAngle = Mathf.LerpAngle(currentRollAngle, targetRollAngle, Time.deltaTime * levelReturnSpeed);

            // Apply the difference as torque
            float stabilizationTorque = (newRollAngle - currentRollAngle) * responseModifier;
            rb.AddTorque(transform.forward * stabilizationTorque);
        }

        // Add pitch stabilization to prevent extreme pitch angles
        float currentPitchAngle = currentRotation.x;
        if (currentPitchAngle > 180f) currentPitchAngle -= 360f; // Convert to -180 to 180 range

        float maxPitchUp = 45f; // Maximum pitch up (nose up)
        float maxPitchDown = 30f; // Maximum pitch down (nose down)
        float pitchStabilizationStrength = 0.3f;

        // Prevent extreme pitch up
        if (currentPitchAngle < -maxPitchUp)
        {
            float overshoot = Mathf.Abs(currentPitchAngle) - maxPitchUp;
            float pitchStabilization = overshoot * responseModifier * pitchStabilizationStrength;
            rb.AddTorque(transform.right * pitchStabilization);
        }

        // Prevent extreme pitch down
        if (currentPitchAngle > maxPitchDown)
        {
            float overshoot = currentPitchAngle - maxPitchDown;
            float pitchStabilization = -overshoot * responseModifier * pitchStabilizationStrength;
            rb.AddTorque(transform.right * pitchStabilization);
        }


        // //ground collision prevention ( peut faire du raze motte aussi avec ça)
        // RaycastHit hit;
        // float raycastDistance = 50f;
        // Vector3 raycastDirection = Vector3.down;
        // int groundLayer = LayerMask.GetMask("Default");

        // print("currentPitch: " + currentPitch);

        // if (Physics.Raycast(transform.position, raycastDirection, out hit, raycastDistance, groundLayer))
        // {
        //     float distanceToGround = hit.distance;


        //     print("distanceToGround: " + distanceToGround);
        //     float avoidanceThreshold = 15f;

        //     if (distanceToGround < avoidanceThreshold)
        //     {
                
        //         // Check if plane is pitching towards ground (nose down)
        //         print("currentPitchAngle: " + currentPitchAngle);
        //         bool isPitchingDown = currentPitchAngle < 90f && currentPitchAngle > 0f; // Adjust threshold as needed
                
        //         if (isPitchingDown)
        //         {
        //             print("avoiding collision - pitching towards ground");
        //             float maxPitchAngle = 30f;
                    
        //             if (currentPitchAngle > -maxPitchAngle)
        //             {
        //                 currentPitch = -1f; // Pitch up to avoid
        //                 currentThrottle += 0.1f;
        //             }
        //             else
        //             {
        //                 currentPitch = 0f;
        //             }
        //         }
        //         else
        //         {
        //             // Not pitching towards ground, use normal input
        //             if(pitchAction != null) currentPitch = pitchAction.ReadValue<float>();
        //         }
        //     }
        //     else
        //     {
        //         if(pitchAction != null) currentPitch = pitchAction.ReadValue<float>();
        //     }
        // }
    }

    /*private void OnDrawGizmos()
    {
        Vector3 raycastDirection = Vector3.down;
        float raycastDistance = 50f;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, raycastDirection * 50f);
    }*/
}
