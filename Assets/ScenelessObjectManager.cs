using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenelessObjectManager : MonoBehaviour
{
    [SerializeField] private GlobalSettings globalSettings;
    [SerializeField] private GameObject settingButton;
    private CanvasRenderer[] _canvasRenderers;
    [Scene] [SerializeField] private string roomScene, gameScene;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

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
        if (scene.path == roomScene || scene.path == gameScene)
        {
            settingButton.SetActive(false);
        }
        else
        {
            settingButton.SetActive(true);
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
