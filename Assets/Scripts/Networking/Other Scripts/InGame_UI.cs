using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class InGame_UI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] [Scene] private string RoomScene;
    [SerializeField] private GameObject screenOptions;
    
    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ToggleScreenOptions();
        }
    }

    #region Screen: Options

    public void ToggleScreenOptions()
    {
        screenOptions.SetActive(!screenOptions.activeSelf);
    }
    
    public void ReturnToRoom()
    {
        networkManager.ServerChangeScene(RoomScene);
    }

    public void Quit()
    {
        Application.Quit();
    }

    #endregion

}
