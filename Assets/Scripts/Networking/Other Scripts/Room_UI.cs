using System;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Room_UI : MonoBehaviour
{

    private NetworkManager networkManager;
    private MainMenu_UI mainMenuUI;
    private Matchmaking matchmaker;

    [Header("Screen")]
    [SerializeField] private GameObject screenHowToPlay;
    [SerializeField] private GameObject screenControls;
    [Header("Start Button")] 
    private GlobalButtonSettings _globalButtonSettings;
    [SerializeField] public GameObject buttonStart;
    public Button buttonStartButton;
    private ButtonHoverTween _buttonStartButtonHoverTween;
    private List<CanvasRenderer> _buttonStartCanvasRenderers;
    [Header("Ready Button")]
    [SerializeField] public GameObject buttonReady;
    private Button _buttonReadyButton;
    private ButtonHoverTween _buttonReadyButtonHoverTween;
    private List<CanvasRenderer> _buttonReadyCanvasRenderers;

    [SerializeField] private PlayerLobbyCard playerLobbyCardPrefab = null;
    [SerializeField] private GameObject playerCardsParent;
    [HideInInspector] public PlayerLobbyCard[] playerCardsList = new PlayerLobbyCard[4];

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

        _globalButtonSettings = FindObjectOfType<GlobalButtonSettings>();
        
        // Cache start button components
        buttonStartButton = buttonStart.GetComponent<Button>();
        _buttonStartButtonHoverTween = buttonStart.GetComponent<ButtonHoverTween>();
        _buttonStartCanvasRenderers = 
            new List<CanvasRenderer>(buttonStart.GetComponentsInChildren<CanvasRenderer>());

        // Cache ready button components
        _buttonReadyButton = buttonReady.GetComponent<Button>();
        _buttonReadyButtonHoverTween = buttonReady.GetComponent<ButtonHoverTween>();
        _buttonReadyCanvasRenderers =
            new List<CanvasRenderer>(buttonReady.GetComponentsInChildren<CanvasRenderer>());

        // Initialize button states
        DeactivateStartButton();
        ActivateReadyButton();

        // Initialize player cards
        InitializePlayerCards();
    }

    private void InitializePlayerCards()
    {
        CharacterSelectionInfo charSelect = FindObjectOfType<CharacterSelectionInfo>();

        float gapBetweenCards = 450;
        float startPosX = -675;
        float posY = -300;

        for (int i = 0; i < playerCardsList.Length; i++)
        {
            // instantiate the player card
            PlayerLobbyCard card = Instantiate(
                playerLobbyCardPrefab,
                new Vector3(0, 0, 0),
                Quaternion.identity,
                playerCardsParent.transform);

            // set the position on the UI
            card.transform.localPosition = new Vector3(startPosX + (gapBetweenCards * i), posY);

            card.changeCharacterButton.onClick.AddListener(charSelect.OnChangeCharacter);

            // add it to the list of player cards
            playerCardsList[i] = card;
        }
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

        if (!matchmaker) matchmaker = Matchmaking.singleton;
        matchmaker.OnMirrorSetStatus("In Game");
    }

    public void OnReadyButtonClick()
    {
        EventReadyButtonClicked?.Invoke();
    }

    public void ActivateStartButton()
    {
        // Update functionality
        buttonStartButton.interactable = true;
        
        // Update appearance
        _globalButtonSettings.ActivateButton(_buttonStartCanvasRenderers);
        buttonStart.transform.localScale.Set(1f,1f,1f);
        _buttonStartButtonHoverTween.enabled = true;
    }

    public void DeactivateStartButton()
    {
        // Update functionality
        buttonStartButton.interactable = false;
        
        // Update appearance
        _globalButtonSettings.DeactivateButton(_buttonStartCanvasRenderers);
        _buttonStartButtonHoverTween.enabled = false;
        buttonStart.transform.localScale.Set(1f,1f,1f);
    }
    
    public void ActivateReadyButton()
    {
        // Update functionality
        _buttonReadyButton.interactable = true;
        
        // Update appearance
        _globalButtonSettings.ActivateButton(_buttonReadyCanvasRenderers);
        buttonReady.transform.localScale.Set(1f,1f,1f);
        _buttonReadyButtonHoverTween.enabled = true;
    }

    public void DeactivateReadyButton()
    {
        // Update functionality
        _buttonReadyButton.interactable = false;
        
        // Update appearance
        _globalButtonSettings.DeactivateButton(_buttonReadyCanvasRenderers);
        _buttonReadyButtonHoverTween.enabled = false;
        buttonReady.transform.localScale.Set(1f,1f,1f);
    }

    public void ToggleScreenHowToPlay()
    {
        screenHowToPlay.SetActive(!screenHowToPlay.activeSelf);
    }

    public void ToggleScreenControls()
    {
        screenControls.SetActive(!screenControls.activeSelf);
    }
}