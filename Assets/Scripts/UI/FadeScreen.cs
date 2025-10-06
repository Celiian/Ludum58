using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeScreen : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("The image component used for fading")]
    public Image fadeImage;
    
    [Tooltip("Default fade duration")]
    public float defaultFadeDuration = 1f;
    
    private Canvas canvas;
    
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>();
        
        // Start with transparent
        if (fadeImage != null)
        {
            Color transparent = fadeImage.color;
            transparent.a = 0f;
            fadeImage.color = transparent;
        }
    }
    
    public void FadeToBlack(float duration = -1f)
    {
        if (duration < 0) duration = defaultFadeDuration;
        StartCoroutine(FadeToColor(Color.black, duration));
    }
    
    public void FadeFromBlack(float duration = -1f)
    {
        if (duration < 0) duration = defaultFadeDuration;
        StartCoroutine(FadeFromColor(Color.black, duration));
    }
    
    private IEnumerator FadeToColor(Color targetColor, float duration)
    {        
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        fadeImage.color = targetColor;
    }
    
    private IEnumerator FadeFromColor(Color startColor, float duration)
    {
        float elapsedTime = 0f;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        fadeImage.color = targetColor;
    }
}
