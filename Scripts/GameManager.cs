using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using FlutterUnityIntegration;




[Serializable]
public class FrameData
{
    public string frameName;
    public string faceName;
    public string imageUrl;
    

}

public class GameManager : MonoBehaviour
{
    public Camera nonARCamera;
    public Camera arCamera;
    public ARSession arSession;
    private TextureDownloader textureDownloader;
    private UnityMessageManager manager;


    void Start()
    {
        Debug.Log("GameManager Start method started");




        // Find or create TextureDownloader
        textureDownloader = FindObjectOfType<TextureDownloader>();
        if (textureDownloader == null)
        {
            GameObject textureDownloaderObject = new GameObject("TextureDownloader");
            textureDownloader = textureDownloaderObject.AddComponent<TextureDownloader>();
            Debug.Log("TextureDownloader created");
        }
        else
        {
            Debug.Log("Existing TextureDownloader found");
           
        }
        
    }


    public void ResetUnityScene(string message)
    {
        Debug.Log("Restarting Unity Scene");
        try
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Debug.Log("Scene restarted successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"error, Failed to restart scene: {e.Message}");
        }
    }

    public void ReceiveDataFromFlutter(string jsonMessage)
    {
        Debug.Log($"Received data from Flutter: {jsonMessage}");
        try
        {
            FrameData data = JsonUtility.FromJson<FrameData>(jsonMessage);
            if (data != null)
            {
                SwitchToNonARCamera();
                Debug.LogError("Switched to preview mode (NonARCamera)");


                Debug.Log($"Parsed FrameData: frameName={data.frameName}, faceName={data.faceName}, imageUrl={data.imageUrl}");
                LoadFrameObject(data.frameName, data.faceName, data.imageUrl);
            }
            else
            {
                Debug.LogError("Invalid data format received from Flutter.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse JSON message: {e.Message}");
        }
    } 

    public void LoadFrameObject(string frameName, string faceName, string imageUrl)
    {
        Debug.Log($"LoadFrameObject called with frameName={frameName}, faceName={faceName}, imageUrl={imageUrl}");
        StartCoroutine(LoadAndSetupFrame(frameName, faceName, imageUrl));
    }

    private IEnumerator LoadAndSetupFrame(string frameName, string faceName, string imageUrl)
    {
        Debug.Log($"Loading frame prefab: {frameName}");
        GameObject framePrefab = Resources.Load<GameObject>($"FrameObjects/{frameName}");
        if (framePrefab == null)
        {
            Debug.LogError($"Failed to load frame prefab: {frameName}");
            yield break;
        }

        Debug.Log($"Frame prefab loaded successfully: {frameName}");
        GameObject frameObject = Instantiate(framePrefab);
        Debug.Log($"Frame object instantiated: {frameObject.name}");

        FaceSelector faceSelector = frameObject.GetComponent<FaceSelector>();
        if (faceSelector == null)
        {
            Debug.LogError("FaceSelector component not found on the frame object.");
            Destroy(frameObject);
            yield break;
        }

        Debug.Log($"Attempting to get face object: {faceName}");
        GameObject faceObject = faceSelector.GetFaceObject(faceName);
        if (faceObject == null)
        {
            Debug.LogError($"Face object '{faceName}' not found in the loaded frame object.");
            Destroy(frameObject);
            yield break;
        }

        Debug.Log($"Face object found: {faceObject.name}");

        if (textureDownloader == null)
        {
            Debug.LogError("TextureDownloader is null. This should not happen as it's initialized in Awake.");
            Destroy(frameObject);
            yield break;
        }

        // Apply texture
        Debug.Log($"Applying texture to face object: {faceObject.name}, with URL: {imageUrl}");
        textureDownloader.ApplyTextureToFace(faceObject, imageUrl);

        // Wait a frame to ensure texture is applied
        yield return new WaitForEndOfFrame();

        // Add and setup ResizeObject component
        ResizeObject resizeComponent = frameObject.AddComponent<ResizeObject>();
        resizeComponent.SetupFaceObject(faceName);


        Debug.Log("Frame setup complete - Resize functionality added");
    }
    public void SwitchToARCamera()
    {
        if (AreARComponentsReady())
        {
            nonARCamera.gameObject.SetActive(false);
            arCamera.gameObject.SetActive(true);
            arSession.enabled = true;
            Debug.Log("Switched to AR Camera");
        }
        else
        {
            Debug.LogError("Cannot switch to AR Camera: Some components are missing");
        }
    }


    public void SwitchToNonARCamera()
    {
        Debug.LogError("Switching cameras for preview mode...");

        if (nonARCamera != null)
        {
            nonARCamera.gameObject.SetActive(true);
            Debug.LogError($"NonARCamera enabled: {nonARCamera.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("NonARCamera reference is null!");
        }

        if (arCamera != null)
        {
            arCamera.gameObject.SetActive(false);
            Debug.LogError("ARCamera disabled");
        }
        else
        {
            Debug.LogError("ARCamera reference is null");
        }

        if (arSession != null)
        {
            arSession.enabled = false;
            Debug.LogError("AR Session disabled");
        }
        else
        {
            Debug.LogError("ARSession reference is null");
        }
    }
    public void CheckARComponents()
    {
        bool allComponentsFound = AreARComponentsReady();

        string message = allComponentsFound
            ? "AR Components Found"
            : $"AR Components Check - ARSession: {(arSession != null ? "Found" : "Not Found")}, " +
              $"ARCamera: {(arCamera != null ? "Found" : "Not Found")}, " +
              $"NonARCamera: {(nonARCamera != null ? "Found" : "Not Found")}";

        Debug.Log(message);
        UnityMessageManager.Instance.SendMessageToFlutter(message);

    }

    public bool AreARComponentsReady()
    {
        
            arSession = FindObjectOfType<ARSession>();
            arCamera = FindObjectOfType<ARCameraManager>().GetComponent<Camera>();

        bool allComponentsFound = (arSession != null) && (arCamera != null);

        string arSessionInfo = arSession != null ? $"Found (Running: {arSession.enabled})" : "Not Found";
        string arCameraInfo = arCamera != null ? $"Found (Name: {arCamera.name}, Tag: {arCamera.tag})" : "Not Found";

        Debug.Log($"AR Components Check - ARSession: {arSessionInfo}, " +
                  $"ARCamera: {arCameraInfo}");

        return allComponentsFound;
    }


}