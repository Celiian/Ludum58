using UnityEngine;
using TMPro;

public class TutorialUiManager : MonoBehaviour
{
    [Header("Tutorial UI Elements")]
    public TextMeshProUGUI thrustText;
    public TextMeshProUGUI yawText;
    public TextMeshProUGUI pitchText;

    [Header("References")]
    public GameObject tutorialPlane;
    public PlaneController planeController;

    [Header("Tutorial State")]
    private TutorialStep currentStep = TutorialStep.None;
    private float initialThrottle;
    private float initialPitch;
    private float initialYaw;

    private enum TutorialStep
    {
        None,
        WaitingForThrust,
        WaitingForPitch,
        WaitingForYaw,
        Completed
    }

    private void Start()
    {
        // Hide all tutorial texts initially
        if (thrustText != null) thrustText.gameObject.SetActive(false);
        if (yawText != null) yawText.gameObject.SetActive(false);
        if (pitchText != null) pitchText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentStep == TutorialStep.None) return;

        switch (currentStep)
        {
            case TutorialStep.WaitingForThrust:
                CheckThrustInput();
                break;
            case TutorialStep.WaitingForPitch:
                CheckPitchInput();
                break;
            case TutorialStep.WaitingForYaw:
                CheckYawInput();
                break;
        }
    }

    public void StartTutorial()
    {
        if (planeController == null)
        {
            Debug.LogError("PlaneController reference is missing!");
            return;
        }

        // Store initial values
        initialThrottle = planeController.GetCurrentThrottle();
        initialPitch = planeController.GetCurrentPitch();
        initialYaw = planeController.GetCurrentYaw();

        // Start with thrust tutorial
        currentStep = TutorialStep.WaitingForThrust;
        if (thrustText != null) thrustText.gameObject.SetActive(true);
        
        Debug.Log("Tutorial started - waiting for thrust input");
    }

    private void CheckThrustInput()
    {
        float currentThrottle = planeController.GetCurrentThrottle();
        
        // Check if throttle has increased from initial value
        if (currentThrottle > initialThrottle + 0.1f) // Small threshold to avoid noise
        {
            // Hide thrust text, show pitch text
            if (thrustText != null) thrustText.gameObject.SetActive(false);
            if (pitchText != null) pitchText.gameObject.SetActive(true);
            
            // Update initial pitch value for next check
            initialPitch = planeController.GetCurrentPitch();
            currentStep = TutorialStep.WaitingForPitch;
            
            Debug.Log("Thrust detected - now waiting for pitch input");
        }
    }

    private void CheckPitchInput()
    {
        float currentPitch = planeController.GetCurrentPitch();
        
        // Check if pitch has changed significantly from initial value
        if (Mathf.Abs(currentPitch - initialPitch) > 0.1f) // Small threshold to avoid noise
        {
            // Hide pitch text, show yaw text
            if (pitchText != null) pitchText.gameObject.SetActive(false);
            if (yawText != null) yawText.gameObject.SetActive(true);
            
            // Update initial yaw value for next check
            initialYaw = planeController.GetCurrentYaw();
            currentStep = TutorialStep.WaitingForYaw;
            
            Debug.Log("Pitch detected - now waiting for yaw input");
        }
    }

    private void CheckYawInput()
    {
        float currentYaw = planeController.GetCurrentYaw();
        
        // Check if yaw has changed significantly from initial value
        if (Mathf.Abs(currentYaw - initialYaw) > 0.1f) // Small threshold to avoid noise
        {
            // Hide all tutorial texts
            if (yawText != null) yawText.gameObject.SetActive(false);
            
            currentStep = TutorialStep.Completed;
            
            Debug.Log("Yaw detected - tutorial completed!");
        }
    }

    public bool IsTutorialCompleted()
    {
        return currentStep == TutorialStep.Completed;
    }

    public void ResetTutorial()
    {
        currentStep = TutorialStep.None;
        if (thrustText != null) thrustText.gameObject.SetActive(false);
        if (yawText != null) yawText.gameObject.SetActive(false);
        if (pitchText != null) pitchText.gameObject.SetActive(false);
    }
}
