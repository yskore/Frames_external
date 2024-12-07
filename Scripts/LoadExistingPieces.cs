using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using UnityEngine.SceneManagement;
using FlutterUnityIntegration;
using System;
using System.Collections.Generic;
using FramesAR.Data;
using System.Linq;

[System.Serializable]
public class PiecesDataWrapper
{
    public List<PiecePost> pieces;
}

public class LoadExistingPieces : MonoBehaviour
{
    private bool isLocationEnabled = false;  // Add th
    private Camera arCamera;
    private const float LOAD_RADIUS = 100f;
    private const float LOAD_INTERVAL = 2f;
    private const float UNLOAD_DISTANCE = 120f;
    private Dictionary<string, GameObject> loadedPieces = new Dictionary<string, GameObject>();
    private Queue<PiecePost> pieceLoadQueue = new Queue<PiecePost>();

    private TextureDownloader textureDownloader;



    private void Start()
    {
        arCamera = Camera.main;
        textureDownloader = GetComponent<TextureDownloader>();
        if (textureDownloader == null)
        {
            textureDownloader = gameObject.AddComponent<TextureDownloader>();
        }
        StartCoroutine(EnableLocation());  // Add location initialization

    }

    private IEnumerator EnableLocation()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location services not enabled");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Location services initialization failed");
            yield break;
        }

        isLocationEnabled = true;

        if (isLocationEnabled)
        {
            UnityMessageManager.Instance.SendMessageToFlutter("LOCATION_ENABLED");
        }
        else
        {
            UnityMessageManager.Instance.SendMessageToFlutter("LOCATION_FAILED");
        }
    }

    public void LoadNearbyPieces(string jsonData)
    {
        Debug.Log("UNITY: LoadNearbyPieces called");
        Debug.Log($"UNITY: DataAnchor data received from flutter: {jsonData}");
        try
        {
            PiecesDataWrapper wrapper = JsonUtility.FromJson<PiecesDataWrapper>(jsonData);
            if (wrapper == null || wrapper.pieces == null)
            {
                Debug.LogError("Invalid or empty pieces data received");
                return;
            }
            pieceLoadQueue.Clear();
            Debug.Log("UNITY: Piece Load Queue Cleared ");
            foreach (var piece in wrapper.pieces)
            {
                if (loadedPieces.ContainsKey(piece.anchorId))
                    continue;
                float distance = LocationUtils.CalculateDistance(
                    Input.location.lastData.latitude,
                    Input.location.lastData.longitude,
                    piece.latitude,
                    piece.longitude
                );
                if (distance <= LOAD_RADIUS)
                {
                    pieceLoadQueue.Enqueue(piece);
                    Debug.Log("UNITY: Piece Enqueued");
                }
            }
            if (pieceLoadQueue.Count > 0)
            {
                Debug.Log("UNITY: Calling LoadPiecesGradually()");
                StartCoroutine(LoadPiecesGradually());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to process nearby pieces: {e.Message}");
        }
    }


    private IEnumerator LoadSinglePiece(PiecePost piece)
    {
        Debug.Log($"UNITY: Starting LoadSinglePiece for piece {piece.anchorId}");

        GameObject framePrefab = Resources.Load<GameObject>($"FrameObjects/{piece.frameName}");
        if (framePrefab == null)
        {
            Debug.LogError($"UNITY: Prefab not found: {piece.frameName}");
            yield break;
        }
        Debug.Log("UNITY: Prefab loaded successfully");

        Vector3 piecePosition = LocationUtils.CalculatePiecePosition(piece, arCamera);
        GameObject newPiece = Instantiate(framePrefab, piecePosition, piece.arRotation);
        Debug.Log($"UNITY: Piece instantiated at position: {newPiece.transform.position}");
        Debug.Log($"UNITY: Piece active state before hiding: {newPiece.activeInHierarchy}");

        newPiece.SetActive(false);

        FaceSelector faceSelector = newPiece.GetComponent<FaceSelector>();
        if (faceSelector == null)
        {
            Debug.LogError("UNITY: No FaceSelector component found on piece");
            yield break;
        }
        Debug.Log("UNITY: FaceSelector found");

        GameObject faceObject = faceSelector.GetFaceObject(piece.faceName);
        if (faceObject == null)
        {
            Debug.LogError($"UNITY: Face object not found with name: {piece.faceName}");
            yield break;
        }
        Debug.Log("UNITY: Face object found");

        textureDownloader.ApplyTextureToFace(faceObject, piece.imageUrl);
        yield return new WaitForEndOfFrame();
        Debug.Log("UNITY: Texture applied");

        // Create anchor and set up hierarchy
        GameObject anchorObject = new GameObject($"Anchor_{piece.anchorId}");
        anchorObject.transform.position = piecePosition;
        anchorObject.transform.rotation = piece.arRotation;
        newPiece.transform.parent = anchorObject.transform;
        anchorObject.AddComponent<ARAnchor>();
        Debug.Log($"UNITY: Anchor created at position: {anchorObject.transform.position}");

        newPiece.SetActive(true);
        Debug.Log($"UNITY: Piece active state after showing: {newPiece.activeInHierarchy}");
        Debug.Log($"UNITY: Final piece world position: {newPiece.transform.position}");

        loadedPieces.Add(piece.anchorId, newPiece);
    }

    // Update LoadPiecesGradually to use coroutine
    private IEnumerator LoadPiecesGradually()
    {
        Debug.Log($"Starting to load {pieceLoadQueue.Count} pieces gradually");
        int totalPieces = pieceLoadQueue.Count;
        int currentPieceNumber = 0;

        while (pieceLoadQueue.Count > 0)
        {
            var piece = pieceLoadQueue.Dequeue();
            currentPieceNumber++;

            Debug.Log($"Loading piece {currentPieceNumber}/{totalPieces}:");
            Debug.Log($"  Anchor ID: {piece.anchorId}");
            Debug.Log($"  Frame Name: {piece.frameName}");
            Debug.Log($"  Location: ({piece.latitude}, {piece.longitude})");
            Debug.Log($"  Pieces remaining in queue: {pieceLoadQueue.Count}");

            yield return StartCoroutine(LoadSinglePiece(piece));

            float waitTime = LOAD_INTERVAL / (pieceLoadQueue.Count + 1);
            yield return new WaitForSeconds(waitTime);
        }

        Debug.Log("Finished loading all pieces");
    }

    private void SetupPieceVisuals(GameObject piece, PiecePost pieceData)
    {
        FaceSelector faceSelector = piece.GetComponent<FaceSelector>();
        if (faceSelector != null)
        {
            GameObject faceObject = faceSelector.GetFaceObject(pieceData.faceName);
            if (faceObject != null)
            {
                var textureDownloader = GetComponent<TextureDownloader>();
                if (textureDownloader != null)
                {
                    textureDownloader.ApplyTextureToFace(faceObject, pieceData.imageUrl);
                }
            }
        }
    }

    // Modify Update method to only handle visibility based on distance
  /*  private void Update()
    {
        Debug.Log("Updated called");
        if (!isLocationEnabled) return;

        foreach (var kvp in loadedPieces.ToList())
        {
            float distance = Vector3.Distance(
                arCamera.transform.position,
                kvp.Value.transform.position
            );

            // Instead of destroying, just toggle visibility
            if (distance > UNLOAD_DISTANCE)
            {
                kvp.Value.SetActive(false);
            }
            else if (distance <= LOAD_RADIUS && !kvp.Value.activeSelf)
            {
                kvp.Value.SetActive(true);
            }
        }
    } */
}