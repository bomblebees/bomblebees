using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerLobby : NetworkManager
{
    [SerializeField] private int minPlayers = 2;
    [Scene] [SerializeField] private string menuScene = string.Empty;

    [Header("Maps")]
    [SerializeField] private int numberOfRounds = 1;
    [SerializeField] private MapSet mapSet = null;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayerLobby gamePlayerPrefab = null;
    [SerializeField] private GameObject playerSpawnSystem = null;
    [SerializeField] private GameObject roundSystem = null;

    private MapHandler mapHandler;
    [SerializeField] private GameObject hexGrid;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;
    public static event Action OnServerStopped;

    public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>();
    public List<NetworkGamePlayerLobby> GamePlayers { get; } = new List<NetworkGamePlayerLobby>();

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("Prefabs");

        ClientScene.ClearSpawners();

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        /*base.OnClientConnect(conn);

        OnClientConnected?.Invoke();*/
        
        if (!clientLoadedScene)
        {
            // Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
            if (!ClientScene.ready) ClientScene.Ready(conn);

            ClientScene.AddPlayer(conn);
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("NetworkManagerLobby.cs: OnServerAddPLayer");
        
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            Debug.Log("True: SceneManager.GetActiveScene().name == menuScene");
            
            bool isLeader = RoomPlayers.Count == 0;

            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        } 
        else
        {
            Debug.Log("False: SceneManager.GetActiveScene().name == menuScene");
            Debug.Log("Scene 1: " + SceneManager.GetActiveScene().name);
            Debug.Log("Scene 2: " + menuScene);
            
            bool isLeader = RoomPlayers.Count == 0;

            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        OnServerStopped?.Invoke();

        RoomPlayers.Clear();
        GamePlayers.Clear();
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) { return false; }

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }

        return true;
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            if (!IsReadyToStart()) { return; }

            //mapHandler = new MapHandler(mapSet, numberOfRounds);

            //ServerChangeScene(mapHandler.NextMap);
            ServerChangeScene("level1.1");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        // From menu to game
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject, true);
                NetworkServer.Destroy(conn.identity.gameObject);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        // Need to spawn in hexgrid so players have a reference to it
        Debug.Log("spawning hexGrid");
        NetworkServer.Spawn(Instantiate(hexGrid));

        GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
        NetworkServer.Spawn(playerSpawnSystemInstance);

        //if (sceneName.StartsWith("Scene_Map"))
        //{
        //    GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
        //    NetworkServer.Spawn(playerSpawnSystemInstance);

        //    GameObject roundSystemInstance = Instantiate(roundSystem);
        //    NetworkServer.Spawn(roundSystemInstance);
        //}
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }
}
