using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalLoadingScreen : MonoBehaviour
{
    [Scene] [SerializeField] private string gameScene;
    
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
        if (scene.path == gameScene)
        {
            gameObject.GetComponent<Canvas>().enabled = false;
        }
    }
}
