using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    [Header("Particle System")]
    [SerializeField] private ParticleSystem particleSystem;
    
    [Header("Water Detection")]
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private LayerMask seaLayerMask = 1 << 4; // Water layer
    [SerializeField] private float seaLevelY = 0f;
    [SerializeField] private float activationDistance = 20f;
    
    [Header("Particle Settings")]
    [SerializeField] private float minEmissionRate = 10f;
    [SerializeField] private float maxEmissionRate = 100f;
    
    [Header("Plane Reference")]
    [SerializeField] private PlaneController planeController;
    
    private ParticleSystem.EmissionModule emissionModule;
    
    private void Start()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();
            
        if (particleSystem == null)
        {
            Debug.LogError("ParticleSystemController: No ParticleSystem found!");
            enabled = false;
            return;
        }
        
        if (planeController == null)
            planeController = FindObjectOfType<PlaneController>();
            
        if (planeController == null)
        {
            Debug.LogError("ParticleSystemController: No PlaneController found!");
            enabled = false;
            return;
        }
        
        emissionModule = particleSystem.emission;
        emissionModule.enabled = true;
        
        if (!particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
    }
    
    private void Update()
    {
        // Keep particles at sea level
        PositionParticlesAtSeaLevel();
        
        float distanceToSea = GetDistanceToSea();
        float planeSpeed = GetPlaneSpeed();
        
        UpdateParticleCount(planeSpeed, distanceToSea);
    }
    
    private void UpdateParticleCount(float planeSpeed, float distanceToSea)
    {
        // Check if close enough to water
        if (distanceToSea > activationDistance)
        {
            // Too high - no particles
            emissionModule.rateOverTime = 0f;
            return;
        }
        
        // Higher speed = more particles
        float speedFactor = Mathf.Clamp01(planeSpeed / 20f); // Normalize to 0-1
        
        // Closer to water = more particles
        float heightFactor = Mathf.Clamp01(1f - (distanceToSea / activationDistance));
        
        // Combine both factors
        float combinedFactor = (speedFactor + heightFactor) / 2f;
        
        float emissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, combinedFactor);
        emissionModule.rateOverTime = emissionRate;
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
    
    private void PositionParticlesAtSeaLevel()
    {
        if (particleSystem == null) return;
        
        Vector3 currentPos = transform.position;
        Vector3 seaLevelPos = new Vector3(currentPos.x, seaLevelY, currentPos.z);
        particleSystem.transform.position = seaLevelPos;
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
}
