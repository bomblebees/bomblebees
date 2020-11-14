using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class KcpNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        Debug.Log("server started.");
        base.OnStartServer();
    }

    public override void OnStopServer()
    {
        Debug.Log("server stopped.");
        base.OnStopServer();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("connected to server.");
        base.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("disconnected from server.");
        base.OnClientDisconnect(conn);
    }
}
