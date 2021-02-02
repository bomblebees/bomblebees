using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Steamworks;
using UnityEngine;

public class SteamNetworkManagerLobby : NetworkManager
{
    [Header("Spawnable GameObject")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject hexGrid;

    [SerializeField] private int minPlayers = 1;
    [Scene] [SerializeField] private string menuScene = string.Empty;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayerLobby gamePlayerPrefab = null;
    
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;
    public static event Action OnServerStopped;
    
    public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>();
    public List<NetworkGamePlayerLobby> GamePlayers { get; } = new List<NetworkGamePlayerLobby>();
    
    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
        SpawnScene();
    } 


    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("Prefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }
    
    public override void OnClientConnect(NetworkConnection conn)
    {
        if (!clientLoadedScene)
        {
            // Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
            if (!ClientScene.ready) ClientScene.Ready(conn);

            ClientScene.AddPlayer(conn);
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject instantiatePlayer = Instantiate(player);
        NetworkServer.Spawn(instantiatePlayer);
        NetworkServer.AddPlayerForConnection(conn, instantiatePlayer);
        
        /*CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(
            SteamLobby.LobbyId,
            numPlayers - 1);

        var playerInfoDisplay = conn.identity.GetComponent<PlayerInfoDisplay>();

        playerInfoDisplay.SetSteamId(steamId.m_SteamID);*/
    }
    
    [ServerCallback]
    void SpawnScene()
    {
        NetworkServer.Spawn(Instantiate(hexGrid));
    }
}


