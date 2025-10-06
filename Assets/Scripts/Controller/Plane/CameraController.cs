using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("An array of transforms representing camera positions")]
    [SerializeField] private Transform[] cameraPositions;

    [Tooltip("The speed at which the camera follows the plane")]
    [SerializeField] private float minMoveSpeed = 5f;
    [SerializeField] private float maxMoveSpeed = 15f;
    [SerializeField] private PlaneController planeController;

    private float moveSpeed = 5f;
    private int index = 0;
    private Vector3 target;
    private int previousIndex = -1;

    private bool cameraLocked = true;

    private void Update(){

        if(cameraLocked) return;

        moveSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, planeController.GetCurrentThrottle());
        
        if(Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        else if(Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        else if(Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
        else if(Input.GetKeyDown(KeyCode.Alpha4)) index = 3;

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
