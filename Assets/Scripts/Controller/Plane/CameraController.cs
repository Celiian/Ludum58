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
    private int previousIndex = -1;

    [SerializeField]
    private bool cameraLocked = true;
    private bool canChangeCamera = false;

    private void Update(){
        
        if(Input.GetKeyDown(KeyCode.Alpha1) && canChangeCamera) index = 0;
        else if(Input.GetKeyDown(KeyCode.Alpha2) && canChangeCamera) index = 1;
        else if(Input.GetKeyDown(KeyCode.Alpha3) && canChangeCamera) index = 2;
        else if(Input.GetKeyDown(KeyCode.Alpha4) && canChangeCamera) index = 3;
        
        if(cameraLocked) return;
        
        // If camera position changed, instantly snap to new position
        if(index != previousIndex)
        {
            transform.position = cameraPositions[index].position;
            transform.forward = cameraPositions[index].forward;
            previousIndex = index;
        }

        // set our target to the relevant POV
        target = cameraPositions[index].position;

    }

    // Method to switch to gameplay camera (index 0) when Play is clicked
    public void SwitchToGameplayCamera()
    {
        index = 0;
        previousIndex = -1; // Force instant snap
        target = cameraPositions[index].position;
        cameraLocked = false;
        canChangeCamera = true;
    }

    public void SwitchEndGameCamera()
    {
        index = cameraPositions.Length - 1; // Assuming last index is end game view
        previousIndex = -1; // Force instant snap
        target = cameraPositions[index].position;
        // cameraLocked = true;
        canChangeCamera = false;
    }

    private void FixedUpdate()
    {
        if(cameraLocked) return;
        Vector3 newPosition = Vector3.Lerp(transform.position, target, moveSpeed * Time.deltaTime);
        newPosition.y = Mathf.Max(newPosition.y, 2f);
        transform.position = newPosition;
        transform.forward = cameraPositions[index].forward;
    }
}
