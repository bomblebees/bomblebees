using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_EventSystem : MonoBehaviour
{
    public GameObject kcpNetworkManager;
    public GameObject steamNetworkManager;

    [SerializeField] private Button buttonInstantiateKcpNetworkManager = null;
    [SerializeField] private Button buttonInstantiateSteamNetworkManager = null;
    [SerializeField] private Button buttonInstantiateOfflineMode = null;

    private void Start()
    {
        buttonInstantiateKcpNetworkManager.onClick.AddListener(() => InstantiateKcpNetworkManager());
        buttonInstantiateSteamNetworkManager.onClick.AddListener(() => InstantiateSteamNetworkManager());
        buttonInstantiateOfflineMode.onClick.AddListener(() => InstantiateOfflineMode());
    }

    private void InstantiateKcpNetworkManager()
    {
        Instantiate(kcpNetworkManager, new Vector3(0,0,0), Quaternion.identity);

        ToggleInstantiateKcpNetworkManagerButton();
        ToggleInstantiateSteamNetworkManagerButton();
        ToggleInstantiateOfflineModeButton();
    }

    private void InstantiateSteamNetworkManager()
    {
        Instantiate(steamNetworkManager, new Vector3(0, 0, 0), Quaternion.identity);
        
        ToggleInstantiateKcpNetworkManagerButton();
        ToggleInstantiateSteamNetworkManagerButton();
        ToggleInstantiateOfflineModeButton();
    }

    private void InstantiateOfflineMode()
    {
        SceneManager.LoadScene("Level1");
    }
    
    private void ToggleInstantiateKcpNetworkManagerButton()
    {
        GameObject.Find("InstantiateKcpNetworkManagerButton")
            .SetActive(!GameObject.Find("InstantiateKcpNetworkManagerButton").activeSelf);
    }
    
    private void ToggleInstantiateSteamNetworkManagerButton()
    {
        GameObject.Find("InstantiateSteamNetworkManagerButton")
            .SetActive(!GameObject.Find("InstantiateSteamNetworkManagerButton").activeSelf);
    }
    
    private void ToggleInstantiateOfflineModeButton()
    {
        GameObject.Find("InstantiateOfflineModeButton")
            .SetActive(!GameObject.Find("InstantiateOfflineModeButton").activeSelf);
    }
}
