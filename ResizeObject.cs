using UnityEngine;

public class ResizeObject : MonoBehaviour
{
    public float resizeSpeed = 0.5f; // Adjust resize speed as needed

    void Update()
    {
        // Handle resizing
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
            Vector3 scaleChange = Vector3.one * scrollAmount * resizeSpeed;
            transform.localScale += scaleChange;

            // Ensure the object doesn't get too small or too large
            float minScale = 0.1f;
            float maxScale = 10.0f;
            transform.localScale = Vector3.Max(Vector3.one * minScale, transform.localScale);
            transform.localScale = Vector3.Min(Vector3.one * maxScale, transform.localScale);
        }
    }
}
