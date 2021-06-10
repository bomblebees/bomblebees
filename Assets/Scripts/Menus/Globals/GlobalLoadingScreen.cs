using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalLoadingScreen : MonoBehaviour
{
    [Scene] [SerializeField] private string roomScene;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.path == roomScene) return;
        gameObject.GetComponent<Canvas>().enabled = false;
    }
}
