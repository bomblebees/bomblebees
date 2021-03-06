using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;
using Mirror;
using Steamworks;

public class RoundManager : NetworkBehaviour
{
    public class PlayerInfo
    {
        public Health health;
        public Player player;
        public ulong steamId;
    }

    public List<PlayerInfo> playerList = new List<PlayerInfo>();

    [Header("Round Settings")]
    [SerializeField] public int startGameFreezeDuration = 5;
    [SerializeField] public int endGameFreezeDuration = 5;

    [NonSerialized] public int playersConnected = 0;
    [NonSerialized] public int totalRoomPlayers;


    // events
    public delegate void PlayerConnectedDelegate(PlayerInfo p);
    public delegate void RoundStartDelegate();
    public delegate void RoundEndDelegate();
    public event PlayerConnectedDelegate EventPlayerConnected;
    public event RoundStartDelegate EventRoundStart;
    public event RoundEndDelegate EventRoundEnd;

    // singletons
    public static RoundManager _instance;
    public static RoundManager Singleton { get { return _instance;  } }

    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: RoundManager");
        else _instance = this;
    }

    #region Setup

    public override void OnStartServer()
    {
        NetworkRoomManagerExt room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        totalRoomPlayers = room.roomSlots.Count;
    }
    
    [Client]
    public override void OnStartClient()
    {
        try
        {
            ulong steamId = SteamUser.GetSteamID().m_SteamID;
            CmdAddPlayerToRound(steamId);
        } catch
        {
            CmdAddPlayerToRound(0);
        }
    }

    [Command(ignoreAuthority = true)]
    public void CmdAddPlayerToRound(ulong steamId, NetworkConnectionToClient sender = null)
    {
        playersConnected++;

        // Add player to list
        Player player = sender.identity.gameObject.GetComponent<Player>();
        Health live = sender.identity.gameObject.GetComponent<Health>();

        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.player = player;
        playerInfo.health = live;
        playerInfo.steamId = steamId;

        playerList.Add(playerInfo);

        // invoke event after added to list
        EventPlayerConnected?.Invoke(playerInfo);

        // Subscribe to life change event
        live.EventLivesChanged += CheckRoundEnd;


        if (totalRoomPlayers == playersConnected)
        {
            StartRound();
        }
    }

    #endregion

    [Server]
    public void StartRound()
    {
        RpcStartRound();
        StartCoroutine(ServerStartRound());
    }

    [Server]
    public void EndRound()
    {
        RpcEndRound();
        StartCoroutine(ServerEndRound());
    }

    [ClientRpc] public void RpcStartRound() { EventRoundStart?.Invoke(); }
    [ClientRpc] public void RpcEndRound() { EventRoundEnd?.Invoke(); }

    [Server]
    public IEnumerator ServerStartRound()
    {
        yield return new WaitForSeconds(startGameFreezeDuration + 1);
        for (int i = 0; i < playerList.Count; i++)
        {
            Player p = playerList[i].player;
            p.SetCanPlaceBombs(true);
            p.SetCanSpin(true);
            p.SetCanSwap(true); 
            p.SetCanMove(true);
        }
    }

    [Server]
    public IEnumerator ServerEndRound()
    {
        yield return new WaitForSeconds(endGameFreezeDuration);
        NetworkRoomManagerExt room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        room.ServerChangeScene(room.RoomScene);
    }

    [Server]
    public void CheckRoundEnd(int currentHealth, int maxHealth)
    {
        if (currentHealth < 1)
        {
            int aliveCount = 0;
            for (int i = 0; i < playerList.Count; i++)
            {
                Debug.Log("ROUND MANAGER: player " + i + " has lives: " + playerList[i].health.currentLives);
                if (playerList[i].health.currentLives > 0)
                {
                    aliveCount++;
                }
            }

            // End the round/game if only one player alive
            if (aliveCount <= 1)
            {
                EndRound();
            }
        }
    }
}
