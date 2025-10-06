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

    private Vector2 startPosition;
    private bool isScrolling = false;
    
    void Start()
    {
        if (creditsPanel != null)
            startPosition = creditsPanel.anchoredPosition;
    
        if (backButton != null)
            backButton.onClick.AddListener(OnReturnMenuClicked);
    }
    
    void Update()
    {
        if (!isScrolling) return;
        if (creditsPanel != null)
        {
            // Move the credits panel upwards
            creditsPanel.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
    
            // Reset position if it has scrolled past a certain point
            if (creditsPanel.anchoredPosition.y >= 1000)
            {
                creditsPanel.anchoredPosition = startPosition;
            }
        }
    }

    public void StartScrolling()
    {
        isScrolling = true;
    }
    
    public void OnReturnMenuClicked()
    {
        Debug.Log("Reload scene!");
        // SceneManager.LoadScene("Credits");
    }
    
}