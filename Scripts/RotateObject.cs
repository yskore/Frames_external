using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 100.0f;
    private bool isRotating = false;
    private Vector2 previousTouchPosition;

    void Update()
    {
        // Only handle single-touch rotation
        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                isRotating = true;
                previousTouchPosition = touch.position;
                break;

            case TouchPhase.Moved:
                if (isRotating)
                {
                    Vector2 deltaTouchPosition = touch.position - previousTouchPosition;
                    previousTouchPosition = touch.position;

                    float rotationX = deltaTouchPosition.x * rotationSpeed * Time.deltaTime;
                    float rotationY = deltaTouchPosition.y * rotationSpeed * Time.deltaTime;

                    transform.Rotate(Vector3.up, -rotationX, Space.World);
                    transform.Rotate(Vector3.right, rotationY, Space.World);
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isRotating = false;
                break;
        }
    }
}