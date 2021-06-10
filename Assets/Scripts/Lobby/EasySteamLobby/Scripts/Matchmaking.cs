//-----------------------------------------------------------------------------
// Project Name    : Easy Steam Lobby
// File Name       : Matchmaking.cs
// Creation Date   : 1/12/2020
//
// Copyright 2020 Inugoya.  All rights reserverd.
//-----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

/* Lobby Metadata
    LobbyVersion:[=string lobbyVersion]
    LobbyName:[=string lobbyPanel.GetComponent<LobbyRoomComponents>().LobbyName.text]
    GameMode:[0,1,2]
    isStartGame:[True,False]
*/

/* Lobby Member Data
     PlayerState:[InGame,Ready,Wait]
*/


public class Matchmaking : MonoBehaviour
{
    private GlobalButtonSettings _globalButtonSettings;

    private string LOBBY_MATCHING_KEY = "[key:bomblebees-lobby]";
    private string LOBBY_MATCHING_VALUE = "[true]";
    private const string HostAddressKey = "HostAddress";

    // The network manager for game scene networking
    [Header("Matchmaking")]
    [SerializeField] private SteamNetworkManager networkManager;

    //this function will be called when the host player pressed the start button.
    public void StartHost()
    {
        Debug.Log("Created the match as the host.");

        if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") == "Ready")
        {
            SteamMatchmaking.SetLobbyMemberData(currentLobby, "PlayerState", "InGame");
            SteamMatchmaking.SetLobbyData(currentLobby, "gameStatus", "In Game");

            //// Change to game scene
            //networkManager.ServerChangeScene(networkManager.GameplayScene);

            //// Disable ui
            //LobbyCanvas.SetActive(false);
        }

    }

    //This function will be called when the guest player is ready 
    //and the Lobby Metadata "isStartGame" is True.
    void JoinMatch()
    {
        Debug.Log("Joinning the match.");

        //if(OnlineGame == Started){
        //SteamMatchmaking.SetLobbyMemberData(currentLobby, "PlayerState", "InGame");
        //return;
        //}
        //Invoke("Joinmatch", 1f);
    }

