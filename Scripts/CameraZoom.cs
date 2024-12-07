using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private Camera specificCamera;
    public float zoomSpeed = 1f;
    public float minDistance = 1f;
    public float maxDistance = 10f;
    private float initialPinchDistance;
    private bool isPinching = false;
    private RotateObject rotateObject;

    // Reference point to zoom towards/away from
    private Vector3 targetPoint = Vector3.zero;

    void Start()
    {
        GameObject cameraObject = GameObject.FindWithTag("MainCamera");
        if (cameraObject != null)
        {
            specificCamera = cameraObject.GetComponent<Camera>();
            Debug.LogError($"Camera found: {cameraObject.name} at position {specificCamera.transform.position}");


            // Store the target point as the point the camera is looking at
            targetPoint = transform.position; // This is the frame's position
            Debug.LogError($"Target point set to: {targetPoint}");
        }
    }

    void Update()
    {
        if (specificCamera == null || !specificCamera.gameObject.activeInHierarchy) return;

        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                isPinching = true;
                initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                Debug.LogError($"Started pinch with distance: {initialPinchDistance}");
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (isPinching)
                {
                    float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                    // Calculate zoom direction
                    Vector3 directionToTarget = targetPoint - specificCamera.transform.position;
                    float currentDistanceToTarget = directionToTarget.magnitude;

                    // Calculate zoom amount
                    float pinchDelta = (currentDistance - initialPinchDistance) * zoomSpeed * 0.01f;

                    // Calculate new position
                    Vector3 zoomDirection = directionToTarget.normalized;
                    Vector3 newPosition = specificCamera.transform.position + zoomDirection * pinchDelta;

                    // Calculate new distance to target
                    float newDistanceToTarget = Vector3.Distance(newPosition, targetPoint);

                    // Only move if within bounds
                    if (newDistanceToTarget > minDistance && newDistanceToTarget < maxDistance)
                    {
                        specificCamera.transform.position = newPosition;
                        Debug.LogError($"Camera moved to: {newPosition}, Distance to target: {newDistanceToTarget}");
                    }

                    initialPinchDistance = currentDistance;
                }
            }
            else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
            {
                isPinching = false;
            }
        }
    }

    // Visualize the zoom limits in the editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
    }
}