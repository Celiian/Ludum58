using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button PlayButton;
    public Button QuitButton;
    public Button OptionsButton;

    [Header("UI Canvas")]
    public GameObject menuCanvas;

    [Header("Game References")]
    public CameraController cameraController;
    public PlaneController planeController;
    public TutorialUiManager tutorialManager;

    private void Start()
    {
        // Set up button listeners
        if (PlayButton != null)
            PlayButton.onClick.AddListener(OnPlayClicked);
        
        if (QuitButton != null)
            QuitButton.onClick.AddListener(OnQuitClicked);
        
        if (OptionsButton != null)
            OptionsButton.onClick.AddListener(OnOptionsClicked);
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play button clicked!");
        
        
        // Switch camera to gameplay view (index 0)
        if (cameraController != null)
            cameraController.SwitchToGameplayCamera();
        
        // Enable plane controls
        if (planeController != null)
            planeController.StartGameplay();
        
        // Start the tutorial
        if (tutorialManager != null)
            tutorialManager.StartTutorial();
        
        // Hide menu UI
        if (menuCanvas != null)
            menuCanvas.SetActive(false);
        else
            gameObject.SetActive(false); // Fallback if no canvas assigned
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit button clicked!");
        Application.Quit();
    }

    private void OnOptionsClicked()
    {
        Debug.Log("Options button clicked!");
        // Add options menu functionality here
    }
}
