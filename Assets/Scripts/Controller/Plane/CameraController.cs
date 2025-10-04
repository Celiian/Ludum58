using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("An array of transforms representing camera positions")]
    [SerializeField] private Transform[] cameraPositions;

    [Tooltip("The current camera position index")]
    private int currentCameraPositionIndex = 0;

    [Tooltip("The speed at which the camera follows the plane")]
    private float moveSpeed = 5f;
    
    private int index = 0;
    private Vector3 target;

    private void Update(){
        if(Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        else if(Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        else if(Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
        else if(Input.GetKeyDown(KeyCode.Alpha4)) index = 3;

        // set our target to the relevant POV
        target = cameraPositions[index].position;

    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target, moveSpeed * Time.deltaTime);
        transform.forward = cameraPositions[index].forward;
    }
}
