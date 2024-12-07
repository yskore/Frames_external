using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TextureDownloader : MonoBehaviour
{
    public void ApplyTextureToFace(GameObject faceObject, string imageUrl)
    {
        Debug.Log($"ApplyTextureToFace called. faceObject: {(faceObject != null ? faceObject.name : "null")}, imageUrl: {imageUrl}");

        if (faceObject == null || string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError($"Face object or image URL is not specified! faceObject: {(faceObject != null ? faceObject.name : "null")}, imageUrl: {imageUrl}");
            return;
        }
        StartCoroutine(DownloadAndApplyTexture(faceObject, imageUrl));
    }

    private IEnumerator DownloadAndApplyTexture(GameObject faceObject, string url)
    {
        Debug.Log("Downloading texture from URL: " + url);
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download texture: " + uwr.error);
            }
            else
            {
                Debug.Log("Texture downloaded successfully");
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                ApplyTextureToFaceObject(faceObject, texture);
            }
        }
    }

    private void ApplyTextureToFaceObject(GameObject faceObject, Texture2D texture)
    {
        if (faceObject == null)
        {
            Debug.LogError("Face object is null when trying to apply texture");
            return;
        }

        MeshRenderer renderer = faceObject.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer component not found on the face object");
            return;
        }

        Material material = new Material(Shader.Find("Standard"));
        material.mainTexture = texture;
        material.mainTextureScale = new Vector2(1, 1);
        renderer.material = material;
        Debug.Log("Texture applied to material successfully");

        AdjustUVCoordinates(faceObject);
    }

    private void AdjustUVCoordinates(GameObject faceObject)
    {
        MeshFilter meshFilter = faceObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component not found on the face object");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
        }

        Vector2 minUV = uvs[0];
        Vector2 maxUV = uvs[0];
        for (int i = 1; i < uvs.Length; i++)
        {
            minUV = Vector2.Min(minUV, uvs[i]);
            maxUV = Vector2.Max(maxUV, uvs[i]);
        }
        Vector2 size = maxUV - minUV;
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = (uvs[i] - minUV) / size;
        }
        mesh.uv = uvs;
        Debug.Log("UV coordinates adjusted successfully");
    }
}