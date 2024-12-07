using UnityEngine;

public class ResizeObject : MonoBehaviour
{
    public float pinchSpeed = 0.5f;
    public float minScale = 0.5f;
    public float maxScale = 2.0f;

    private float initialPinchDistance;
    private bool isPinching = false;
    private GameObject faceObject;
    private FaceSelector faceSelector;
    private RotateObject rotateObject;

    // Store initial scales
    private Vector3 initialFrameScale;
    private Vector3 initialFaceScale;
    private float baseScaleFactor = 1.0f;  // Track overall scaling

    public void SetupFaceObject(string faceName)
    {
        faceSelector = GetComponent<FaceSelector>();
        rotateObject = GetComponent<RotateObject>();

        if (faceSelector != null)
        {
            try
            {
                faceObject = faceSelector.GetFaceObject(faceName);
                if (faceObject != null)
                {
                    // Store initial scales
                    initialFrameScale = transform.localScale;
                    initialFaceScale = faceObject.transform.localScale;
                    Debug.LogError($"Initial frame scale: {initialFrameScale}");
                    Debug.LogError($"Initial face scale: {initialFaceScale}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting face object: {e.Message}");
            }
        }
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            if (rotateObject != null)
            {
                rotateObject.enabled = false;
            }

            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                isPinching = true;
                initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                Debug.LogError($"Pinch started. Initial distance: {initialPinchDistance}");
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (isPinching)
                {
                    float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);

                    // Calculate delta instead of direct ratio
                    float pinchDelta = (currentPinchDistance - initialPinchDistance) * pinchSpeed / Screen.dpi;
                    baseScaleFactor += pinchDelta;
                    baseScaleFactor = Mathf.Clamp(baseScaleFactor, minScale, maxScale);

                    Debug.LogError($"Base scale factor: {baseScaleFactor}");

                    // Calculate frame scale
                    Vector3 newFrameScale = initialFrameScale * baseScaleFactor;

                    // Calculate face scale with a dampened factor for smaller dimensions
                    float faceDampenFactor = 0.2f; // Adjust this value between 0-1 to control how much the face scales
                    Vector3 newFaceScale = initialFaceScale * (1 + (baseScaleFactor - 1) * faceDampenFactor);

                    // Apply scales
                    transform.localScale = newFrameScale;
                    if (faceObject != null)
                    {
                        faceObject.transform.localScale = newFaceScale;
                    }

                    Debug.LogError($"New frame scale: {newFrameScale}");
                    Debug.LogError($"New face scale: {newFaceScale}");

                    initialPinchDistance = currentPinchDistance;
                }
            }
            else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
            {
                isPinching = false;
                Debug.LogError("Pinch ended");
            }
        }
        else
        {
            if (rotateObject != null && !rotateObject.enabled)
            {
                rotateObject.enabled = true;
            }

            if (isPinching)
            {
                isPinching = false;
            }
        }
    }
}