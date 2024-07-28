using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TextureDownloader : MonoBehaviour
{
    public GameObject faceObject; // Reference to the face object
    public string imageUrl; // URL of the image

    void Start()
    {
        // No longer use Start() to automatically download and apply texture.
    }

    // Method to start downloading and applying the texture
    public void StartDownloadAndApplyTexture()
    {
        if (faceObject == null || string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("Face object or image URL is not specified!");
            return;
        }

        StartCoroutine(DownloadAndApplyTexture(imageUrl));
    }

    IEnumerator DownloadAndApplyTexture(string url)
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
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                // Create a new material and assign the texture
                Material material = new Material(Shader.Find("Standard"));
                material.mainTexture = texture;
                material.mainTextureScale = new Vector2(1, 1);

                // Apply the material to the face
                MeshRenderer renderer = faceObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                    Debug.Log("Texture applied to material successfully!");

                    // Adjust the UV coordinates to make the texture fit the entire face without tiling
                    MeshFilter meshFilter = faceObject.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        Mesh mesh = meshFilter.mesh;
                        Vector2[] uvs = new Vector2[mesh.vertices.Length];

                        // Set UVs to span the entire texture space
                        for (int i = 0; i < uvs.Length; i++)
                        {
                            uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
                        }

                        // Normalize the UVs to ensure they span from (0, 0) to (1, 1)
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
                        Debug.Log("UV coordinates adjusted successfully!");
                    }
                    else
                    {
                        Debug.LogError("MeshFilter component not found on the face object!");
                    }
                }
                else
                {
                    Debug.LogError("The specified face object does not have a MeshRenderer component!");
                }
            }
        }
    }
}
