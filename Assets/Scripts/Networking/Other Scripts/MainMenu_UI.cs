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
    [Header("Screen")]
    [SerializeField] private GameObject screenMainMenu;
    [SerializeField] private GameObject screenConnectWithSteam;
    [SerializeField] private GameObject screenLocal;
    [SerializeField] private GameObject screenOptions;
    [SerializeField] private GameObject screenLobbyList;
    [SerializeField] public GameObject screenNavigation;
    [SerializeField] public GameObject screenLoading;
    [SerializeField] public GameObject screenError;
    [Header("Other")]
    [SerializeField] private SteamLobby steamLobby;
    [SerializeField] private TMP_InputField customIPAddress;

    public static MainMenu_UI singleton;

    private void Awake()
    {
        singleton = this;
        //if (NetworkClient.isConnected)
        //{
        //    Destroy(mainMenuUIGameObject);
        //}
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    #region Configuration
    
    
    
    #endregion

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
        }

        if (screenLobbyList && screenLobbyList.activeSelf)
        {
            screenLobbyList.SetActive(false);
            screenMainMenu.SetActive(true);
        }

        if (screenLocal.activeSelf)
        {
            screenLocal.SetActive(false);
            screenMainMenu.SetActive(true);
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


    }

    public void OnClickButtonOptions()
    {
        // Disable main menu screen
        screenMainMenu.SetActive(false);

        // Enable options screen
        screenOptions.SetActive(true);
        
    }

    #endregion

    #region Screen: Connect with Steam

    public void HostSteamLobby()
    {
        steamLobby.HostLobby();
        
        //mainMenuUIGameObject.SetActive(false);
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
        
        //mainMenuUIGameObject.SetActive(false);
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
}
