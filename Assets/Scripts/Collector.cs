using UnityEngine;
using System.Collections;

public class Collector : MonoBehaviour
{
    public Material material;
    public string colorName;
    public float transitionDuration = 1.0f;
    
    private bool isTriggered = false;

    void Start()
    {
        if (material != null)
        {
            material.SetFloat(colorName, 0.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(TransitionColor());
        }
    }

    private IEnumerator TransitionColor()
    {
        float elapsedTime = 0.0f;
        float startValue = material.GetFloat(colorName);

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            float currentValue = Mathf.Lerp(startValue, 1.0f, t);
            material.SetFloat(colorName, currentValue);
            yield return null;
        }

        material.SetFloat(colorName, 1.0f);
    }
}