    public void CreateLobby()
    {
        SteamAPICall_t handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, (int)sliderMaxPlayers.value + 1);
        OnLobbyCreatedCallResult.Set(handle);
    }

    public void OnMirrorSetStatus(string status)
    {
        SteamMatchmaking.SetLobbyData(currentLobby, "gameStatus", status);
    }

    public void OnMirrorLobbyCreated()
    {
        // Start Mirror host
        networkManager.StartHost();
    }

    public void OnMirrorLobbyEnter(LobbyEnter_t callback)
    {
        LobbyCanvas.SetActive(false);

        // If host, do nothing
        if (NetworkServer.active) return;

        string hostAddress = SteamMatchmaking.GetLobbyData(
            currentLobby,
            HostAddressKey);

        // Start Mirror Client
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        MainMenu_UI menu = MainMenu_UI.Singleton;
        menu.screenLoading.SetActive(true);
        menu.screenNavigation.SetActive(false);
    }

    public void SetInitiatedLobbyData()
    {
        SteamMatchmaking.SetLobbyData(currentLobby, "LobbyVersion", Application.version);
        SteamMatchmaking.SetLobbyData(currentLobby, LOBBY_MATCHING_KEY, LOBBY_MATCHING_VALUE);

        SteamMatchmaking.SetLobbyData(
            currentLobby,
            HostAddressKey,
            SteamUser.GetSteamID().ToString());

        SteamMatchmaking.SetLobbyData(currentLobby, "gameStatus", "In Lobby");

        if (!string.IsNullOrEmpty(inputLobbyName.text))
        {
            SteamMatchmaking.SetLobbyData(currentLobby, "LobbyName", inputLobbyName.text);
            lobbyPanel.GetComponent<LobbyRoomComponents>().LobbyName.text = inputLobbyName.text;
        }
        else
        {
            SteamMatchmaking.SetLobbyData(currentLobby, "LobbyName", SteamFriends.GetPersonaName() + "'s Lobby");
            lobbyPanel.GetComponent<LobbyRoomComponents>().LobbyName.text = SteamFriends.GetPersonaName() + "'s Lobby";
        }

        int gamemode = 0;
        if (gameModes != null)
        {
            gamemode = gameModes.value;
        }
        SteamMatchmaking.SetLobbyData(currentLobby, "GameMode", gamemode.ToString());
    }

    public void uiJoinLobby(ulong l)
    {
        if (SteamMatchmaking.GetLobbyData((CSteamID)l, "gameStatus") == "In Game")
        {
            return;
        }
        else
        {
            CSteamID c = new CSteamID(l);

            SteamMatchmaking.JoinLobby(c);
        }
    }

    public void MirrorLeaveLobby()
    {
        if (NetworkServer.active) networkManager.StopHost();
        else if (NetworkClient.isConnected) networkManager.StopClient();

        uiLeaveLobby();
    }

    public void uiLeaveLobby()
    {
        if (currentLobby != (CSteamID)0)
            SteamMatchmaking.LeaveLobby(currentLobby);

        currentLobby = (CSteamID)0;
        lobbyPlayers.Clear();

        if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") != "InGame")
        {
            isLobbyHost = false;
            if (lobbyPanel != null)
                lobbyPanel.SetActive(false);
            if (lobbyListPanel != null)
                lobbyListPanel.SetActive(true);
        }

        lobbyOwner = (CSteamID)0;
        currentLobby = new CSteamID();

        LobbyCanvas.SetActive(true);
        MainMenu_UI menu = MainMenu_UI.Singleton;
        menu.gameObject.SetActive(true);

        PresentLobbies(); // refresh lobbies
    }

    public string GetLobbyName()
    {
        return SteamMatchmaking.GetLobbyData(currentLobby, "LobbyName");
    }

    #region EasySteamLobby
    public static Matchmaking Singleton;

    public string lobbyVersion;

    public CSteamID currentLobby;
    public CSteamID lobbyOwner;
    public bool isLobbyHost;

    private Callback<LobbyEnter_t> m_LobbyEnter;
    private Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;
    private Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
    private Callback<LobbyChatMsg_t> m_LobbyChatMsg;
    private Callback<LobbyGameCreated_t> m_LobbyGameCreated;
    protected Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;
    private CallResult<LobbyEnter_t> OnLobbyEnterCallResult;
    private CallResult<LobbyMatchList_t> OnLobbyMatchListCallResult;
    private CallResult<LobbyCreated_t> OnLobbyCreatedCallResult;

    private CallResult<GameLobbyJoinRequested_t> OnGameLobbyJoinRequestedCallResult;

    public List<PeerInfo> lobbyPlayers = new List<PeerInfo>();
    private List<GameObject> lobbyPanels = new List<GameObject>();
    public PeerInfo myPlayer;

    public GameObject LobbyCanvas;

    public List<CSteamID> lobbies;
    public GameObject lobbyRowPrefab;
    public RectTransform scrollViewContent;
    public GameObject lobbyListPanel;
    public GameObject createLobbyWindow;
    public Text inputLobbyName;
    public Text txtMaxLobbyPlayers;
    public Slider sliderMaxPlayers;
    public Dropdown gameModes;
    public GameObject lobbyPanel;

    public bool isStartgame = false;

    public void OnEnable()
    {
        m_LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        m_LobbyGameCreated = Callback<LobbyGameCreated_t>.Create(OnLobbyGameCreated);
        m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
        m_GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);

        OnLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
        OnLobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
        OnLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);

        OnGameLobbyJoinRequestedCallResult = CallResult<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
    }

    private void OnDisable()
    {
        CloseCreateLobbyWindow();
    }

    private void Awake()
    {
        Singleton = this;

        LobbyCanvas = GameObject.Find("LobbyCanvas");

        isLobbyHost = false;
    }

    public void GetLobbies()
    {
        SteamMatchmaking.AddRequestLobbyListStringFilter(LOBBY_MATCHING_KEY, LOBBY_MATCHING_VALUE, ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
        OnLobbyMatchListCallResult.Set(SteamMatchmaking.RequestLobbyList());
    }

    public void OpenCreateLobbyWindow()
    {
        lobbyListPanel.SetActive(false);
        createLobbyWindow.SetActive(true);
    }

    public void CloseCreateLobbyWindow()
    {
        createLobbyWindow.SetActive(false);
        lobbyListPanel.SetActive(true);
    }

    public void isLobbyOwner(bool enabled)
    {
        isLobbyHost = enabled;
        if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") != "InGame")
        {
            if (lobbyPanel != null)
                lobbyPanel.transform.Find("btnStart").gameObject.SetActive(enabled);
            if (lobbyPanel != null)
                lobbyPanel.transform.Find("btnReady").gameObject.SetActive(!enabled);
        }

        if (enabled)
        {
            lobbyOwner = SteamUser.GetSteamID();
        }
    }

    public void PresentLobbies()
    {
        foreach (Transform t in scrollViewContent.gameObject.transform)
        {
            Destroy(t.gameObject);
        }

        int i = 0;
        foreach (CSteamID l in lobbies)
        {
            RectTransform r = (Instantiate(lobbyRowPrefab, scrollViewContent) as GameObject).GetComponent<RectTransform>();
            r.anchoredPosition += new Vector2(0, -90 * i);

            RowComponents rc = r.GetComponent<RowComponents>();
            rc.txtLobbyName.text = SteamMatchmaking.GetLobbyData(l, "LobbyName");
            rc.txtNumMem.text = SteamMatchmaking.GetNumLobbyMembers(l) + " / " + (SteamMatchmaking.GetLobbyMemberLimit(l) - 1);
            rc.txtVer.text = SteamMatchmaking.GetLobbyData(l, "LobbyVersion");
            rc.txtStatus.text = SteamMatchmaking.GetLobbyData(l, "gameStatus");
            
            // Update join button
            LobbyJoinButton lobbyJoinButton = rc.joinButton.GetComponent<LobbyJoinButton>();
            lobbyJoinButton.DeactivateButton();

            if (SteamMatchmaking.GetNumLobbyMembers(l) != SteamMatchmaking.GetLobbyMemberLimit(l) - 1 &&
                SteamMatchmaking.GetLobbyData(l, "gameStatus") != "In Game" &&
                SteamMatchmaking.GetLobbyData(l, "LobbyVersion") == Application.version)
            {
                lobbyJoinButton.ActivateButton();
            }
            
            rc.joinButton.onClick.AddListener(delegate { uiJoinLobby(l.m_SteamID); });

            i++;
        }
    }

    void GetLobbyPlayers()
    {
        if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") == "InGame")
            return;

        lobbyPlayers = new List<PeerInfo>();
        if (lobbyPanel != null)
            lobbyPanel.GetComponent<LobbyRoomComponents>().Players.text = null;

        for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(currentLobby); i++)
        {
            PeerInfo p = new PeerInfo();
            p.steamID = SteamMatchmaking.GetLobbyMemberByIndex(currentLobby, i);

            if (p.steamID == SteamMatchmaking.GetLobbyOwner(currentLobby))
            {
                if (lobbyPanel != null)
                    lobbyPanel.GetComponent<LobbyRoomComponents>().Players.text += "*";
                lobbyOwner = p.steamID;
                if (p.steamID == SteamUser.GetSteamID())
                {
                    isLobbyOwner(true);
                    isStartgame = false;
                }
            }

            if (lobbyPanel != null)
                lobbyPanel.GetComponent<LobbyRoomComponents>().Players.text += SteamFriends.GetFriendPersonaName(p.steamID);

            if (SteamMatchmaking.GetLobbyMemberData(currentLobby, p.steamID, "PlayerState") == "InGame")
            {
                if (lobbyPanel != null)
                    lobbyPanel.GetComponent<LobbyRoomComponents>().Players.text += ": In-Game";
            }
            else if (SteamMatchmaking.GetLobbyMemberData(currentLobby, p.steamID, "PlayerState") == "Ready")
            {
                if (lobbyPanel != null)
                    lobbyPanel.GetComponent<LobbyRoomComponents>().Players.text += ": Ready";
            }

            if (lobbyPanel != null)
                lobbyPanel.GetComponent<LobbyRoomComponents>().Players.text += "\n";

            lobbyPlayers.Add(p);
        }

        if (lobbyPanel != null)
            Invoke("GetLobbyPlayers", 1f);
    }

    void CheckNoOnePlaying()
    {
        if (currentLobby.m_SteamID != 0)
        {
            if (SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(currentLobby))
            {
                for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(currentLobby); i++)
                {
                    if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamMatchmaking.GetLobbyMemberByIndex(currentLobby, i), "PlayerState") == "InGame")
                    {
                        Invoke("CheckNoOnePlaying", 1f);

                        return;
                    }
                }
            }
        }

        Invoke("CheckNoOnePlaying", 1f);
    }

    void AddLobbyPlayer(ulong id)
    {
        PeerInfo p = new PeerInfo();
        p.steamID = new CSteamID(id);

        if (lobbyPanel != null)
            lobbyPanel.GetComponent<LobbyRoomComponents>().Players.text += SteamFriends.GetFriendPersonaName(p.steamID) + "\n";

        if (lobbyPanel != null)
            lobbyPanel.GetComponent<LobbyRoomComponents>().startButton.interactable = false;

        lobbyPlayers.Add(p);

        if (lobbyPanel != null)
            lobbyPanel.GetComponent<LobbyRoomComponents>().startButton.interactable = true;
    }

    void RemoveLobbyPlayer(ulong id)
    {
        CSteamID steamID = new CSteamID(id);
        PeerInfo onLobby = isPlayerOnLobby(steamID);

        Destroy(onLobby.gameObject);
        lobbyPlayers.Remove(onLobby);
    }

    public PeerInfo isPlayerOnLobby(CSteamID player)
    {
        foreach (PeerInfo p in lobbyPlayers)
        {
            if (p.steamID.Equals(player))
            {
                return p;
            }
        }
        return null;
    }

    private void Start()
    {
        GetLobbies();

        Invoke("CheckNoOnePlaying", 1f);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
            Destroy(gameObject);

        if (currentLobby.m_SteamID != 0)
        {
            if (SteamMatchmaking.GetLobbyData(currentLobby, "isStartGame") == "True")
            {
                if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") == "Ready")
                {
                    if (!isStartgame)
                    {
                        isStartgame = true;
                        Play();
                    }
                }
            }
            else
            {

            }
        }

        if (currentLobby == (CSteamID)0)
            txtMaxLobbyPlayers.text = "Max Players: " + sliderMaxPlayers.value;
    }

    public void ReadyButtonDown()
    {
        if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") == "Wait")
        {
            SteamMatchmaking.SetLobbyMemberData(currentLobby, "PlayerState", "Ready");
        }
        else if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") == "Ready")
        {
            SteamMatchmaking.SetLobbyMemberData(currentLobby, "PlayerState", "Wait");
        }
    }

    public void StartButtonDown()
    {
        if (SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(currentLobby))
        {
            if (SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") == "InGame" ||
                SteamMatchmaking.GetLobbyMemberData(currentLobby, SteamUser.GetSteamID(), "PlayerState") == "Ready")
            {
                SteamMatchmaking.SetLobbyMemberData(currentLobby, "PlayerState", "Wait");
                SteamMatchmaking.SetLobbyData(currentLobby, "isStartGame", "False");

                return;
            }

            Debug.Log("OnlineGameStart");
            SteamMatchmaking.SetLobbyMemberData(currentLobby, "PlayerState", "Ready");
            SteamMatchmaking.SetLobbyData(currentLobby, "isStartGame", "True");
        }
    }

    private void Play()
    {
        if (SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(currentLobby))
        {
            StartHost();
        }
        else
        {
            JoinMatch();
        }
    }

    public void Wait()
    {
        SteamMatchmaking.SetLobbyData(currentLobby, "isStartGame", "False");
        isStartgame = false;
    }

    public void InitializeLobbyGame()
    {
        SteamMatchmaking.SetLobbyData(currentLobby, "matchID", "");
    }

    public void SetMyState(string pchKey, string pchValue)
    {
        SteamMatchmaking.SetLobbyMemberData(currentLobby, pchKey, pchValue);

        Debug.Log(pchKey + ": " + pchValue);
    }

    void ChatUpdate()
    {

    }

    void OnLobbyEnter(LobbyEnter_t pCallback)
    {
        currentLobby = (CSteamID)pCallback.m_ulSteamIDLobby;
        lobbyOwner = SteamMatchmaking.GetLobbyOwner(currentLobby);

        isStartgame = false;
        SteamMatchmaking.SetLobbyMemberData(currentLobby, "PlayerState", "Wait");
        if (lobbyPanel != null)
            lobbyPanel.GetComponent<LobbyRoomComponents>().LobbyName.text = SteamMatchmaking.GetLobbyData(currentLobby, "LobbyName");

        GetLobbyPlayers();

        if (lobbyListPanel != null)
            lobbyListPanel.SetActive(false);
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        OnMirrorLobbyEnter(pCallback);
    }

    void OnLobbyEnter(LobbyEnter_t pCallback, bool bIOFailure)
    {
        OnLobbyEnter(pCallback);
    }

    void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback)
    {

    }

    void OnLobbyGameCreated(LobbyGameCreated_t pCallback)
    {

    }

    void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        lobbies.Clear();
        for (int i = 0; i < pCallback.m_nLobbiesMatching; i++)
        {
            lobbies.Add(SteamMatchmaking.GetLobbyByIndex(i));
        }

        PresentLobbies();
    }

    void OnLobbyCreated(LobbyCreated_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + LobbyCreated_t.k_iCallback + " - LobbyCreated] - " + pCallback.m_eResult + " -- " + pCallback.m_ulSteamIDLobby);

        currentLobby = (CSteamID)pCallback.m_ulSteamIDLobby;

        isLobbyOwner(true);

        SteamMatchmaking.SetLobbyMemberData(currentLobby, "PlayerState", "Wait");

        SetInitiatedLobbyData();
        createLobbyWindow.SetActive(false);
        lobbyPanel.SetActive(true);

        OnMirrorLobbyCreated();
    }

    void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        if (pCallback.m_rgfChatMemberStateChange == 1)
        {
            AddLobbyPlayer(pCallback.m_ulSteamIDUserChanged);
        }
        else
        {
            RemoveLobbyPlayer(pCallback.m_ulSteamIDUserChanged);
        }
    }

    void OnLobbyChatMsg(LobbyChatMsg_t pCallback)
    {

    }

    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t pCallback)
    {
        SteamMatchmaking.JoinLobby(pCallback.m_steamIDLobby);
    }

    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t pCallback, bool IOFailure)
    {
        OnGameLobbyJoinRequested(pCallback);
    }
    #endregion
}