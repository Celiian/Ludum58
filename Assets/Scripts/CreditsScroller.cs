using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CreditsScroller : MonoBehaviour
{
    public float scrollSpeed = 20f; // Speed of the scrolling
    public RectTransform creditsPanel; // The panel containing the credits
    public Button backButton; // Button to go back to main menu
    
    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnReturnMenuClicked);

            creditsPanel.gameObject.SetActive(false);
    }
    
    public void OnReturnMenuClicked()
    {
        Debug.Log("Reload scene!");
        SceneManager.LoadScene("Game");
    }
    
}