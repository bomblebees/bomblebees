using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenelessObjectManager : MonoBehaviour
{
    [Header("Scene")] 
    [Scene] [SerializeField] public string roomScene;
    [Scene] [SerializeField] public string gameScene;
    
    [Header("Misc.")]
    [SerializeField] private GameObject globalSettings;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject pingDisplay;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.path == gameScene)
        {
            settingsButton.SetActive(false);
            pingDisplay.SetActive(true);
        } 
        else if (scene.path == roomScene)
        {
            settingsButton.SetActive(false);
            pingDisplay.SetActive(true);
        }
        else // Preload/MainMenu Scene
        {
            settingsButton.SetActive(true);
            pingDisplay.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ToggleSettingsScreen();
        }
    }

    public void ToggleSettingsScreen()
    {
        globalSettings.SetActive(!globalSettings.gameObject.activeSelf);
    }
}
