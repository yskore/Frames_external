using UnityEngine;

public class Piece_movement : MonoBehaviour
{
    private bool isDragging = false;
    private bool isScaling = false;
    private Vector2 touchStart;
    private Vector3 objectStartPosition;
    private float startRotationY;
    private float initialDistance;
    private Vector3 initialScale;

    void Update()
    {
        // Handle single touch for movement and rotation
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                    {
                        isDragging = true;
                        touchStart = touch.position;
                        objectStartPosition = transform.position;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        // Calculate movement in screen space
                        Vector2 delta = touch.position - touchStart;

                        // Convert screen movement to world space movement
                        Vector3 forward = Camera.main.transform.forward;
                        forward.y = 0; // Keep movement in horizontal plane
                        forward.Normalize();
                        Vector3 right = Camera.main.transform.right;
                        right.y = 0;
                        right.Normalize();

                        // Apply movement
                        Vector3 movement = (right * delta.x + forward * -delta.y) * 0.01f;
                        transform.position = objectStartPosition + movement;
                    }
                    break;

                case TouchPhase.Ended:
                    isDragging = false;
                    break;
            }
        }
        // Handle two finger touch for scaling and rotation
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                // Calculate initial distance between touches
                initialDistance = Vector2.Distance(touch0.position, touch1.position);
                initialScale = transform.localScale;

                // Calculate center point between touches
                Vector2 center = (touch0.position + touch1.position) / 2;
                Ray ray = Camera.main.ScreenPointToRay(center);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                {
                    isScaling = true;
                }
            }
            // Handle scaling
            else if (isScaling && (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved))
            {
                // Calculate current distance between touches
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                float scaleFactor = currentDistance / initialDistance;

                // Apply scaling with limits
                Vector3 newScale = initialScale * scaleFactor;
                float minScale = 0.1f;
                float maxScale = 2.0f;
                newScale = new Vector3(
                    Mathf.Clamp(newScale.x, minScale, maxScale),
                    Mathf.Clamp(newScale.y, minScale, maxScale),
                    Mathf.Clamp(newScale.z, minScale, maxScale)
                );
                transform.localScale = newScale;

                // Calculate rotation based on the change in angle between touches
                Vector2 previousVector = touch0.position - touch1.position;
                Vector2 currentVector = touch0.position + touch0.deltaPosition - (touch1.position + touch1.deltaPosition);
                float angle = Vector2.SignedAngle(previousVector, currentVector);
                transform.Rotate(Vector3.up, angle);
            }

            if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
            {
                isScaling = false;
            }
        }
    }
}