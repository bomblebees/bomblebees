﻿using System;
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
    [SerializeField] private GameObject screenBombList;
    [Header("Opacity Configuration")]
    [Range(0f, 1f)]
    [SerializeField] private float deactivatedOpacity;
    [Header("Start Button")]
    [SerializeField] public GameObject buttonStart;
    public Button _buttonStartButton;
    private ButtonHoverTween _buttonStartButtonHoverTween;
    private CanvasRenderer[] _buttonStartCanvasRenderer;
    [Header("Ready Button")]
    [SerializeField] public GameObject buttonReady;
    private Button _buttonReadyButton;
    private ButtonHoverTween _buttonReadyButtonHoverTween;
    private CanvasRenderer[] _buttonReadyCanvasRenderer;

    [Serializable]
    public class PlayerLobbyCard
    {
        public GameObject playerCard;
        public RawImage avatar;
        public TMP_Text username;
        public GameObject readyStatus;
        public RawImage characterPortrait;
        public Button changeCharacterButton;
        public ButtonHoverTween changeCharacterButtonHoverTween;
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

        if (matchmaker)
        {
            lobbyName.text = matchmaker.GetLobbyName();
            matchmaker.OnMirrorSetStatus("In Lobby");
        }

        // Cache start button components
        _buttonStartButton = buttonStart.GetComponent<Button>();
        _buttonStartButtonHoverTween = buttonStart.GetComponent<ButtonHoverTween>();
        _buttonStartCanvasRenderer = buttonStart.GetComponentsInChildren<CanvasRenderer>();
        
        // Cache ready button components
        _buttonReadyButton = buttonReady.GetComponent<Button>();
        _buttonReadyButtonHoverTween = buttonReady.GetComponent<ButtonHoverTween>();
        _buttonReadyCanvasRenderer = buttonReady.GetComponentsInChildren<CanvasRenderer>();
        
        // Initialize button states
        DeactivateStartButton();
        ActivateReadyButton();
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

            // For some reason network manager is moved out of don't destroy, this is to put it back
            DontDestroyOnLoad(networkManager.gameObject);

            mainMenuUI.gameObject.SetActive(true);
        }
    }

    public void OnStartButtonClick()
    {
        EventStartButtonClicked?.Invoke();
        matchmaker.OnMirrorSetStatus("In Game");
    }

    public void OnReadyButtonClick()
    {
        EventReadyButtonClicked?.Invoke();
    }

    public void ActivateStartButton()
    {
        // Update functionality
        _buttonStartButton.enabled = true;
        
        // Update appearance
        foreach (CanvasRenderer t in _buttonStartCanvasRenderer)
        {
            t.SetAlpha(1f);
        }
        buttonStart.transform.localScale.Set(1f,1f,1f);
        _buttonStartButtonHoverTween.enabled = true;
    }

    public void DeactivateStartButton()
    {
        // Update functionality
        _buttonStartButton.enabled = false;
        
        // Update appearance
        foreach (CanvasRenderer t in _buttonStartCanvasRenderer)
        {
            t.SetAlpha(deactivatedOpacity);
        }
        
        _buttonStartButtonHoverTween.enabled = false;
        buttonStart.transform.localScale.Set(1f,1f,1f);
    }
    
    public void ActivateReadyButton()
    {
        // Update functionality
        _buttonReadyButton.enabled = true;
        
        // Update appearance
        foreach (CanvasRenderer t in _buttonReadyCanvasRenderer)
        {
            t.SetAlpha(1f);
        }
        buttonReady.transform.localScale.Set(1f,1f,1f);
        _buttonReadyButtonHoverTween.enabled = true;
    }

    public void DeactivateReadyButton()
    {
        // Update functionality
        _buttonReadyButton.enabled = false;
        
        // Update appearance
        foreach (CanvasRenderer t in _buttonReadyCanvasRenderer)
        {
            t.SetAlpha(deactivatedOpacity);
        }
        _buttonReadyButtonHoverTween.enabled = false;
        buttonReady.transform.localScale.Set(1f,1f,1f);
    }
    
    #region Screen: HOW TO PLAY

    public void ToggleScreenHowToPlay()
    {
        screenHowToPlay.SetActive(!screenHowToPlay.activeSelf);
    }

    #endregion

    #region Screen: BOMB LIST

    public void ToggleScreenBombList()
    {
        screenBombList.SetActive(!screenBombList.activeSelf);
    }
    
    #endregion
}