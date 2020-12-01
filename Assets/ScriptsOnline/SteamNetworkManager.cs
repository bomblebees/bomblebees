using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SteamNetworkManager : NetworkManager
{
    [Header("Custom Features")]
    public GameObject _HexGrid;

    private void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            SpawnHexGrid();
        }
    }

    [Server]
    void SpawnHexGrid()
    {
        NetworkServer.Spawn(Instantiate(_HexGrid));
    }
}
