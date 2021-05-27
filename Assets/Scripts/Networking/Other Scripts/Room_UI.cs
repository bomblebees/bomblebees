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
    [SerializeField] private GameObject screenLeavePopup;

    [Header("Opacity Configuration")]
    [Range(0f, 1f)]
    [SerializeField] private float deactivatedOpacity = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float activatedOpacity = 1f;
    [Header("Start Button")]
    [SerializeField] public GameObject buttonStart;

    private Button _buttonStartButton;
    private ButtonHoverTween _buttonStartButtonHoverTween;
    private CanvasRenderer[] _buttonStartCanvasRenderer;
    [Header("Ready Button")]
    [SerializeField] public GameObject buttonReady;
    private Button _buttonReadyButton;
    private ButtonHoverTween _buttonReadyButtonHoverTween;
    private CanvasRenderer[] _buttonReadyCanvasRenderer;

    [Header("Helper Text")] 
    [SerializeField] private TMP_Text readyHelperText;
    [SerializeField] private TMP_Text startHelperText;
    [SerializeField] private TMP_Text gameModesHelperText;
    private CanvasRenderer _readyHelperTextCanvasRenderer;
    private CanvasRenderer _startHelperTextCanvasRenderer;
    // private CanvasRenderer gameModesHelperTextCanvasRenderer;

    //[SerializeField] private PlayerLobbyCard playerLobbyCardPrefab = null;
    [SerializeField] public GameObject playerCardsParent;
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

        // Cache start button components
        _buttonStartButton = buttonStart.GetComponent<Button>();
        _buttonStartButtonHoverTween = buttonStart.GetComponent<ButtonHoverTween>();
        _buttonStartCanvasRenderer = buttonStart.GetComponentsInChildren<CanvasRenderer>();

        // Cache ready button components
        _buttonReadyButton = buttonReady.GetComponent<Button>();
        _buttonReadyButtonHoverTween = buttonReady.GetComponent<ButtonHoverTween>();
        _buttonReadyCanvasRenderer = buttonReady.GetComponentsInChildren<CanvasRenderer>();
        
        // Cache button helper texts
        _readyHelperTextCanvasRenderer = readyHelperText.gameObject.GetComponent<CanvasRenderer>();
        _startHelperTextCanvasRenderer = startHelperText.gameObject.GetComponent<CanvasRenderer>();
        // gameModesHelperTextCanvasRenderer = gameModesHelperText.gameObject.GetComponent<CanvasRenderer>();

        // Initialize button states
        DeactivateStartButton();
        ActivateReadyButton();

        // Initialize player cards
        InitializePlayerCards();
    }

    private void InitializePlayerCards()
    {
        //CharacterSelectionInfo charSelect = FindObjectOfType<CharacterSelectionInfo>();

        //float gapBetweenCards = 450;
        //float startPosX = -675;
        //float posY = -300;

        //for (int i = 0; i < playerCardsList.Length; i++)
        //{
        //    // instantiate the player card
        //    PlayerLobbyCard card = Instantiate(
        //        playerLobbyCardPrefab,
        //        new Vector3(0, 0, 0),
        //        Quaternion.identity,
        //        playerCardsParent.transform);

        //    // set the position on the UI
        //    card.transform.localPosition = new Vector3(startPosX + (gapBetweenCards * i), posY);

        //    card.changeCharacterButton.onClick.AddListener(charSelect.OnChangeCharacter);

        //    // add it to the list of player cards
        //    playerCardsList[i] = card;
        //}
    }

    public void ExitLobby()
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
        }
    }

    public void OnLeaveLobbyClick()
    {
        screenLeavePopup.SetActive(true);
    }

    public void OnStartButtonClick()
    {
        EventStartButtonClicked?.Invoke();

        if (!matchmaker) matchmaker = Matchmaking.singleton;
        if (matchmaker) matchmaker.OnMirrorSetStatus("In Game");
    }

    public void OnReadyButtonClick()
    {
        EventReadyButtonClicked?.Invoke();
    }

    public void ActivateStartButton()
    {
        // Update functionality
        _buttonStartButton.interactable = true;
        
        // Update appearance
        foreach (CanvasRenderer t in _buttonStartCanvasRenderer)
        {
            t.SetAlpha(activatedOpacity);
        }
        buttonStart.transform.localScale.Set(1f,1f,1f);
        _buttonStartButtonHoverTween.enabled = true;
        
        _startHelperTextCanvasRenderer.SetAlpha(float.Epsilon);
    }

    public void DeactivateStartButton()
    {
        // Update functionality
        _buttonStartButton.interactable = false;
        
        // Update appearance
        foreach (CanvasRenderer t in _buttonStartCanvasRenderer)
        {
            t.SetAlpha(deactivatedOpacity);
        }
        
        _buttonStartButtonHoverTween.enabled = false;
        buttonStart.transform.localScale.Set(1f,1f,1f);
        
        _startHelperTextCanvasRenderer.SetAlpha(1f);
    }
    
    public void ActivateReadyButton()
    {
        // Update functionality
        _buttonReadyButton.interactable = true;
        
        // Update appearance
        foreach (CanvasRenderer t in _buttonReadyCanvasRenderer)
        {
            t.SetAlpha(activatedOpacity);
        }
        buttonReady.transform.localScale.Set(1f,1f,1f);
        _buttonReadyButtonHoverTween.enabled = true;
        
        _readyHelperTextCanvasRenderer.SetAlpha(float.Epsilon);
    }

    public void DeactivateReadyButton()
    {
        // Update functionality
        _buttonReadyButton.interactable = false;
        
        // Update appearance
        foreach (CanvasRenderer t in _buttonReadyCanvasRenderer)
        {
            t.SetAlpha(deactivatedOpacity);
        }
        _buttonReadyButtonHoverTween.enabled = false;
        buttonReady.transform.localScale.Set(1f,1f,1f);
        
        _readyHelperTextCanvasRenderer.SetAlpha(1f);
    }

    public void SetReadyHelperText(string helperText)
    {
        readyHelperText.text = helperText;
    }
    
    public void SetStartHelperText(string helperText)
    {
        startHelperText.text = helperText;
    }
    
    public void SetGameModeHelperText(string helperText)
    {
        gameModesHelperText.text = helperText;
    }

    public void OnSettingsButtonClick()
    {
        FindObjectOfType<LobbySettings>().OnClickOpenSettings();
    }

    #region Screen: HOW TO PLAY

    public void ToggleScreenHowToPlay()
    {
        screenHowToPlay.SetActive(!screenHowToPlay.activeSelf);
    }

    #endregion

    #region Screen: BOMB LIST

    public void ToggleScreenControls()
    {
        screenControls.SetActive(!screenControls.activeSelf);
    }
    
    #endregion
}