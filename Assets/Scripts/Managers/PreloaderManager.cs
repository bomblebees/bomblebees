using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloaderManager : MonoBehaviour
{
    [Header("Required")]
    [Scene] public string mainMenuScene;
    public GameObject steamNetworkManager;
    public GameObject localNetworkManager;
    public GameObject mainMenuUI;
    public GameObject lobbyUI;

    [Header("Configurations")]
    [Tooltip("If enabled, will use the steam lobby and steam connection transport. If disabled, will use room network manager.")]
    public bool useSteamLobbyNetworkManager = true;

    private void Awake()
    {
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


        // Load menu scene
        SceneManager.LoadScene(mainMenuScene);
    }
}