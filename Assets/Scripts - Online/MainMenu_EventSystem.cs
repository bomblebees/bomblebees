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
    
    private bool _activeNetworkManager;

    private void Start()
    {
        _activeNetworkManager = false;
        
        buttonInstantiateKcpNetworkManager.onClick.AddListener(() => InstantiateKcpNetworkManager());
        buttonInstantiateSteamNetworkManager.onClick.AddListener(() => InstantiateSteamNetworkManager());
        buttonInstantiateOfflineMode.onClick.AddListener(() => InstantiateOfflineMode());
    }

    private void InstantiateKcpNetworkManager()
    {
        if (_activeNetworkManager is true)
        {
            return;
        }
        Instantiate(kcpNetworkManager, new Vector3(0,0,0), Quaternion.identity);
        _activeNetworkManager = true;
        
        GameObject.Find("Buttons").SetActive(false);
    }

    private void InstantiateSteamNetworkManager()
    {
        if (_activeNetworkManager is true)
        {
            return;
        }
        Instantiate(steamNetworkManager, new Vector3(0, 0, 0), Quaternion.identity);
        _activeNetworkManager = true;
        
        GameObject.Find("Buttons").SetActive(false);
    }

    private void InstantiateOfflineMode()
    {
        if (_activeNetworkManager is true)
        {
            return;
        }
        SceneManager.LoadScene("Level1");
        _activeNetworkManager = true;
        
        GameObject.Find("Buttons").SetActive(false);
    }
}
