using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using FlutterUnityIntegration;

public class SceneLoader : MonoBehaviour

{

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("SceneLoader Awake called");
        UnityMessageManager.Instance.SendMessageToFlutter($"SceneLoader Awake called");

    }


    public void GetActiveScene()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        UnityMessageManager.Instance.SendMessageToFlutter($"ACTIVE_SCENE:{activeSceneName}");
    }

    public void LoadSceneByName(string sceneName)
    {
        Debug.Log($"Loading scene with name: {sceneName}");
        UnityMessageManager.Instance.SendMessageToFlutter($"Loading scene with name: {sceneName}");

        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).completed += (asyncOperation) =>
        {
            UnityMessageManager.Instance.SendMessageToFlutter("SCENE_SWITCHED");
        };
    }

    private IEnumerator NotifySceneLoaded()
    {
        // Wait for the scene to be fully loaded
        yield return new WaitForSeconds(0.5f);
        UnityMessageManager.Instance.SendMessageToFlutter("AR_SCENE_LOADED");
    }

    public void LoadScene(int buildIndex)
    {
        Debug.Log($"Loading scene with build index: {buildIndex}");
        SceneManager.LoadScene(buildIndex);
        StartCoroutine(NotifySceneLoaded());
    }
}