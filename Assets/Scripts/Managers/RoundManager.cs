﻿using System.Collections;
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

	[SerializeField] private Canvas serverEndSelectionCanvas;

	// potential to-do: separate stat tracker UI stuff into separate script away from roundmanager
	[Header("Stat Tracker")]
	[SerializeField] public PlayerStatTracker statTracker;
	[SerializeField] public GameObject statsElementUIAnchorObject;

	[Header("Round Settings")]
    [SerializeField] public int startGameFreezeDuration = 5;
    [SerializeField] public int endGameFreezeDuration = 5;
	[SerializeField] public int playerMaxLives = 3;

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
	public delegate void PlayerEliminatedDelegate(GameObject eliminatedPlayer);
    public event PlayerConnectedDelegate EventPlayerConnected;
    public event RoundStartDelegate EventRoundStart;
    public event RoundEndDelegate EventRoundEnd;
	public event PlayerEliminatedDelegate EventPlayerEliminated;

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

		// add player to stat tracker list and store reference to the player gameobject
		statTracker.CreatePlayerStatObject(sender.identity.gameObject);

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
        // wait for round to end, - 1 second for start time delay
        yield return new WaitForSeconds(roundDuration - 1);

        if (!CheckRoundEnd())
        {

            // Player with lowest lives win
            Player winner = GetWinnerPlayerByLives();

            //Debug.Log("winner: " + winner.gameObject.transform.name);

            if (winner != null)
            {
                EndRound(winner.gameObject);
            } else
            {
                // Else, some other way to determine winner here
                // EndRound() ends the round as a tie
                EndRound();
            }
        }

    }

    [Server]
    public void EndRound(GameObject winner = null)
    {


		

		

		

		// Append all player objects to player list for event manager
		List<Player> players = new List<Player>();
        playerList.ForEach(pi => 
		{
			players.Add(pi.player);
		});

        eventManager.OnEndRound(players);
        RpcEndRound(winner);
        StartCoroutine(ServerEndRound(winner));
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

	// Call this event when player gets eliminated
	[ClientRpc]
	public void RpcPlayerEliminated(GameObject eliminatedPlayer)
	{
		EventPlayerEliminated?.Invoke(eliminatedPlayer);
	}

    [Server]
    public IEnumerator ServerStartRound()
    {
        yield return new WaitForSeconds(startGameFreezeDuration + 1);
        for (int i = 0; i < playerList.Count; i++)
        {
            alivePlayers.Add(playerList[i]);
            Player p = playerList[i].player;
			p.playerListIndex = i;
            //p.SetCanPlaceBombs(true);
            //p.SetCanSpin(true);
            //p.SetCanSwap(true); 
            //p.SetCanMove(true);
        }
        aliveCount = alivePlayers.Count;
        UpdateGridsAliveCount(0, aliveCount);
        if (useRoundTimer) StartCoroutine(StartRoundTimer());
    }

    [Server]
    public IEnumerator ServerEndRound(GameObject winner = null)
    {

		yield return new WaitForSeconds(endGameFreezeDuration);

		if (winner != null)
		{
			/*
			PlayerStatTracker.PlayerStats winningPlayerStat = statTracker.playerStatsList[statTracker.getPlayerIndexInList(alivePlayers[0].player.gameObject)];
			winningPlayerStat.placement = 1;
			statTracker.playerStatsOrderedByElimination.Insert(0, winningPlayerStat);
			*/

			// if we have a winner, but we also have other survivors
			if (alivePlayers.Count > 1)
			{
				// add to ordered stats list in order of less lives to max
				for (int i = 1; i <= playerMaxLives; i++)
				{
					for (int j = 0; j < alivePlayers.Count; j++)
					{
						if (alivePlayers[j].player.GetComponent<Health>().currentLives == i)
						{
							PlayerStatTracker.PlayerStats currentPlayer = statTracker.playerStatsList[statTracker.getPlayerIndexInList(alivePlayers[j].player.gameObject)];
							currentPlayer.placement = playerList.Count - statTracker.playerStatsOrderedByElimination.Count;
							statTracker.playerStatsOrderedByElimination.Insert(0, currentPlayer);
						}
					}
				}
			}
			else // we have just one winner
			{
				// player is in index 0, last one standing
				/*
				PlayerStatTracker.PlayerStats currentPlayer = statTracker.playerStatsList[statTracker.getPlayerIndexInList(alivePlayers[0].player.gameObject)];
				currentPlayer.placement = 1;
				statTracker.playerStatsOrderedByElimination.Insert(0, currentPlayer);
				*/
			}
		}
		else
		{
			Debug.Log("no winners, winner is null");
			// tie
			if (alivePlayers.Count == 0)
			{
				// round ended before time but last one standing was also person to die; single player game
				Debug.Log("single player game");
				PlayerStatTracker.PlayerStats currentPlayer = statTracker.playerStatsList[statTracker.getPlayerIndexInList(playerList[0].player.gameObject)];
				currentPlayer.placement = 1;
				statTracker.playerStatsOrderedByElimination.Insert(0, statTracker.playerStatsList[statTracker.getPlayerIndexInList(playerList[0].player.gameObject)]);

			}
			else // no winner, round ended by time 
			{
				for (int i = 0; i < alivePlayers.Count; i++)
				{
					// go through the set of player stats and add to the ordered list in order of kills, then combos made
					PlayerStatTracker.PlayerStats currentPlayer = statTracker.playerStatsList[statTracker.getPlayerIndexInList(alivePlayers[i].player.gameObject)];
					statTracker.playerStatsOrderedByElimination.Insert(0, currentPlayer);
				}
			}
		}
		RpcShowEndCard();

		// printing game results in console

		statTracker.PrintStats();
		
		

		Button[] buttonList = serverEndSelectionCanvas.GetComponentsInChildren<Button>();
        foreach (Button button in buttonList)
        {
            button.interactable = true;
            button.gameObject.GetComponent<ButtonHoverTween>().enabled = true;
        }
    }


    [ClientRpc]
    private void RpcShowEndCard()
    {
        serverEndSelectionCanvas.enabled = true;
    }
    
    [ClientRpc]
    private void RpcShowLoadingScreen()
    {
        FindObjectOfType<GlobalLoadingScreen>().gameObject.GetComponent<Canvas>().enabled = true;
    }

    [Server]
    public void ChooseReturnToLobby()
    {
        serverEndSelectionCanvas.enabled = false;
        eventManager.OnReturnToLobby(); // invoke event

        NetworkRoomManagerExt room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        room.ServerChangeScene(room.RoomScene);
    }

    [Server]
    public void ChooseRematch()
    {
        RpcShowLoadingScreen();
        serverEndSelectionCanvas.enabled = true;
        NetworkRoomManagerExt room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        room.ServerChangeScene(room.RoomScene);
        room.ServerChangeScene(room.GameplayScene);
    }

    [Server]
    public void OnLivesChanged(int currentHealth, int _, GameObject player)
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

			Debug.Log(player.name);

			// check if the round has ended
			CheckRoundEnd();
			EventPlayerEliminated?.Invoke(player);
			
        }
    }

    [Server] // returns true if the round ended successfully, false otherwise
    public bool CheckRoundEnd()
    {
        // End the round if only one player alive
        if (aliveCount <= 1)
        {
            if (aliveCount == 0) EndRound();
            else EndRound(alivePlayers[0].player.gameObject);
            return true;
        }

        return false;
    }

    // returns the player than won this round, null if there is a tie
    public Player GetWinnerPlayerByLives()
    {
        int maxLife = -1;

        foreach (PlayerInfo pi in alivePlayers)
        {
            int l = pi.health.currentLives;

            // If any two players have same lives, then no winner
            if (l == maxLife) { maxLife = -1; break; }

            // First life becomes the minLife
            if (maxLife == -1) maxLife = l;

            // If player life is new max, set that as maxLife
            if (l > maxLife) maxLife = l;
        }

        if (maxLife >= 0)
        {
            // There is a winner
            return alivePlayers.Find(e => e.health.currentLives == maxLife).player;
        } else 
        {
            // There was a tie (we can check further win conditions here, ex. player with most combos)
            return null;
        }
    }

    private void Update()
    {
        // TODO: Delete later
        if (Input.GetKeyDown(KeyCode.Alpha5) && Input.GetKey(KeyCode.C))
        {
            ChooseRematch();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha6) && Input.GetKey(KeyCode.C))
        {
            ChooseReturnToLobby();
        }
    }
}
