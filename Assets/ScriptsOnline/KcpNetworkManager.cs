using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Zenject;

public class KcpNetworkManager : NetworkManager
{
    private void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            SpawnHexGrid();
        }
        
        if (Input.GetKeyDown("3"))
        {
            SpawnLaserObject();
        }
        
        if (Input.GetKeyDown("4"))
        {
            SpawnBombObject();
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

    [Header("Test")]
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _hexGrid;
    [SerializeField] private GameObject _laserObject;
    [SerializeField] private GameObject _bombObject;
    
    
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject player = Instantiate(_player);
        NetworkServer.Spawn(player);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    [ServerCallback]
    void SpawnHexGrid()
    {
        NetworkServer.Spawn(Instantiate(_hexGrid));
    }

    [ServerCallback]
    void SpawnLaserObject()
    {
        NetworkServer.Spawn(Instantiate(_laserObject));
    }

    [ServerCallback]
    void SpawnBombObject()
    {
        NetworkServer.Spawn(Instantiate(_bombObject));
    }
}
