using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Authenticators;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu_UI : MonoBehaviour
{
    [Scene][SerializeField] private string mainMenuScene;
    [Header("MainMenu_UI GameObject")]
    [SerializeField] private GameObject mainMenuUIGameObject;
    [Header("Network Manager")]
    [SerializeField] private NetworkManager steamNetworkManager;
    [SerializeField] private NetworkManager kcpNetworkManager;
    [Header("Network Manager GameObject")]
    [SerializeField] private GameObject steamNetworkManagerGameObject;
    [SerializeField] private GameObject kcpNetworkManagerGameObject;
    [Header("Canvases")]
    [SerializeField] public Canvas menuCanvas;
    [SerializeField] public Canvas splashCanvas;
    [Header("Screen")]
    [SerializeField] private GameObject screenMainMenu;
    [SerializeField] private GameObject screenConnectWithSteam;
    [SerializeField] private GameObject screenLocal;
    [SerializeField] private GameObject screenOptions;
    [SerializeField] private GameObject screenLobbyList;
    [SerializeField] public GameObject screenNavigation;
    [SerializeField] public GameObject screenLoading;
    [SerializeField] public GameObject screenError;
    [SerializeField] public GameObject screenCredits;
    [SerializeField] public GameObject screenControls;
	[SerializeField] public GameObject screenHowToPlay;
	[Header("Other")]
    [SerializeField] private SteamLobby steamLobby;
    [SerializeField] private TMP_InputField customIPAddress;
    [SerializeField] private GameObject backButton;

    public static MainMenu_UI singleton;

    public bool playSplashScreen;

    private void Awake()
    {
        singleton = this;
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

            //GameObject.Find("Main Camera").transform.localRotation = Quaternion.Euler(-30, 0, 0);
        }
    }

    #region Continous

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {
        if (screenOptions.activeSelf)
        {
            screenOptions.SetActive(false);
            screenMainMenu.SetActive(true);
            backButton.SetActive(false);
        }

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
        } else
        {
            screenLocal.SetActive(true);
        }
        
        // Enable back button
        backButton.SetActive(true);
    }

    public void OnClickButtonOptions()
    {
        // Disable main menu screen
        screenMainMenu.SetActive(false);

        // Enable options screen
        screenOptions.SetActive(true);
        
    }

    public void OnClickButtonControls()
    {
        // Disable main menu screen
        screenMainMenu.SetActive(false);

        // Enable controls screen
        screenControls.SetActive(true);
    }

    #endregion

    #region Screen: Controls

    public void OnClickButtonExitControls()
    {
        // Enable main menu screen
        screenMainMenu.SetActive(true);

        // Disable controls screen
        screenControls.SetActive(false);
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

	#region Screen: Connect with Steam

	public void HostSteamLobby()
    {
        steamLobby.HostLobby();
    }

    public void JoinFriendsLobby()
    {
        screenConnectWithSteam.SetActive(false);
        screenLobbyList.SetActive(true);
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
    
    public void Options()
    {
        if (steamNetworkManagerGameObject.activeSelf)
        {
            screenConnectWithSteam.SetActive(false);
            screenOptions.SetActive(true);
        }

        if (kcpNetworkManagerGameObject.activeSelf)
        {
            screenLocal.SetActive(false);
            screenOptions.SetActive(true);
        }
    }

    #endregion

    #region Screen: Credits

    public void ToggleCredits()
    {
        screenCredits.SetActive(!screenCredits.activeSelf);
    }

    #endregion
}
