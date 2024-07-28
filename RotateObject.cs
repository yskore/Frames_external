using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 100.0f; // Adjust rotation speed as needed
    private bool isRotating = false;
    private Vector3 previousMousePosition;

    void Update()
    {
        // Check for mouse button input
        if (Input.GetMouseButtonDown(0))
        {
            isRotating = true;
            previousMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }

        // Handle rotation
        if (isRotating)
        {
            Vector3 deltaMousePosition = Input.mousePosition - previousMousePosition;
            previousMousePosition = Input.mousePosition;

            float rotationX = deltaMousePosition.x * rotationSpeed * Time.deltaTime;
            float rotationY = deltaMousePosition.y * rotationSpeed * Time.deltaTime;

            // Apply rotation
            transform.Rotate(Vector3.up, -rotationX, Space.World);
            transform.Rotate(Vector3.right, rotationY, Space.World);
        }
    }
}
