using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialUiManager : MonoBehaviour
{
    [Header("Tutorial UI Elements")]
    public TextMeshProUGUI thrustText;
    public TextMeshProUGUI yawText;
    public TextMeshProUGUI pitchText;
    public TextMeshProUGUI cameraText;

    [Header("References")]
    public GameObject tutorialPlane;
    public PlaneController planeController;

    [Header("Tutorial State")]
    private TutorialStep currentStep = TutorialStep.None;
    private float initialThrottle;
    private float initialPitch;
    private float initialYaw;
    
    [Header("Animation Settings")]
    public float fadeDuration = 1f;

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
        // Hide all tutorial texts initially and set alpha to 0
        if (thrustText != null) 
        {
            thrustText.gameObject.SetActive(false);
            SetTextAlpha(thrustText, 0f);
        }
        if (yawText != null) 
        {
            yawText.gameObject.SetActive(false);
            SetTextAlpha(yawText, 0f);
        }
        if (pitchText != null) 
        {
            pitchText.gameObject.SetActive(false);
            SetTextAlpha(pitchText, 0f);
        }
        if (cameraText != null) 
        {
            cameraText.gameObject.SetActive(false);
            SetTextAlpha(cameraText, 0f);
        }
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
        if (thrustText != null) 
        {
            thrustText.gameObject.SetActive(true);
            StartCoroutine(FadeInText(thrustText));
        }
        
        Debug.Log("Tutorial started - waiting for thrust input");
    }

    private void CheckThrustInput()
    {
        float currentThrottle = planeController.GetCurrentThrottle();
        
        // Check if throttle has increased from initial value
        if (currentThrottle > initialThrottle + 0.1f) // Small threshold to avoid noise
        {
            // Fade out thrust text, fade in pitch text
            if (thrustText != null) StartCoroutine(FadeOutText(thrustText));
            if (pitchText != null) 
            {
                pitchText.gameObject.SetActive(true);
                StartCoroutine(FadeInText(pitchText));
            }
            
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
            // Fade out pitch text, fade in yaw text
            if (pitchText != null) StartCoroutine(FadeOutText(pitchText));
            if (yawText != null) 
            {
                yawText.gameObject.SetActive(true);
                StartCoroutine(FadeInText(yawText));
            }
            
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
            // Fade out yaw text and show camera text for 3 seconds
            if (yawText != null) StartCoroutine(FadeOutText(yawText));
            if (cameraText != null) 
            {
                cameraText.gameObject.SetActive(true);
                StartCoroutine(ShowCameraTextTemporarily());
            }
            
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
        if (thrustText != null) 
        {
            thrustText.gameObject.SetActive(false);
            SetTextAlpha(thrustText, 0f);
        }
        if (yawText != null) 
        {
            yawText.gameObject.SetActive(false);
            SetTextAlpha(yawText, 0f);
        }
        if (pitchText != null) 
        {
            pitchText.gameObject.SetActive(false);
            SetTextAlpha(pitchText, 0f);
        }
        if (cameraText != null) 
        {
            cameraText.gameObject.SetActive(false);
            SetTextAlpha(cameraText, 0f);
        }
    }

    // Helper method to set text alpha
    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text != null)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }

    // Fade in animation coroutine
    private IEnumerator FadeInText(TextMeshProUGUI text)
    {
        if (text == null) yield break;
        
        float elapsedTime = 0f;
        Color startColor = text.color;
        Color targetColor = startColor;
        targetColor.a = 1f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            text.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        text.color = targetColor;
    }

    // Fade out animation coroutine
    private IEnumerator FadeOutText(TextMeshProUGUI text)
    {
        if (text == null) yield break;
        
        float elapsedTime = 0f;
        Color startColor = text.color;
        Color targetColor = startColor;
        targetColor.a = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            text.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        text.color = targetColor;
        text.gameObject.SetActive(false);
    }

    // Show camera text for 3 seconds then fade out
    private IEnumerator ShowCameraTextTemporarily()
    {
        if (cameraText == null) yield break;
        
        // Fade in camera text
        yield return StartCoroutine(FadeInText(cameraText));
        
        // Wait for 3 seconds
        yield return new WaitForSeconds(2f);
        
        // Fade out camera text
        yield return StartCoroutine(FadeOutText(cameraText));
    }
}
