using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenelessObjectManager : MonoBehaviour
{
    [Header("Scene")]
    [Scene] [SerializeField] private string roomScene, gameScene;
    [Header("Misc.")]
    [SerializeField] private GlobalSettings globalSettings;
    [SerializeField] private GameObject settingButton;
    [SerializeField] private PingDisplay pingDisplay;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

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
        if (scene.path == roomScene || scene.path == gameScene)
        {
            settingButton.SetActive(false);
            pingDisplay.gameObject.SetActive(true);
        }
        else
        {
            settingButton.SetActive(true);
            pingDisplay.gameObject.SetActive(false);
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
        globalSettings.gameObject.SetActive(!globalSettings.gameObject.activeSelf);
    }
}
