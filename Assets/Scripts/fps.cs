using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPS : MonoBehaviour
{
    [Header("FPS Display Settings")]
    public TextMeshProUGUI fpsText;
    public float updateInterval = 0.5f;
    
    private float accum = 0.0f;
    private int frames = 0;
    private float timeleft;
    private float fps;
    
    void Start()
    {
        timeleft = updateInterval;
        
        if (fpsText == null)
        {
            fpsText = GetComponent<TextMeshProUGUI>();
        }
    }
    
    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;
        
        if (timeleft <= 0.0f)
        {
            fps = accum / frames;
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
            
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {fps:F1}";
            }
        }
    }
}
