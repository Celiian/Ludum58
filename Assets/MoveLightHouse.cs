using System.Collections;
using UnityEngine;

public class MoveLightHouse : MonoBehaviour
{
    // Déplace le transform à y = 0 en 5 secondes
    private IEnumerator MoveToYZeroIn5Sec()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, 0f, startPos.z);
        float elapsed = 0f;
        float duration = 5f;
    
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
    
    public void MoveLightHouseToYZero()
    {
        StartCoroutine(MoveToYZeroIn5Sec());
    }
    
}
