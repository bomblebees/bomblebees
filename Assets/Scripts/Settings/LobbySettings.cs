using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbySettings : NetworkBehaviour
{

    /// <summary>
    /// The max number of players that can join this lobby.
    /// </summary>
    public int maxPlayers;

    /// <summary>
    /// The number of teams player will be competing in.
    /// A value of 0 or 1 indicates that teams is not used.
    /// </summary>
    public int teams = 2;

    private void OnEnable()
    {
        // Persist this object across scenes
        DontDestroyOnLoad(this);
    }
}
