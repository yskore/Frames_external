using UnityEngine;

public class FaceSelector : MonoBehaviour
{
    public string defaultFaceName = "Face"; // Default face name

    public GameObject GetFaceObject(string faceName)
    {
        Transform faceTransform = transform.Find(faceName);
        if (faceTransform != null)
        {
            return faceTransform.gameObject;
        }
        else
        {
            Debug.LogError("Face object with the name '" + faceName + "' not found.");
            return null;
        }
    }
}
