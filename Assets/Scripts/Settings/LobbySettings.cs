﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;


/// <summary>
/// Settings for a game, these values only have effect if they are
/// changed on the host. Otherwise
/// </summary>
public class LobbySettings : NetworkBehaviour
{
    [Header("Variables")]

    /// <summary>
    /// The max number of players that can join this lobby.
    /// </summary>
    public int maxPlayers;

    /// <summary>
    /// The number of teams player will be competing in.
    /// A value of 0 or 1 indicates that teams is not used.
    /// </summary>
    public int teams = 2;

    /// <summary>
    /// The duration of a single round, used only if timer is active
    /// </summary>
    public float roundDuration = 120f;

    [Header("Win Conditions")]
    public bool endAfterFirstWinCondition = true;

    public bool byLastAlive = true;
    public bool byTimerFinished = true;

    private void Awake()
    {
        // Persist this object across scenes
        DontDestroyOnLoad(this.gameObject);
    }
}
