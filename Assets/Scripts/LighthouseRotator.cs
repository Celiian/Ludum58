using UnityEngine;

public class LighthouseRotator : MonoBehaviour
{
    [Tooltip("Tours par minute")]
    public float rpm = 4f;

    void Update()
    {
        // 360° * tours/seconde * dt
        float degPerSec = 360f * (rpm / 60f);
        transform.Rotate(0f, degPerSec * Time.deltaTime, 0f, Space.World);
    }
}