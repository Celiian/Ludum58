using UnityEngine;

public class WaveAudioController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] waveSounds;
    [SerializeField] private float minPlayInterval = 2f;
    [SerializeField] private float maxPlayInterval = 8f;
    [SerializeField] private bool keepAudioSourceAtSeaLevel = true;
    
    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private LayerMask seaLayerMask = 1 << 4; // Water layer
    [SerializeField] private float seaLevelY = 0f;
    
    [Header("Volume Settings")]
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float minVolume = 0.1f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float minDistance = 5f;
    
    private float nextPlayTime;
    private bool isPlaying = false;
    
    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null)
        {
            Debug.LogError("WaveAudioController: No AudioSource found!");
            enabled = false;
            return;
        }
        
        PositionAudioSourceAtSeaLevel();
        SetNextPlayTime();
    }
    
    private void Update()
    {
        if (keepAudioSourceAtSeaLevel)
        {
            PositionAudioSourceAtSeaLevel();
        }
        
        if (Time.time >= nextPlayTime && !isPlaying)
        {
            PlayRandomWaveSound();
        }
    }
    
    private void PlayRandomWaveSound()
    {
        if (waveSounds == null || waveSounds.Length == 0)
        {
            Debug.LogWarning("WaveAudioController: No wave sounds assigned!");
            return;
        }
        
        float distanceToSea = GetDistanceToSea();
        float volume = CalculateVolumeFromDistance(distanceToSea);
        
        if (volume > minVolume)
        {
            AudioClip randomClip = waveSounds[Random.Range(0, waveSounds.Length)];
            audioSource.clip = randomClip;
            audioSource.volume = volume;
            audioSource.Play();
            
            isPlaying = true;
            Invoke(nameof(OnSoundFinished), randomClip.length);
        }
        
        SetNextPlayTime();
    }
    
    private float GetDistanceToSea()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Vector3.down;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance, seaLayerMask))
        {
            return hit.distance;
        }
        
        // Fallback: calculate distance to sea level Y position
        return Mathf.Abs(transform.position.y - seaLevelY);
    }
    
    private float CalculateVolumeFromDistance(float distance)
    {
        if (distance <= minDistance)
            return maxVolume;
            
        if (distance >= maxDistance)
            return minVolume;
            
        // Linear interpolation between max and min volume based on distance
        float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);
        return Mathf.Lerp(maxVolume, minVolume, normalizedDistance);
    }
    
    private void SetNextPlayTime()
    {
        nextPlayTime = Time.time + Random.Range(minPlayInterval, maxPlayInterval);
    }
    
    private void PositionAudioSourceAtSeaLevel()
    {
        if (audioSource == null) return;
        
        Vector3 currentPos = transform.position;
        Vector3 seaLevelPos = new Vector3(currentPos.x, seaLevelY, currentPos.z);
        audioSource.transform.position = seaLevelPos;
    }
    
    private void OnSoundFinished()
    {
        isPlaying = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize raycast in scene view
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.down * raycastDistance);
        
        // Show volume influence area
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}