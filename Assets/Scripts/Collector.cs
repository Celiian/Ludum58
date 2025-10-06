using UnityEngine;
using System.Collections;
using UnityEngine.VFX;

public class Collector : MonoBehaviour
{
    public VisualEffect visualEffect;
    public Material material;
    public string colorName;
    public float transitionDuration = 1.0f;
    public bool isTriggered = false;


    void Start()
    {
        if (material != null)
        {
            material.SetFloat(colorName, 0.0f);
        }
        visualEffect.gameObject.SetActive(false);
        visualEffect.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            if (isTriggered) return;
            GameManager.Instance.collected.Add(gameObject);
            
            isTriggered = true;
            StartCoroutine(TransitionColor());
            visualEffect.gameObject.SetActive(true);
            visualEffect.transform.position = transform.position;
            visualEffect.Play();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
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
        visualEffect.Stop();
        visualEffect.gameObject.SetActive(false);
    }
}
