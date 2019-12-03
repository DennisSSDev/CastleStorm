using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class UserInput : ComponentSystem
{
    private Transform cameraTransform = null;
    
    private float3 cameraPosition;

    private float ScreenWidth = 0f;
    private float ScreenHeight = 0f;

    private float mouseX = 0.0f;
    private float mouseY = 0.0f;

    private float sensitivity = 0.5f;
    private float panSpeed = 20f;
    private float panBorderThickness = 10f;
    private float scrollSpeed = 2500f;

    protected override void OnStartRunning()
    {
        ScreenWidth = Screen.width;
        ScreenHeight = Screen.height;
        cameraTransform = Camera.main.transform;
        base.OnStartRunning();
    }

    protected override void OnUpdate()
    {
        Pan();
        Zoom();
    }
    
    /**
     * Movement of the camera
     * Move based on one way or the other but not both at the same time
     */
    private void Pan()
    {
        if (Input.GetMouseButton (0)) 
        {
            cameraPosition = cameraTransform.position;
            float3 right = cameraTransform.right;
            float3 forward = new float3(0, 0, 1);

            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            cameraPosition += right * (mouseX * -1) * sensitivity;
            cameraPosition += forward * (mouseY * -1) * sensitivity;
            cameraTransform.position = cameraPosition;
        }
        else
        {
            float delta = Time.deltaTime;
            cameraPosition = cameraTransform.position;

            if (Input.mousePosition.y > ScreenHeight - panBorderThickness)
            {
                cameraPosition.z += panSpeed * delta;
            }
            else if (Input.mousePosition.y < panBorderThickness)
            {
                cameraPosition.z -= panSpeed * delta;
            }

            if (Input.mousePosition.x > ScreenWidth - panBorderThickness)
            {
                cameraPosition.x += panSpeed * delta;
            }
            else if (Input.mousePosition.x < panBorderThickness)
            {
                cameraPosition.x -= panSpeed * delta;
            }

            cameraTransform.position = cameraPosition;
        }
    }

    /**
     * Control how far away the camera is from the field
     */
    private void Zoom()
    {
        float delta = Time.deltaTime;
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        cameraPosition = cameraTransform.position;

        cameraPosition.y -= scroll * scrollSpeed * delta;

        cameraTransform.position = cameraPosition;
    }
}
