using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Room_UI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private MainMenu_UI mainMenuUI;

    public void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        mainMenuUI = FindObjectOfType<MainMenu_UI>();
        
        mainMenuUI.gameObject.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            networkManager.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            networkManager.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            networkManager.StopServer();
        }
        
        SceneManager.LoadScene("MainMenu");
        mainMenuUI.Activate();
    }
}