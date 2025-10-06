using UnityEngine;

public class Clouds : MonoBehaviour
{
    public Vector2 mapSize;
    public float cloudsSpeed;
    
    private Vector3 targetPosition;
    private Vector3 direction;

    void Start()
    {
        SetRandomTarget();
    }

    public void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetRandomTarget();
        }
        
        transform.position += direction * cloudsSpeed * Time.deltaTime;
        
        Vector3 pos = transform.position;
        if (pos.x < -mapSize.x / 2 || pos.x > mapSize.x / 2 || 
            pos.z < -mapSize.y / 2 || pos.z > mapSize.y / 2)
        {
            SetRandomTarget();
        }
    }
    
    void SetRandomTarget()
    {
        targetPosition = new Vector3(
            Random.Range(-mapSize.x / 2, mapSize.x / 2),
            transform.position.y,
            Random.Range(-mapSize.y / 2, mapSize.y / 2)
        );
        direction = (targetPosition - transform.position).normalized;
    }
}