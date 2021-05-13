using System.Linq;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkRoomManagerExt : NetworkRoomManager
{
    [SerializeField] private GameObject eventManager;
    [SerializeField] private GameObject hexGrid;
    [SerializeField] private GameObject roundManager;
    [SerializeField] private GameObject gameUIManager;
    [SerializeField] private GameObject audioManager;
    [SerializeField] private GameObject lobbySettings;

    private GameObject settings = null;

    public override void OnStartServer()
    {
        base.OnStartServer();

        spawnPrefabs.Clear();
        spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Prefab Loading
        var spawnablePrefabs = Resources.LoadAll<GameObject>("Prefabs");

        NetworkClient.ClearSpawners();

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    public override void OnRoomStopClient()
    {
        // Enable lobby list
        Matchmaking mm = Matchmaking.singleton;
        if (mm) mm.uiLeaveLobby();

        base.OnRoomStopClient();
    }

    public override void OnRoomStartServer()
    {
        base.OnRoomStartServer();

        if (settings == null)
        {
            settings = Instantiate(lobbySettings);
            NetworkServer.Spawn(settings);
        }
    }

    public override void OnRoomStopServer()
    {
        base.OnRoomStopServer();

        if (settings != null)
        {
            NetworkServer.Destroy(settings);
            settings = null;
        }
    }

    // Temp list of player colors
    private Color[] listColors = { Color.red, Color.blue, Color.yellow, Color.green };

    /// <summary>
    /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
    /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
    /// into the GamePlayer object as it is about to enter the Online scene.
    /// </summary>
    /// <param name="roomPlayer"></param>
    /// <param name="gamePlayer"></param>
    /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {

        if (gamePlayer.GetComponent<Player>().steamId == 0)
        {
            Debug.LogWarning("Steam id not found, generating unique ID for this session");

            // generate unique ID based on the time in ms, for development
            DateTimeOffset curDate = new DateTimeOffset(DateTime.UtcNow);
            ulong timeId = (ulong)curDate.ToUnixTimeMilliseconds();

            // set the steamId temporarily to a timeId
            gamePlayer.GetComponent<Player>().playerId = timeId;
        }

        // transfer the index
        gamePlayer.GetComponent<Player>().playerRoomIndex = roomPlayer.GetComponent<NetworkRoomPlayerExt>().index;
        
        // transfer the color over
        gamePlayer.GetComponent<Player>().playerColor = listColors[roomPlayer.GetComponent<NetworkRoomPlayerExt>().characterCode];
        
        // transfer the character chosen
        gamePlayer.GetComponent<Player>().characterCode = roomPlayer.GetComponent<NetworkRoomPlayerExt>().characterCode;

        // transfer the team chosen
        gamePlayer.GetComponent<Player>().teamIndex = roomPlayer.GetComponent<NetworkRoomPlayerExt>().teamIndex;

        // let the round manager know that the player has finished loading
        RoundManager roundManager = FindObjectOfType<RoundManager>();
        roundManager.OnPlayerLoadedIntoRound(gamePlayer);

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
        if (sceneName == GameplayScene)
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
        Matchmaking mm = Matchmaking.singleton;
        if (mm) return;

        base.OnGUI();

        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        {
            // set to false to hide it in the game scene
            showStartButton = false;

            ServerChangeScene(GameplayScene);
        }
    }
}
