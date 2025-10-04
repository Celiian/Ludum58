using UnityEngine;

public class Collector : MonoBehaviour
{

    public Material material;
    public string colorName;


    void Start()
    {
        if (material != null)
        {
            material.SetFloat(colorName, 0.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (material != null)
            {
                material.SetFloat(colorName, 1.0f);
            }
            // Destroy(gameObject);
        }
    }
}
