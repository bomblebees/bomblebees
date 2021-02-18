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
    [SerializeField] private GameObject screenSelectNetwork;
    [SerializeField] private GameObject screenConnectWithSteam;
    [SerializeField] private GameObject screenLocal;
    [SerializeField] private GameObject screenOptions;
    [SerializeField] private GameObject screenLobbyList;
    [Header("Other")]
    [SerializeField] private SteamLobby steamLobby;
    [SerializeField] private TMP_InputField customIPAddress;

    private void Awake()
    {
        if (NetworkClient.isConnected)
        {
            Destroy(mainMenuUIGameObject);
        }
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
            if (steamNetworkManagerGameObject.activeSelf)
            {
                screenOptions.SetActive(false);
                screenConnectWithSteam.SetActive(true);
            }

            if (kcpNetworkManagerGameObject.activeSelf)
            {
                screenOptions.SetActive(false);
                screenLocal.SetActive(true);
            }
        }
    }
    
    #endregion

    #region Screen: Select Network

    public void ConnectWithSteam()
    {
        screenSelectNetwork.SetActive(false);
        steamNetworkManagerGameObject.SetActive(true);
        screenConnectWithSteam.SetActive(true);
        
        mainMenuUIGameObject.transform.SetParent(steamNetworkManagerGameObject.transform);
    }

    public void LocalAreaNetwork()
    {
        screenSelectNetwork.SetActive(false);
        kcpNetworkManagerGameObject.SetActive(true);
        screenLocal.SetActive(true);
        
        mainMenuUIGameObject.transform.SetParent(kcpNetworkManagerGameObject.transform);
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
