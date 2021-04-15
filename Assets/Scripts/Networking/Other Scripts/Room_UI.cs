using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Room_UI : MonoBehaviour
{

    private NetworkManager networkManager;
    private MainMenu_UI mainMenuUI;
    private Matchmaking matchmaker;

    [Header("Screen")]
    [SerializeField] private GameObject screenHowToPlay;
    [SerializeField] private GameObject screenBombleList;
    [Header("Start Button")]
    [SerializeField] public GameObject buttonStart;
    [SerializeField] private Image buttonStartImage;
    [SerializeField] private Button buttonStartButton;
    [SerializeField] private ButtonHoverTween buttonStartButtonHoverTween;
    
    [Header("Button Sprite")]
    [SerializeField] private Sprite buttonActivated;
    [SerializeField] private Sprite buttonDeactivated;
    
    [Serializable]
    public class PlayerLobbyCard
    {
        public GameObject playerCard;
        public RawImage avatar;
        public TMP_Text username;
        public GameObject readyStatus;
    }

    [SerializeField] public PlayerLobbyCard[] playerLobbyUi = new PlayerLobbyCard[4];
    [SerializeField] public TMP_Text lobbyName;

    // events
    public delegate void ReadyClickDelegate();
    public delegate void StartClickDelegate();
    public event ReadyClickDelegate EventReadyButtonClicked;
    public event StartClickDelegate EventStartButtonClicked;

    public static Room_UI singleton;
    private void Awake()
    {
        singleton = this;
    }

    public void Start()
    {
        networkManager = NetworkManager.singleton;
        mainMenuUI = MainMenu_UI.singleton;
        matchmaker = Matchmaking.singleton;

        if (mainMenuUI.screenLoading.activeSelf)
        {
            mainMenuUI.screenLoading.SetActive(false);
            
        }

        if (!mainMenuUI.screenNavigation.activeSelf)
        {
            mainMenuUI.screenNavigation.SetActive(true);
        }

        mainMenuUI.gameObject.SetActive(false);

        if (matchmaker) lobbyName.text = matchmaker.GetLobbyName();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {
        if (matchmaker)
        {
            matchmaker.MirrorLeaveLobby();
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

    public void OnStartButtonClick()
    {
        EventStartButtonClicked?.Invoke();
    }

    public void OnReadyButtonClick()
    {
        EventReadyButtonClicked?.Invoke();
    }

    public void ActivateStartButton()
    {
        buttonStartImage.sprite = buttonActivated;
        buttonStartButtonHoverTween.enabled = true;
        buttonStartButton.enabled = true;
    }

    public void DeactivateStartButton()
    {
        buttonStartImage.sprite = buttonDeactivated;
        buttonStartButtonHoverTween.enabled = false;
        buttonStartButton.enabled = false;
    }
    
    #region Screen: HOW TO PLAY

    public void ToggleScreenHowToPlay()
    {
        screenHowToPlay.SetActive(!screenHowToPlay.activeSelf);
    }

    #endregion

    #region Screen: BOMBLE LIST

    public void ToggleScreenBombleList()
    {
        screenBombleList.SetActive(!screenBombleList.activeSelf);
    }
    
    #endregion
}