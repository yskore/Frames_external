using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using UnityEngine.SceneManagement;
using FlutterUnityIntegration;
using System;
using System.Collections.Generic;
using FramesAR.Data;
using System.Linq;



public class ARManager : MonoBehaviour
{
    public ARSession arSession;
    public ARCameraManager arCameraManager;

    private GameObject currentFrameObject;
    private TextureDownloader textureDownloader;
    private bool isFramePlaced = false;
    private float placementDistance = 3f;
    private Camera arCamera;
    private FrameData currentFrameData;
   // private LoadExistingPieces pieceLoader;



    [System.Serializable]
    public class PiecesDataWrapper
    {
        public List<PiecePost> pieces;
    }



    private bool isLocationEnabled = false;
    private LocationService location;

    private void Start()
    {
        Debug.Log("ARManager Start called");
        InitializeARComponents();
        InitializeTextureDownloader();
       // pieceLoader = FindObjectOfType<LoadExistingPieces>();  // Add this line
        StartCoroutine(EnableLocation());

    }

  

    //Load Test Piece
    public void LoadTestPiece()
    {
        float testLat = 51.59423828125f;
        float testLong = -0.24676810204982758f;

        if (isLocationEnabled)
        {
            float distance = LocationUtils.CalculateDistance(
                Input.location.lastData.latitude,
                Input.location.lastData.longitude,
                testLat,
                testLong
            );

            if (distance > 100) // 100 meters
            {
                Debug.Log("Too far from piece location");
                return;
            }
        }

        Debug.Log("Loading test piece");
        GameObject framePrefab = Resources.Load<GameObject>("FrameObjects/original_frame");
        if (framePrefab == null)
        {
            Debug.LogError("Failed to load original_frame prefab");
            return;
        }

        Vector3 position = new Vector3(-42.15769958496094f, -2.3932507038116455f, 13.672657012939453f);
        Quaternion rotation = new Quaternion(0.17216292023658752f, -0.05870966985821724f,
                                           -0.014347316697239876f, 0.9832127094268799f);

        GameObject piece = Instantiate(framePrefab, position, rotation);

        FaceSelector faceSelector = piece.GetComponent<FaceSelector>();
        if (faceSelector != null)
        {
            GameObject faceObject = faceSelector.GetFaceObject("Face");
            if (faceObject != null)
            {
                textureDownloader.ApplyTextureToFace(faceObject,
                    "https://storage.googleapis.com/x-fabric-419423.appspot.com/piece_display/1000000031.jpg");
            }
        }

        // Create anchor
        GameObject anchorObject = new GameObject("PieceAnchor_test");
        anchorObject.transform.position = position;
        anchorObject.transform.rotation = rotation;
        piece.transform.parent = anchorObject.transform;
        anchorObject.AddComponent<ARAnchor>();
        Debug.Log("Test Piece Loaded & Anchored ");
    }
    //

    //Use CalculatePieceLocation when POSTING

    private (float latitude, float longitude) CalculatePieceLocation(Vector3 piecePosition)
    {
        // Get device location
        float deviceLat = Input.location.lastData.latitude;
        float deviceLon = Input.location.lastData.longitude;

        // Calculate distance from device to piece in meters
        float distance = Vector3.Distance(arCamera.transform.position, piecePosition);

        // Get forward direction in world space
        Vector3 forward = arCamera.transform.forward;

        // Calculate bearing angle in radians
        float bearing = Mathf.Atan2(forward.x, forward.z);

        // Convert to degrees and normalize to 0-360
        float bearingDegrees = bearing * Mathf.Rad2Deg;
        if (bearingDegrees < 0) bearingDegrees += 360f;

        // Calculate new coordinates using distance and bearing
        float lat2 = Mathf.Asin(
            Mathf.Sin(deviceLat * Mathf.Deg2Rad) * Mathf.Cos(distance / 6371000f) +
            Mathf.Cos(deviceLat * Mathf.Deg2Rad) * Mathf.Sin(distance / 6371000f) * Mathf.Cos(bearing)
        ) * Mathf.Rad2Deg;

        float lon2 = deviceLon + Mathf.Atan2(
            Mathf.Sin(bearing) * Mathf.Sin(distance / 6371000f) * Mathf.Cos(deviceLat * Mathf.Deg2Rad),
            Mathf.Cos(distance / 6371000f) - Mathf.Sin(deviceLat * Mathf.Deg2Rad) * Mathf.Sin(lat2 * Mathf.Deg2Rad)
        ) * Mathf.Rad2Deg;

        return (lat2, lon2);
    }

