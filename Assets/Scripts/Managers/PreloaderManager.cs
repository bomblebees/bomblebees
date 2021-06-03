using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloaderManager : MonoBehaviour
{
    [Header("Required")]
    [Scene] public string mainMenuScene;
    public GameObject sessionLogger;
    public GameObject steamNetworkManager;
    public GameObject localNetworkManager;
    public GameObject mainMenuUI;
    public GameObject lobbyUI;

    [Header("Configurations")]
    [Tooltip("Controls whether to play through the splash screen animation or not.")]
    public bool playSplashScreen = true;

    [Tooltip("If enabled, will use the steam lobby and steam connection transport. If disabled, will use room network manager.")]
    public bool useSteamLobbyNetworkManager = true;

    private void Awake()
    {
        mainMenuUI.GetComponent<MainMenu_UI>().playSplashScreen = playSplashScreen;

        if (useSteamLobbyNetworkManager)
        {
            steamNetworkManager.SetActive(true);
            mainMenuUI.transform.SetParent(steamNetworkManager.transform);

            // Carry over Lobby UI
            DontDestroyOnLoad(lobbyUI);
        }
        else
        {
            localNetworkManager.SetActive(true);
            mainMenuUI.transform.SetParent(localNetworkManager.transform);
        }

        Preload();
    }

    private void Preload()
    {


        // Anything that needs to persist past Preload scene goes 
        DontDestroyOnLoad(sessionLogger);

        // Load menu scene
        SceneManager.LoadScene(mainMenuScene);
    }
}