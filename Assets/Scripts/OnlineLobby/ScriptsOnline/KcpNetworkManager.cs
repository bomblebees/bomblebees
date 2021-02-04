using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class KcpNetworkManager : NetworkManager
{
    [Header("Spawnable GameObject")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject hexGrid;
    [SerializeField] private GameObject developmentUI;
    [SerializeField] private GameObject eventManager;
    [SerializeField] private GameObject levelManager;
    [SerializeField] private GameObject healthManager;
    [SerializeField] private GameObject playUI;
    //[SerializeField] private GameObject bombObject;
    //[SerializeField] private GameObject laserObject;

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
        SpawnScene();
    } 


    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("Prefabs");

        // Clear for host client to clear warning messages
        ClientScene.ClearSpawners();

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
        //NetworkServer.Spawn(Instantiate(developmentUI));
        //NetworkServer.Spawn(Instantiate(eventManager));
        //NetworkServer.Spawn(Instantiate(levelManager));
        //NetworkServer.Spawn(Instantiate(healthManager));
        //NetworkServer.Spawn(Instantiate(playUI));
    }
}
