using Mirror;
using TMPro;
using UnityEngine;

public class MainMenu_UI : MonoBehaviour
{
    [Header("Network Manager")] [SerializeField]
    private NetworkManager kcpNetworkManager;

    [Header("Canvas")] [SerializeField] public Canvas menuCanvas;
    [SerializeField] public Canvas splashCanvas;
    [Header("Screen")] [SerializeField] private GameObject screenMainMenu;
    [SerializeField] private GameObject screenLocal;
    [SerializeField] private GameObject screenLobbyList;
    [SerializeField] public GameObject screenNavigation;
    [SerializeField] public GameObject screenLoading;
    [SerializeField] public GameObject screenCredits;
    [SerializeField] public GameObject screenHowToPlay;
    [SerializeField] public GameObject screenShowcase;
    [SerializeField] private TMP_InputField customIPAddress;
    [SerializeField] private GameObject backButton;

    public static MainMenu_UI Singleton;

    public bool playSplashScreen;

    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        if (playSplashScreen)
        {
            splashCanvas.enabled = true;
        }
        else
        {
            splashCanvas.gameObject.SetActive(false);

            menuCanvas.GetComponent<FadeCanvas>().duration = 0;
            menuCanvas.GetComponent<FadeCanvas>().delay = 0;
            menuCanvas.GetComponent<FadeCanvas>().StartFadeIn();
        }
    }

    #region Continous

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {
        if (screenLobbyList && screenLobbyList.activeSelf)
        {
            screenLobbyList.SetActive(false);
            screenMainMenu.SetActive(true);
            backButton.SetActive(false);
        }

        if (screenLocal.activeSelf)
        {
            screenLocal.SetActive(false);
            screenMainMenu.SetActive(true);
            backButton.SetActive(false);
        }
    }

    #endregion

    #region Screen: Main Menu

    public void OnClickButtonPlay()
    {
        // Disable main menu screen
        screenMainMenu.SetActive(false);

        if (screenLobbyList)
        {
            // Enable steam lobby UI
            screenLobbyList.SetActive(true);
        }
        else
        {
            screenLocal.SetActive(true);
        }

        // Enable back button
        backButton.SetActive(true);
    }

    #endregion

    #region Screen: How To Play

    public void OnClickButtonHowToPlay()
    {
        // Disable main menu screen
        screenMainMenu.SetActive(false);

        // Enable How To Play screen
        screenHowToPlay.SetActive(true);
    }

    public void OnClickButtonExitHowToPlay()
    {
        // Enable main menu screen
        screenMainMenu.SetActive(true);

        // Disable How To Play screen
        screenHowToPlay.SetActive(false);
    }

    #endregion

    #region Screen: Local

    public void HostLocalLobby()
    {
        kcpNetworkManager.networkAddress = customIPAddress.ToString();
        kcpNetworkManager.StartHost();
    }

    public void JoinLocalLobby()
    {
        kcpNetworkManager.networkAddress = "localhost";
        kcpNetworkManager.StartClient();
    }

    public void JoinWithIPAddress()
    {
        kcpNetworkManager.StartClient();
    }

    #endregion

    #region Screen: Credits

    public void ToggleCredits()
    {
        screenCredits.SetActive(!screenCredits.activeSelf);
    }

    #endregion

    #region Screen: Showcase

    public void OnClickButtonShowcase()
    {
        screenShowcase.SetActive(true);
    }

    public void OnClickButtonExitShowcase()
    {
        screenShowcase.SetActive(false);
    }

    #endregion
}