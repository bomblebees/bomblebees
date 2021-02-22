using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Room_UI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private MainMenu_UI mainMenuUI;

    public void Start()
    {
        networkManager = NetworkManager.singleton;
        mainMenuUI = MainMenu_UI.singleton;
        
        mainMenuUI.gameObject.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {


        Matchmaking matchmaker = Matchmaking.singleton;
        if (matchmaker)
        {
            matchmaker.uiLeaveLobby();
        }
        else
        {
            if (NetworkServer.active) networkManager.StopHost();
            else networkManager.StopClient();

            // For some reason netowrk manager is moved out of dontdestroy, this is to put it back
            DontDestroyOnLoad(networkManager.gameObject);

            mainMenuUI.gameObject.SetActive(true);
        }
    }
}