using System.Linq;
using Mirror;
using Steamworks;
using UnityEngine;

public class SteamNetworkManager : NetworkManager
{
    [Header("Spawnable GameObject")] 
    
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject hexGrid;

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
    }
    
    [ServerCallback]
    void SpawnScene()
    {
        NetworkServer.Spawn(Instantiate(hexGrid));
    }
}

