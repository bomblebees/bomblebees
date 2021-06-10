using Mirror;
using TMPro;
using UnityEngine;

public class Room_UI : MonoBehaviour
{
    private NetworkManager _networkManager;
    private MainMenu_UI _mainMenuUI;
    private Matchmaking _matchmaker;

    [Header("Screen")] [SerializeField] private GameObject screenLeavePopup;

    [Header("Buttons")] [SerializeField] public GameObject buttonStart;
    [SerializeField] public GameObject buttonReady;
    [SerializeField] public GameObject buttonLeave;
    [SerializeField] public GameObject buttonSettings;

    [Header("Helper Text")] [SerializeField]
    private TMP_Text readyHelperText;

    [SerializeField] private TMP_Text startHelperText;
    [SerializeField] private TMP_Text gameModeHelperText;
    private CanvasRenderer _readyHelperTextCanvasRenderer;
    private CanvasRenderer _startHelperTextCanvasRenderer;
    public CanvasRenderer gameModeHelperTextCanvasRenderer;

    [Header("Misc.")] [SerializeField] public GameObject playerCardsParent;
    [SerializeField] public TMP_Text lobbyName;

    // events
    public delegate void ReadyClickDelegate();

    public delegate void StartClickDelegate();

    public event ReadyClickDelegate EventReadyButtonClicked;
    public event StartClickDelegate EventStartButtonClicked;

    public static Room_UI Singleton;

    private void Awake()
    {
        Singleton = this;

        // Helper texts
        _readyHelperTextCanvasRenderer = readyHelperText.gameObject.GetComponent<CanvasRenderer>();
        _startHelperTextCanvasRenderer = startHelperText.gameObject.GetComponent<CanvasRenderer>();
        gameModeHelperTextCanvasRenderer = gameModeHelperText.gameObject.GetComponent<CanvasRenderer>();
    }

    public void Start()
    {
        _networkManager = NetworkManager.singleton;
        _mainMenuUI = MainMenu_UI.Singleton;
        _matchmaker = Matchmaking.Singleton;

        if (_mainMenuUI.screenLoading.activeSelf)
        {
            _mainMenuUI.screenLoading.SetActive(false);
        }

        if (!_mainMenuUI.screenNavigation.activeSelf)
        {
            _mainMenuUI.screenNavigation.SetActive(true);
        }

        _mainMenuUI.gameObject.SetActive(false);

        if (_matchmaker)
        {
            lobbyName.text = _matchmaker.GetLobbyName();
            _matchmaker.OnMirrorSetStatus("In Lobby");
        }

        // Initialize button states
        DeactivateStartButton();
        ActivateReadyButton();
    }

    public void ExitLobby()
    {
        if (_matchmaker)
        {
            _matchmaker.MirrorLeaveLobby();
        }
        else
        {
            if (NetworkServer.active) _networkManager.StopHost();
            else _networkManager.StopClient();

            // For some reason network manager is moved out of don't destroy, this is to put it back
            DontDestroyOnLoad(_networkManager.gameObject);
        }
    }

    public void OnLeaveLobbyClick()
    {
        screenLeavePopup.SetActive(true);
    }

    public void OnStartButtonClick()
    {
        EventStartButtonClicked?.Invoke();

        if (!_matchmaker) _matchmaker = Matchmaking.Singleton;
        if (_matchmaker) _matchmaker.OnMirrorSetStatus("In Game");
    }

    public void OnReadyButtonClick()
    {
        EventReadyButtonClicked?.Invoke();
    }

    public void ActivateStartButton()
    {
        buttonStart.GetComponent<ButtonDisable>().EnableButton();

        _startHelperTextCanvasRenderer.SetAlpha(float.Epsilon);
    }

    public void DeactivateStartButton()
    {
        buttonStart.GetComponent<ButtonDisable>().DisableButton();

        _startHelperTextCanvasRenderer.SetAlpha(1f);
    }

    public void ActivateReadyButton()
    {
        buttonReady.GetComponent<ButtonDisable>().EnableButton();

        _readyHelperTextCanvasRenderer.SetAlpha(float.Epsilon);
    }

    public void DeactivateReadyButton()
    {
        buttonReady.GetComponent<ButtonDisable>().DisableButton();

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
        gameModeHelperText.text = helperText;
    }

    public void OnSettingsButtonClick()
    {
        FindObjectOfType<LobbySettings>().OnClickOpenSettings();
    }
}