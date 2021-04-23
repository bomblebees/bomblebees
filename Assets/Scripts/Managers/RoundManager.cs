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
    public List<PlayerInfo> alivePlayers = new List<PlayerInfo>();

    [SyncVar(hook = nameof(UpdateGridsAliveCount))]
    public int aliveCount = -1;

    [Header("Round Settings")]
    [SerializeField] public int startGameFreezeDuration = 5;
    [SerializeField] public int endGameFreezeDuration = 5;

    [SerializeField] public bool useRoundTimer = true;
    [SerializeField] public float roundDuration = 60.0f;

    [NonSerialized] public int playersConnected = 0;
    [NonSerialized] public int totalRoomPlayers;

    [NonSerialized]
    public HexGrid hexGrid;


    // events
    public delegate void PlayerConnectedDelegate(PlayerInfo p);
    public delegate void RoundStartDelegate();
    public delegate void RoundEndDelegate(GameObject winner);
    public event PlayerConnectedDelegate EventPlayerConnected;
    public event RoundStartDelegate EventRoundStart;
    public event RoundEndDelegate EventRoundEnd;

    // singletons
    public static RoundManager _instance;
    public static RoundManager Singleton { get { return _instance;  } }

    private EventManager eventManager;

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

        // Event manager singleton
        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");
        hexGrid = GameObject.FindObjectOfType<HexGrid>();
    }

    public void UpdateGridsAliveCount(int oldAliveCount, int newAliveCount)
    {
        if(hexGrid)
        hexGrid.SetAliveCount(newAliveCount);
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
        live.EventLivesChanged += OnLivesChanged;


        if (totalRoomPlayers == playersConnected)
        {
            StartRound();
        }
    }

    #endregion

    [Server]
    public void StartRound()
    {
        eventManager.OnStartRound();
        RpcStartRound();
        StartCoroutine(ServerStartRound());
    }

    [Server]
    public IEnumerator StartRoundTimer()
    {
        yield return new WaitForSeconds(roundDuration);

        if (!CheckRoundEnd())
        {

            // Player with lowest lives win
            Player winner = GetWinnerPlayerByLives();
            if (winner != null)
            {
                EndRound(winner);
            } else
            {
                // Else, some other way to determine winner here

            }
        }

    }

    [Server]
    public void EndRound(Player winner = null)
    {
        // Append all player objects to player list for event manager
        List<Player> players = new List<Player>();
        playerList.ForEach(pi => players.Add(pi.player));

        eventManager.OnEndRound(players);
        RpcEndRound(winner.gameObject);
        StartCoroutine(ServerEndRound());
    }

    [ClientRpc]
    public void RpcStartRound()
    {
        EventRoundStart?.Invoke();

    }
    [ClientRpc] public void RpcEndRound(GameObject winner) 
    {
        EventRoundEnd?.Invoke(winner);
    }

    [Server]
    public IEnumerator ServerStartRound()
    {
        yield return new WaitForSeconds(startGameFreezeDuration + 1);
        for (int i = 0; i < playerList.Count; i++)
        {
            alivePlayers.Add(playerList[i]);
            Player p = playerList[i].player;
            p.SetCanPlaceBombs(true);
            p.SetCanSpin(true);
            p.SetCanSwap(true); 
            p.SetCanMove(true);
        }
        aliveCount = alivePlayers.Count;
        UpdateGridsAliveCount(0, aliveCount);
        if (useRoundTimer) StartCoroutine(StartRoundTimer());
    }

    [Server]
    public IEnumerator ServerEndRound()
    {
        yield return new WaitForSeconds(endGameFreezeDuration);
        eventManager.OnReturnToLobby(); // invoke event

        NetworkRoomManagerExt room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        room.ServerChangeScene(room.RoomScene);
    }

    [Server]
    public void OnLivesChanged(int currentHealth, int _, GameObject __)
    {
        if (currentHealth < 1)
        {
            // Remove players that are dead
            for (int i = 0; i < playerList.Count; i++)
            {
                //Debug.Log("ROUND MANAGER: player " + i + " has lives: " + playerList[i].health.currentLives);
                if (playerList[i].health.currentLives <= 0)
                {
                    alivePlayers.Remove(playerList[i]);
                }
            }

            // update alive count
            aliveCount = alivePlayers.Count;

            // check if the round has ended
            CheckRoundEnd();
        }
    }

    [Server] // returns true if the round ended successfully, false otherwise
    public bool CheckRoundEnd()
    {
        // End the round if only one player alive
        if (aliveCount <= 1)
        {
            if (aliveCount == 0) EndRound(playerList[0].player);
            else EndRound(alivePlayers[0].player);
            return true;
        }

        return false;
    }

    // returns the player than won this round, null if there is a tie
    public Player GetWinnerPlayerByLives()
    {
        int minLife = -1;

        foreach (PlayerInfo pi in alivePlayers)
        {
            int l = pi.health.currentLives;

            // First life becomes the minLife
            if (minLife == -1) minLife = l;

            // If two players have same lives, then no winner
            if (l == minLife) { minLife = -1; break; }

            // If player life is new min, set that as minLife
            if (l < minLife) minLife = l;
        }

        if (minLife >= 0)
        {
            // There is a winner
            return alivePlayers.Find(e => e.health.currentLives == minLife).player;
        } else 
        {
            // There was a tie (we can check further win conditions here, ex. player with most combos)
            return null;
        }
    }
}
