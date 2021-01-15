using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SteamNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        ServerChangeScene("Scene_SteamworksLobby");
    }
}