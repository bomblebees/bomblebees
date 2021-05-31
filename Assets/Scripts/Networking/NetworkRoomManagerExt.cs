using System.Linq;
using System;
using UnityEngine;
using Mirror;

public class NetworkRoomManagerExt : NetworkRoomManager
{
    [SerializeField] private GameObject eventManager;
    [SerializeField] private GameObject hexGrid;
    [SerializeField] private GameObject roundManager;
    [SerializeField] private GameObject gameUIManager;
    [SerializeField] private GameObject audioManager;
    [SerializeField] private GameObject lobbySettings;

    private GameObject _settings;

    public override void OnStartServer()
    {
        base.OnStartServer();

        spawnPrefabs.Clear();
        spawnPrefabs = Resources.LoadAll<GameObject>("NetworkedPrefabs").ToList();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Prefab Loading
        var spawnablePrefabs = Resources.LoadAll<GameObject>("NetworkedPrefabs");

        NetworkClient.ClearSpawners();

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    public override void OnRoomStopClient()
    {
        // Enable lobby list
        Matchmaking mm = Matchmaking.Singleton;
        if (mm) mm.uiLeaveLobby();

        // re-enable the main menu ui
        MainMenu_UI mainMenuUI = MainMenu_UI.singleton;
        if (mainMenuUI) mainMenuUI.gameObject.SetActive(true);

        base.OnRoomStopClient();
    }

    public override void OnRoomStartServer()
    {
        base.OnRoomStartServer();

        if (_settings is null)
        {
            _settings = Instantiate(lobbySettings);
            NetworkServer.Spawn(_settings);
        }
    }

    public override void OnRoomStopServer()
    {
        base.OnRoomStopServer();

        if (!(_settings is null))
        {
            NetworkServer.Destroy(_settings);
            _settings = null;
        }
    }

    // Temp list of player colors
    private readonly Color[] _listColors = {Color.red, Color.blue, Color.yellow, Color.green};

    /// <summary>
    /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
    /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
    /// into the GamePlayer object as it is about to enter the Online scene.
    /// </summary>
    /// <param name="roomPlayer"></param>
    /// <param name="gamePlayer"></param>
    /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer,
        GameObject gamePlayer)
    {
        // turn off loading screen
        FindObjectOfType<GlobalLoadingScreen>().gameObject.GetComponent<Canvas>().enabled = false;

        if (gamePlayer.GetComponent<Player>().steamId.Equals(0))
        {
            Debug.LogWarning("Steam id not found, generating unique ID for this session");

            // generate unique ID based on the time in ms, for development
            DateTimeOffset curDate = new DateTimeOffset(DateTime.UtcNow);
            ulong timeId = (ulong) curDate.ToUnixTimeMilliseconds();

            // set the steamId temporarily to a timeId
            gamePlayer.GetComponent<Player>().playerId = timeId;
        }

        // transfer steam id
        gamePlayer.GetComponent<Player>().steamId = roomPlayer.GetComponent<NetworkRoomPlayerExt>().steamId;

        // transfer steam username
        gamePlayer.GetComponent<Player>().steamName = roomPlayer.GetComponent<NetworkRoomPlayerExt>().steamUsername;

        // transfer the index
        gamePlayer.GetComponent<Player>().playerRoomIndex = roomPlayer.GetComponent<NetworkRoomPlayerExt>().index;

        // transfer the color over
        gamePlayer.GetComponent<Player>().playerColor =
            _listColors[roomPlayer.GetComponent<NetworkRoomPlayerExt>().characterCode];

        // transfer the character chosen
        gamePlayer.GetComponent<Player>().characterCode = roomPlayer.GetComponent<NetworkRoomPlayerExt>().characterCode;

        // transfer the team chosen
        gamePlayer.GetComponent<Player>().teamIndex = roomPlayer.GetComponent<NetworkRoomPlayerExt>().teamIndex;

        // let the event manager know that the player has finished loading
        FindObjectOfType<EventManager>().OnPlayerLoadedIntoGame(gamePlayer);

        return true;
    }

    /*
        This code below is to demonstrate how to do a Start button that only appears for the Host player
        showStartButton is a local bool that's needed because OnRoomServerPlayersReady is only fired when
        all players are ready, but if a player cancels their ready state there's no callback to set it back to false
        Therefore, allPlayersReady is used in combination with showStartButton to show/hide the Start button correctly.
        Setting showStartButton false when the button is pressed hides it in the game scene since NetworkRoomManager
        is set as DontDestroyOnLoad = true.
    */

    public bool showStartButton;

    public override void OnRoomServerPlayersReady()
    {
        // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
#if UNITY_SERVER
        base.OnRoomServerPlayersReady();
#else
        showStartButton = true;
#endif
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if (sceneName.Equals(GameplayScene))
        {
            NetworkServer.Spawn(Instantiate(eventManager));
            NetworkServer.Spawn(Instantiate(hexGrid));
            NetworkServer.Spawn(Instantiate(roundManager));
            NetworkServer.Spawn(Instantiate(gameUIManager));
            NetworkServer.Spawn(Instantiate(audioManager));
        }
    }

    public override void OnGUI()
    {
        // Matchmaking mm = Matchmaking.Singleton;
        // if (mm) return;
        //
        // base.OnGUI();
        //
        // if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        // {
        //     // set to false to hide it in the game scene
        //     showStartButton = false;
        //
        //     ServerChangeScene(GameplayScene);
        // }
    }
}