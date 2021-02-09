using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private GameObject hexGrid;

    private void Start()
    {
        SpawnScene();
    }

    [ServerCallback]
    void SpawnScene()
    {
        RpcTest("SpawnScene()");

        NetworkServer.Spawn(Instantiate(hexGrid));
    }

    [ClientRpc]
    void RpcTest(string text)
    {
        Debug.Log(text);
    }
}
