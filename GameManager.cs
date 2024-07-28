using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

[Serializable]
public class FrameData
{
    public string frameName;
    public string faceName;
    public string imageUrl;
}

public class GameManager : MonoBehaviour
{
    private TextureDownloader textureDownloader;

    void Awake()
    {
        // Initialize TextureDownloader or other components that should be ready before Start
        textureDownloader = GetComponent<TextureDownloader>();
    }

    // This method will be called by UnityMessageManager to receive data from Flutter
    public void ReceiveDataFromFlutter(string jsonMessage)
    {
        try
        {
            // Deserialize the JSON message into a FrameData object
            FrameData data = JsonUtility.FromJson<FrameData>(jsonMessage);
            if (data != null)
            {
                // Call the method to load the frame object with the received data
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

    // Method to load the frame object and apply the texture
    public void LoadFrameObject(string frameName, string faceName, string imageUrl)
    {
        StartCoroutine(LoadAndSetupFrame(frameName, faceName, imageUrl));
    }

    private IEnumerator LoadAndSetupFrame(string frameName, string faceName, string imageUrl)
    {
        string bundleUrl = "file://" + Path.Combine(Application.streamingAssetsPath, "AssetBundles", "framebundle");

        // Log for debugging
        Debug.Log($"Loading AssetBundle from URL: {bundleUrl}");

        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load AssetBundle from URL: {bundleUrl}, Error: {uwr.error}");
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                if (bundle != null)
                {
                    // Load the specified frame prefab from the AssetBundle
                    GameObject framePrefab = bundle.LoadAsset<GameObject>(frameName);
                    if (framePrefab == null)
                    {
                        Debug.LogError($"Failed to load frame prefab: {frameName} from AssetBundle");
                        yield break;
                    }

                    // Instantiate the frame object
                    GameObject frameObject = Instantiate(framePrefab);

                    // Find the specified face object
                    FaceSelector faceSelector = frameObject.GetComponent<FaceSelector>();
                    if (faceSelector != null)
                    {
                        GameObject faceObject = faceSelector.GetFaceObject(faceName);
                        if (faceObject != null)
                        {
                            // Apply the texture to the face object
                            textureDownloader.faceObject = faceObject;
                            textureDownloader.imageUrl = imageUrl;
                            textureDownloader.StartDownloadAndApplyTexture();
                        }
                        else
                        {
                            Debug.LogError("Face object not found in the loaded frame object.");
                        }
                    }
                    else
                    {
                        Debug.LogError("FaceSelector component not found on the frame object.");
                    }
                }
            }
        }
    }
}
