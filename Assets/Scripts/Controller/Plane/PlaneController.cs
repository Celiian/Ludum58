using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{

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
    public float bankingFactor = 0.3f;

    [Tooltip("How quickly the plane returns to level flight")]
    public float levelReturnSpeed = 2f;

    [Tooltip("Maximum roll angle in degrees")]
    public float maxRollAngle = 30f;

    [Tooltip("How much overshoot is allowed before correction kicks in")]
    public float rollOvershootTolerance = 10f;

    [Tooltip("How strong the roll correction force is")]
    public float rollCorrectionStrength = 0.5f;

    private float currentThrottle = 0f;

    private float currentPitch = 0f;

    private float currentRoll = 0f;

    private float currentYaw = 0f;

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
        // Get input values from Input System
        if (pitchAction != null) currentPitch = pitchAction.ReadValue<float>();
        if (yawAction != null) currentYaw = yawAction.ReadValue<float>();
        
        // Calculate automatic banking based on yaw input
        float targetRoll = -currentYaw * bankingFactor; // Negative because left yaw should cause left roll
        
        // Smoothly transition to target roll or return to level
        if (Mathf.Abs(currentYaw) > 0.1f) {
            // Banking when yawing
            currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * responsiveness);
        } else {
            // Return to level flight when not yawing
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
            
        // currentThrottle = Mathf.Clamp(currentThrottle, 0f, maxThrottle); // this prevents the plane from moving for some reason
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
    // Update is called once per frame
    void Update()
    {
        HandleInputs();

    }

    private void FixedUpdate()
    {
        // Apply forward thrust
        rb.AddForce(transform.forward * maxThrottle * currentThrottle);
        
        // Apply rotational forces
        rb.AddTorque(transform.right * currentPitch * responseModifier); // Pitch (nose up/down)
        rb.AddTorque(transform.up * currentYaw * responseModifier);      // Yaw (left/right)
        rb.AddTorque(transform.forward * currentRoll * responseModifier); // Roll (banking)
        
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
        }
    }
}