    private IEnumerator EnableLocation()
    {
        Debug.Log("Checking Location Status");
        // Request location permissions
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location services not enabled");
            UnityMessageManager.Instance.SendMessageToFlutter("LOCATION_PERMISSION_REQUIRED");
            yield break;
        }

        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.Log("Location service initialization timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        isLocationEnabled = true;
        Debug.Log($"Location enabled: latitude is {Input.location.lastData.latitude} and longitude is {Input.location.lastData.longitude}");
    }

    private IEnumerator LoadAndSetupARFrame(string jsonMessage)
    {
        Debug.Log($"Received data from Flutter: {jsonMessage}");

        //Set FrameData

        try
        {
            currentFrameData = JsonUtility.FromJson<FrameData>(jsonMessage);
            if (currentFrameData == null)
            {
                Debug.LogError("Invalid data format received from Flutter.");
                yield break;
            }

            Debug.Log($"Parsed FrameData: frameName={currentFrameData.frameName}, faceName={currentFrameData.faceName}, imageUrl={currentFrameData.imageUrl}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse JSON message: {e.Message}");
            yield break;
        }

        //data is set 
        FrameData data = null;

        try
        {
            data = JsonUtility.FromJson<FrameData>(jsonMessage);
            if (data == null)
            {
                Debug.LogError("Invalid data format received from Flutter.");
                yield break;
            }

            Debug.Log($"Parsed FrameData: frameName={data.frameName}, faceName={data.faceName}, imageUrl={data.imageUrl}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse JSON message: {e.Message}");
            yield break;
        }

        // Create the frame object but don't place it in AR view yet
        GameObject framePrefab = Resources.Load<GameObject>($"FrameObjects/{data.frameName}");
        if (framePrefab == null)
        {
            Debug.LogError($"Failed to load frame prefab: {data.frameName}");
            yield break;
        }

        // Create and hide the frame initially
        currentFrameObject = Instantiate(framePrefab);
        currentFrameObject.SetActive(false);

        // Get and setup the face object
        FaceSelector faceSelector = currentFrameObject.GetComponent<FaceSelector>();
        if (faceSelector != null)
        {
            GameObject faceObject = faceSelector.GetFaceObject(data.faceName);
            if (faceObject != null)
            {
                // Apply texture to the face
                textureDownloader.ApplyTextureToFace(faceObject, data.imageUrl);

                // Wait for texture to be applied
                yield return new WaitForEndOfFrame();
            }
            else
            {
                Debug.LogError($"Face object '{data.faceName}' not found");
                Destroy(currentFrameObject);
                yield break;
            }
        }

        // Position the frame in front of the AR camera
        if (arCamera != null)
        {
            currentFrameObject.transform.position = arCamera.transform.position + arCamera.transform.forward * placementDistance;
            currentFrameObject.transform.rotation = arCamera.transform.rotation;
            currentFrameObject.SetActive(true);
            Debug.Log($"Piece with image: {currentFrameData.imageUrl} made active");
        }

        DisableRotationEnableMovement();
        Debug.Log("[AR Placement] rotateObject script disabled");
        isFramePlaced = true;
        Debug.Log("Frame loaded in AR view");
        UnityMessageManager.Instance.SendMessageToFlutter("FRAME_LOADED");
    }

