using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    [Header("SyncVars")]
    [SyncVar] public ulong steamId;
    [SyncVar] public string steamUsername = "Username";
    [SyncVar] public int steamAvatarId;
    [SyncVar] public Color playerColor;

    /// <summary>The ping of the player</summary>
    [SyncVar(hook=nameof(PingChanged))] public string ping;

    /// <summary>The selected character/color</summary>
    [SyncVar(hook = nameof(CharacterChanged))] public int characterCode;

    /// <summary>The selected team</summary>
    [SyncVar(hook = nameof(TeamChanged))] public int teamIndex;

    [Header("Required")]
    [SerializeField] private PlayerLobbyCard playerLobbyCardPrefab;
    private PingDisplay _pingDisplay;
    private NetworkManager _networkManager;
    private CharacterSelectionInfo _characterSelectionInfo;
    private LobbySettings _lobbySettings;
    private MenuAudioManager _audioManager;

    /// <summary>
    /// The player lobby card UI of this player. A value of null means it is uninitialized.
    /// </summary>
    private PlayerLobbyCard _playerCard;

    // Temp list of player colors
    private readonly List<Color> _listColors = new List<Color> {
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green,
    };

    private Room_UI _roomUI;
    private NetworkRoomManagerExt _room;

    private void Awake()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        _pingDisplay = FindObjectOfType<PingDisplay>();
        _lobbySettings = FindObjectOfType<LobbySettings>();
        _audioManager = FindObjectOfType<MenuAudioManager>();
    }

    #region Joining/Leaving Lobby

    /// <summary>
    /// Called when the client first connects to the lobby.
    /// </summary>
    [Client] public override void OnStartClient()
    {
        SetupLobby();

        // Start updating ping
        if (isLocalPlayer)
        {
            SetPing();
            InvokeRepeating(nameof(WaitForFirstPing), float.Epsilon, 0.1f);
        }
    }


    /// <summary>
    /// Called when the client disconnects from the lobby
    /// </summary>
    [Client] public override void OnStopClient()
    {
        _characterSelectionInfo.ResetAll();
        DestroyPlayerCard();
    }

    /// <summary>
    /// Called when the client enters the lobby scene.
    /// <para>We should only use this for when the client re-enters the lobby from the game scene.
    /// For when the client first joins the lobby, use OnStartClient instead</para>
    /// </summary>
    [Client] public override void OnClientEnterRoom()
    {
        SetupLobby();
    }

    ///// <summary>
    ///// Called when the client leaves the lobby scene.
    ///// </summary>
    //[Client] public override void OnClientExitRoom()
    //{
    //    isSetup = false;
    //}

    #endregion

    #region Update Loop

    private bool gameStarted = false;

    private void Update()
    {
        // Exit if required vars are not initialized
        if (!_characterSelectionInfo || !_room || !_roomUI) return;

        UpdateLobby();

        // If practice mode, immediately start game
        if (isServer && !gameStarted && _lobbySettings.practiceMode)
        {
            Debug.Log("entering practice mode");
            gameStarted = true;
            _lobbySettings.roundDuration = 0f; // set inf time
            _lobbySettings.playerLives = 0; // set inf lives
            FindObjectOfType<PingDisplay>().PracticeStatus(); // set ping status
            _room.ServerChangeScene(_room.GameplayScene); // change 
        }
    }

    /// <summary>
    /// Updates certain elements of the lobby every frame.
    /// <para>Only updates that cannot be updated using SyncVar hooks should be implemented here.</para>
    /// </summary>
    [Client] private void UpdateLobby()
    {
        // For local lobby buttons
        if (isLocalPlayer)
        {
            // Enable/disable the start game button (for host only)
            if (_room.allPlayersReady)
            {
                if (_room.showStartButton)
                {
                    // Host: all players ready
                    _roomUI.ActivateStartButton();
                    _roomUI.SetStartHelperText(null);
                }
                else
                {
                    _roomUI.DeactivateStartButton();
                }
            }
            else
            {
                if (_networkManager.networkAddress.Equals("localhost"))
                {
                    // Host: not all players ready
                    _roomUI.DeactivateStartButton();
                    _roomUI.SetStartHelperText($"ready ({_room.readyPlayers}/{_room.currentPlayers})");
                }
                else
                {
                    if (_room.readyPlayers.Equals(_room.currentPlayers))
                    {
                        // Client: all players ready
                        _roomUI.DeactivateStartButton();
                        _roomUI.SetStartHelperText("host only");
                    }
                    else
                    {
                        // Client: not all players ready
                        _roomUI.DeactivateStartButton();
                        _roomUI.SetStartHelperText($"ready ({_room.readyPlayers}/{_room.currentPlayers})");
                    }
                }
            }
            
            // Enable/disable the ready button
            if (!readyToBegin && !_characterSelectionInfo.characterAvailable[characterCode])
            {
                _roomUI.DeactivateReadyButton();
                _roomUI.SetReadyHelperText("choose an unselected character!");
            }
            else
            {
                _roomUI.ActivateReadyButton();
                _roomUI.SetReadyHelperText("");
            }
        }

        // If not ready, and character portrait is unavailable, grey out the portrait
        if (!readyToBegin && !_characterSelectionInfo.characterAvailable[characterCode])
        {
            _playerCard.characterPortrait.color = new Color(0.4f, 0.4f, 0.4f);
            if (isLocalPlayer) _playerCard.disabledText.gameObject.SetActive(true);
        }
        else
        {
            _playerCard.characterPortrait.color = new Color(1f, 1f, 1f);
            if (isLocalPlayer) _playerCard.disabledText.gameObject.SetActive(false);
        }

        if (_playerCard)
        {
            // If using teams gamemode, enable the button
            _playerCard.changeTeamButton.gameObject.SetActive(_lobbySettings.GetGamemode() is TeamsGamemode);
        }
    }

    #endregion

    #region Lobby Creation

    /// <summary>
    /// Sets up the lobby for this player
    /// </summary>
    [Client] private void SetupLobby()
    {
        // Exit if lobby is already setup
        if (!_playerCard)
        {
            InitRequiredVars();
            InitLobbyButtons();
            CreatePlayerCard();
            InitPlayerCard();
        }
        
        _room.ReadyStatusChanged();
        _lobbySettings.SetGamemodeText();
    }
    
    /// <summary>
    /// Initializes lobby room variables
    /// </summary>
    [Client] private void InitRequiredVars()
    {
        // Only init if we are in the room scene
        if (SceneManager.GetActiveScene().name != "Room") return;

        if (!_characterSelectionInfo)
        {
            _characterSelectionInfo = FindObjectOfType<CharacterSelectionInfo>();
            if (!_characterSelectionInfo) Debug.LogError("_characterSelectionInfo not found");
        }

        if (!_room)
        {
            _room = NetworkManager.singleton as SteamNetworkManager;
            if (!_room) _room = NetworkManager.singleton as NetworkRoomManagerExt;
            if (!_room) Debug.LogError("room not found");
        }

        if (!_roomUI)
        {
            _roomUI = Room_UI.Singleton;
            if (!_roomUI) Debug.LogError("room not found");
        }
    }

    /// <summary>
    /// Initializes lobby room buttons
    /// </summary>
    [Client] private void InitLobbyButtons()
    {
        // Exit if we are not the local player
        if (!isLocalPlayer) return;

        _roomUI.EventReadyButtonClicked += OnReadyButtonClick;
        _roomUI.EventStartButtonClicked += OnStartButtonClick;
    }

    /// <summary>
    /// Creates and instantiates the player card game object
    /// </summary>
    [Client] private void CreatePlayerCard()
    {
        // if the card is already created, do not create again
        if (_playerCard) return;

        // instantiate the player card
        _playerCard = Instantiate(
            playerLobbyCardPrefab,
            new Vector3(0, 0, 0),
            Quaternion.identity,
            _roomUI.playerCardsParent.transform);


        // add button event listeners for the local player
        if (isLocalPlayer)
        {
            _playerCard.changeCharacterButton.onClick.AddListener(OnCharacterClicked);
            _playerCard.changeTeamButton.onClick.AddListener(OnTeamClicked);
        }
        
        // set the transform of the card
        SetCardPosition();
    }

    /// <summary>
    /// Initializes the player card that was created
    /// </summary>
    [Client] private void InitPlayerCard()
    {
        // enable the character portrait
        _playerCard.characterPortrait.enabled = true;

        // set the player name
        _playerCard.username.text = $"{steamUsername} ({ping})";

        // set initial card variables
        SetCardPosition();
        SetCardReadyStatus();
        SetCardCharacterPortrait();
        SetCardTeamButton();
        SetCardButtons();
        SetCardHostCrown();
    }

    #endregion

    #region Lobby Destruction

    /// <summary>
    /// Destroys the lobby player card of this player
    /// </summary>
    [Client] private void DestroyPlayerCard()
    {
        if (_playerCard)
        {
            Destroy(_playerCard.gameObject);
        }
    }

    #endregion

    #region Player Card

    // Static positions used in SetCardPosition()
    private static float distBetweenCards = 450;
    private static float cardStartPosX = -675;
    private static float cardPosY = -300;

    /// <summary>
    /// Sets the position of this player's lobby player to a position based on the player's index in the lobby.
    /// </summary>
    [Client] private void SetCardPosition()
    {
        if (!_playerCard)
        {
            Debug.LogWarning("Player Card not initialized in SetCardPosition()!"); 
            return;
        }

        _playerCard.transform.localPosition = new Vector3(cardStartPosX + (distBetweenCards * index), cardPosY);
    }

    /// <summary>
    /// Sets the ready status of this player based on their current ready status
    /// </summary>
    [Client] private void SetCardReadyStatus()
    {
        if (!_playerCard)
        {
            Debug.LogWarning("Player Card not initialized in SetCardReadyStatus()!"); 
            return;
        }

        // Enable/Disable the ready status UI
        _playerCard.readyStatus.SetActive(readyToBegin);

        // Lock character code if player is readied
        _characterSelectionInfo.characterAvailable[characterCode] = !readyToBegin;
    }

    /// <summary>
    /// Sets the character portrait of this player based on their current selected character code
    /// </summary>
    [Client] private void SetCardCharacterPortrait()
    {
        if (!_playerCard)
        {
            Debug.LogWarning("Player Card not initialized in SetCardCharacterPortrait()!"); 
            return;
        }

        // Set the character portrait
        _playerCard.characterPortrait.texture = _characterSelectionInfo.characterPortraitList[characterCode];

        // Set colors of the color frames
        foreach (var image in _playerCard.colorFrames)
        {
            image.color = _listColors[characterCode];
        }
    }

    /// <summary>
    /// Sets the team button text on the player card
    /// </summary>
    [Client]
    private void SetCardTeamButton()
    {
        if (!_playerCard)
        {
            Debug.LogWarning("Player Card not initialized in SetCardTeamButton()!"); 
            return;
        }

        // Set the character portrait
        _playerCard.changeTeamButton.GetComponentInChildren<TMP_Text>().text = $"Team {teamIndex + 1}";
    }

    /// <summary>
    /// Sets the card buttons for this player.
    /// <para>Card specific buttons should only be interactable by the local player.</para>
    /// </summary>
    [Client] private void SetCardButtons()
    {
        if (!_playerCard)
        {
            Debug.LogWarning("Player Card not initialized in SetCardButtons()!"); 
            return;
        }

        if (isLocalPlayer)
        {
            // Allow local player to change the portrait
            _playerCard.changeCharacterButton.enabled = !readyToBegin;
            _playerCard.changeCharacterButtonHoverTween.enabled = !readyToBegin;

            _roomUI.buttonReady.GetComponentInChildren<TMP_Text>().text = readyToBegin ? "Unready" : "Ready";
        }
        else
        {
            _playerCard.changeCharacterButton.enabled = false;
            _playerCard.changeCharacterButtonHoverTween.enabled = false;
        }
    }

    [Client] private void SetCardHostCrown()
    {
        if (!_playerCard)
        {
            Debug.LogWarning("Player Card not initialized in SetCardHostCrown()!"); 
            return;
        }

        // if this is the host, enable the crown icon
        _playerCard.crown.SetActive(index.Equals(0));
    }

    #endregion

    #region Ping

    private void WaitForFirstPing()
    {
        if (!_pingDisplay.isConnected) return;

        CancelInvoke(nameof(WaitForFirstPing));
        InvokeRepeating(nameof(SetPing), float.Epsilon, _pingDisplay.updateInterval);
    }

    [Client] private void SetPing()
    {
        CmdSetPing(_pingDisplay.myPingDisplay);
        UpdatePingDisplay();
    }

    [Command] private void CmdSetPing(string newPing)
    {
        ping = newPing;
    }

    [ClientCallback] private void PingChanged(string prevPing, string newPing)
    {
        UpdatePingDisplay();
    }

    [Client] private void UpdatePingDisplay()
    {
        if (_playerCard.Equals(null)) return;

        _playerCard.username.text = $"{steamUsername} ({ping})";
    }

    #endregion

    #region SyncVar Hooks

    /// <summary>
    /// SyncVar hook for the index of the lobby player
    /// </summary>
    [ClientCallback] public override void IndexChanged(int prevIndex, int newIndex)
    {
        // Prevent updating card before it is created (this function is fired on join)
        if (_playerCard.Equals(null)) return;

        // Propagate updates to the player card
        SetCardPosition();
        SetCardHostCrown();
    }

    /// <summary>
    /// SyncVar hook for the ready status of the lobby player
    /// </summary>
    [ClientCallback] public override void ReadyStateChanged(bool prevReadyState, bool newReadyState)
    {
        // Propagate updates to the player card
        SetCardReadyStatus();
        SetCardButtons();
        
        _room.ReadyStatusChanged();

        // Disable buttons when readied
        if (newReadyState)
        {
            _roomUI.buttonLeave.GetComponent<ButtonDisable>().DisableButton();
            _roomUI.buttonSettings.GetComponent<ButtonDisable>().DisableButton();
        }
        else
        {
            // if (_roomUI.buttonLeave)
                _roomUI.buttonLeave.GetComponent<ButtonDisable>().EnableButton();
            // if (_roomUI.buttonSettings)
                _roomUI.buttonSettings.GetComponent<ButtonDisable>().EnableButton();
        }
    }

    /// <summary>
    /// SyncVar hook for the variable characterCode
    /// </summary>
    [ClientCallback] private void CharacterChanged(int prevCharCode, int newCharCode)
    {
        // Propagate updates to the player card
        SetCardCharacterPortrait();
    }

    /// <summary>
    /// SyncVar hook for the variable teamIndex
    /// </summary>
    [ClientCallback] private void TeamChanged(int prevTeamIdx, int newTeamIdx)
    {
        // Propagate updates to the player card
        SetCardTeamButton();
    }

    #endregion

    #region Button Click Events

    // Called by the local host player when the start button is clicked
    [Client] private void OnStartButtonClick()
    {
        _room.showStartButton = false;

        Matchmaking mm = FindObjectOfType<Matchmaking>();
        if (mm) mm.StartHost();

        _room.ServerChangeScene(_room.GameplayScene);
    }

    // Called by the local player when ready button is clicked
    [Client] private void OnReadyButtonClick()
    {
        if (!readyToBegin)
        {
            _audioManager.PlayReady();
        }
        CmdChangeReadyState(!readyToBegin);
    }

    // Called by the local player when the character card is pressed
    [Client] private void OnCharacterClicked()
    {
        CmdChangeCharacterCode(GetNextAvailableCharacter());
    }

    // Called by the local player when the team button is pressed
    [Client] private void OnTeamClicked()
    {
        var newTeam = (teamIndex + 1) % FindObjectOfType<TeamsGamemode>().teams;

        _lobbySettings.localTeamIndex = newTeam;
        CmdChangeTeams(newTeam);
    }

    #endregion

    [Client] private int GetNextAvailableCharacter()
    {
        var nextCode = -1;

        var len = _characterSelectionInfo.characterAvailable.Length;

        // Check the next available character
        for (var i = characterCode + 1; i < characterCode + len + 1; i++)
        {
            if (_characterSelectionInfo.characterAvailable[i % len])
            {
                nextCode = i % len;
                break;
            }
        }

        // return the character code
        return nextCode;
    }

    [Command] private void CmdChangeCharacterCode(int code)
    {
        characterCode = code;
    }

    [Command] private void CmdChangeTeams(int idx)
    {
        teamIndex = idx;
    }
}