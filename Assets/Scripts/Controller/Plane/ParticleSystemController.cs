using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    [Header("Particle System")]
    [SerializeField] private ParticleSystem _particleSystem;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Water Detection")]
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private LayerMask seaLayerMask = 1 << 4; // Water layer
    [SerializeField] private float seaLevelY = 0f;
    [SerializeField] private float activationDistance = 10f;
    
    [Header("Particle Settings")]
    [SerializeField] private float minEmissionRate = 0f;
    [SerializeField] private float maxEmissionRate = 100f;
    [SerializeField] private float forwardOffset = 5f;
    
    [Header("Plane Reference")]
    [SerializeField] private PlaneController planeController;
    
    private ParticleSystem.EmissionModule emissionModule;
     
    private void Start()
    {   
        emissionModule = _particleSystem.emission;
        emissionModule.enabled = true;
        
        if (!_particleSystem.isPlaying)
        {
            _particleSystem.Play();
        }
        
        if (audioSource != null)
        {
            audioSource.volume = 0f;
        }
    }
    
    private void Update()
    {
        // Position particles in front of plane at sea level
        PositionParticlesInFrontOfPlane();
        
        float distanceToSea = GetDistanceToSea();
        float planeSpeed = GetPlaneSpeed();
        
        UpdateParticleCount(planeSpeed, distanceToSea);
    }
    
    private void UpdateParticleCount(float planeSpeed, float distanceToSea)
    {
        // Higher speed = more particles
        float speedFactor = Mathf.Clamp01(planeSpeed / 20f); // Normalize to 0-1
        
        // Closer to water = more particles (gradual falloff beyond activation distance)
        float heightFactor = Mathf.Clamp01(1f - (distanceToSea / activationDistance));
        
        // Combine both factors
        float combinedFactor = (speedFactor + heightFactor) / 2f;
        
        // Check if close enough to water for particles
        if (distanceToSea > activationDistance)
        {
            // Too high - no particles
            emissionModule.rateOverTime = 0f;
        }
        else if (planeSpeed <= 0.1f)
        {
            // If not moving, no particles
            emissionModule.rateOverTime = 0f;
        }
        else
        {
            float emissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, combinedFactor);
            emissionModule.rateOverTime = emissionRate;
        }
        
        // Audio volume should be primarily based on height, with speed as a modifier
        float audioVolume = heightFactor * speedFactor;
        UpdateAudioVolume(audioVolume);
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
    
    private float GetPlaneSpeed()
    {
        if (planeController == null) return 0f;
        
        Rigidbody planeRb = planeController.GetComponent<Rigidbody>();
        if (planeRb == null) return 0f;
        
        return planeRb.linearVelocity.magnitude;
    }
    
    private void PositionParticlesInFrontOfPlane()
    {
        if (_particleSystem == null || planeController == null) return;
        
        // Get plane's forward direction
        Vector3 planeForward = planeController.transform.forward;
        
        // Calculate position in front of plane at sea level
        Vector3 planePos = planeController.transform.position;
        Vector3 frontPosition = planePos + planeForward * forwardOffset;
        frontPosition.y = seaLevelY;
        
        _particleSystem.transform.position = frontPosition;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize raycast in scene view
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.down * raycastDistance);
        
        // Show activation area
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
    
    private void UpdateAudioVolume(float combinedFactor)
    {
        if (audioSource == null) return;
        
        float volume = Mathf.Clamp01(combinedFactor);
        audioSource.volume = volume / 2f;
    }
}