    public void PostPiece()
    {

        Debug.Log("Post Piece called from unity");

        if (!isFramePlaced || currentFrameObject == null)
        {
            Debug.LogError("No frame to post");
            UnityMessageManager.Instance.SendMessageToFlutter("NO_FRAME_TO_POST");
            return;
        }

        if (!isLocationEnabled)
        {
            Debug.LogError("Location services not available");
            UnityMessageManager.Instance.SendMessageToFlutter("LOCATION_SERVICES_REQUIRED");
            return;
        }

        if (currentFrameData == null)
        {
            Debug.LogError("No frame data available");
            UnityMessageManager.Instance.SendMessageToFlutter("NO_FRAME_DATA");
            return;
        }

        var pieceLocation = CalculatePieceLocation(currentFrameObject.transform.position);


        PiecePost post = new PiecePost
        {
            anchorId = System.Guid.NewGuid().ToString(),
            frameName = currentFrameData.frameName,
            faceName = currentFrameData.faceName,
            imageUrl = currentFrameData.imageUrl,
            latitude = pieceLocation.latitude, //Actual long and lat of the AR piece is used here and below
            longitude = pieceLocation.longitude,
            arPosition = currentFrameObject.transform.position,
            arRotation = currentFrameObject.transform.rotation,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // Optionally, anchor the frame to make it stable in AR space
        StartCoroutine(AnchorPiece(post));

        // Convert to JSON to send to Flutter
        string jsonData = JsonUtility.ToJson(post);
        UnityMessageManager.Instance.SendMessageToFlutter($"FRAME_POST_DATA:{jsonData}");



    }

    private IEnumerator AnchorPiece(PiecePost post)
    {
        Debug.Log("Attempting to Anchor Piece");
        try
        {
            GameObject anchorObject = new GameObject($"PieceAnchor_{post.anchorId}");
            anchorObject.transform.position = post.arPosition;
            anchorObject.transform.rotation = post.arRotation;

            ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();

            if (anchor != null)
            {
                currentFrameObject.transform.parent = anchorObject.transform;
                Debug.Log($"Piece anchored successfully");
                UnityMessageManager.Instance.SendMessageToFlutter("PIECE_ANCHORED");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error anchoring piece: {e.Message}");
            UnityMessageManager.Instance.SendMessageToFlutter("ANCHOR_FAILED");
        }
        yield return null;
    }


    private void InitializeTextureDownloader()
    {
        textureDownloader = FindObjectOfType<TextureDownloader>();
        if (textureDownloader == null)
        {
            GameObject textureDownloaderObject = new GameObject("TextureDownloader");
            textureDownloader = textureDownloaderObject.AddComponent<TextureDownloader>();
            Debug.Log("TextureDownloader created [AR Placement]");
        }
    }

    private void InitializeARComponents()
    {
        if (arSession == null)
            arSession = FindObjectOfType<ARSession>();

        if (arCameraManager == null)
            arCameraManager = FindObjectOfType<ARCameraManager>();

        if (arCameraManager != null)
            arCamera = arCameraManager.gameObject.GetComponent<Camera>();

        if (arSession != null && arCameraManager != null)
        {
            Debug.Log("AR Components Initialized");
            UnityMessageManager.Instance.SendMessageToFlutter("AR_COMPONENTS_INITIALIZED");
        }
        else
        {
            Debug.LogError("Failed to initialize all AR components");
            UnityMessageManager.Instance.SendMessageToFlutter("AR_INITIALIZATION_FAILED");
        }
    }

    public void ShowARView()
    {
        Debug.Log("Switching to AR view");
        try
        {
            if (arSession != null)
            {
                arSession.enabled = true;
            }

            if (arCameraManager != null)
            {
                arCameraManager.gameObject.SetActive(true);
            }

            UnityMessageManager.Instance.SendMessageToFlutter("AR_VIEW_ACTIVE");
            Debug.Log("AR view activated successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to switch to AR view: {e.Message}");
            UnityMessageManager.Instance.SendMessageToFlutter("AR_VIEW_FAILED");
        }
    }

    public void LoadFrameInAR(string jsonMessage)
    {
        Debug.Log("Loading frame in AR view");
        StartCoroutine(LoadAndSetupARFrame(jsonMessage));
    }



    private void DisableRotationEnableMovement()
    {
        var rotateScript = currentFrameObject.GetComponent<RotateObject>();
        if (rotateScript != null)
        {
            rotateScript.enabled = false;
            Debug.Log("Rotate script disabled for AR mode");
        }

        var pieceMovement = currentFrameObject.GetComponent<Piece_movement>();
        if (pieceMovement != null)
        {
            pieceMovement.enabled = true;
            Debug.Log("Piece movement script enabled for AR mode");
        }
    }




    public void SwitchToPreview(string message)
    {
        Debug.Log("Switching to frames_test Scene");
        try
        {
            SceneManager.LoadScene("frames_test");
            Debug.Log("Scene switched to frames_test");
            UnityMessageManager.Instance.SendMessageToFlutter("Scene switched to frames_test");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error, Failed to restart scene: {e.Message}");
            UnityMessageManager.Instance.SendMessageToFlutter("Failed to switch Scene to frames_test");
        }
    }

    //----------------------------------Piece Loading Funcitonality--------------------------------------

 
}
