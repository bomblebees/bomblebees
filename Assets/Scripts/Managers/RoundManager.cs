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
        public double timeOfElim;
    }

    // whether the round is over
	private bool roundOver = false;

    // Lists
    public List<PlayerInfo> playerList = new List<PlayerInfo>();
    public List<WinCondition> winConditions = new List<WinCondition>();

    // order of elimination so we can print out the placements in results screen in right order
    public SyncList<GameObject> orderedPlayerList = new SyncList<GameObject>();

    [SyncVar(hook = nameof(UpdateGridsAliveCount))]
    public int aliveCount;

    [SyncVar] public int currentGlobalInventorySize = 3;

    [Header("Server End Selection")]
	[SerializeField] private Canvas serverEndSelectionCanvas;
    private GlobalButtonSettings _globalButtonSettings;

	// potential to-do: separate stat tracker UI stuff into separate script away from round manager
	[Header("Stat Tracker")]
	[SerializeField] public GameObject[] statsElementUIAnchorObjects;

	[Header("Round Settings")]
    [SerializeField] public int startGameFreezeDuration = 5;
    [SerializeField] public int endGameFreezeDuration = 5;
    
    // events
	public delegate void GlobalInventorySizeChangeDelegate(int newSize);
	public event GlobalInventorySizeChangeDelegate EventInventorySizeChanged;

    // singletons
    public static RoundManager _instance;
    public static RoundManager Singleton { get { return _instance;  } }

    // Required Variables
    private EventManager eventManager;
	private NetworkRoomManagerExt room;
    private HexGrid hexGrid;
    private LobbySettings settings;


    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: RoundManager");
        else _instance = this;

        // Setup buttons
        _globalButtonSettings = FindObjectOfType<GlobalButtonSettings>();
        Button[] buttons = serverEndSelectionCanvas.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
	        button.interactable = false;
	        button.gameObject.GetComponent<ButtonHoverTween>().enabled = false;
	        CanvasRenderer[] canvasRenderers = button.GetComponentsInChildren<CanvasRenderer>();
	        _globalButtonSettings.DeactivateButtonOpacity(canvasRenderers);
        }
    }


	#region Setup

	public override void OnStartServer()
    {
        InitRequiredVars();

        // Subscribe to relevant events
        eventManager.EventPlayerLoaded += OnPlayerLoadedIntoRound;

        int numPlayers = FindObjectsOfType<NetworkRoomPlayerExt>().Length;

		if (numPlayers > 1)
		{
			currentGlobalInventorySize = (numPlayers - 7) * -1;
		}
		else
		{
			currentGlobalInventorySize = 5;
		}

        InitRoundManager();
    }



    /// <summary>
    /// Called when the player finishes loading into the scene.
    /// </summary>
    [Server] public void OnPlayerLoadedIntoRound(GameObject playerObject, int remaining)
    {
        // Add player to list
        Player player = playerObject.GetComponent<Player>();
        Health live = playerObject.gameObject.GetComponent<Health>();

        // Init player info struct
        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.player = player;
        playerInfo.health = live;
        playerInfo.timeOfElim = 0;

        // Add the player to list of all players
        playerList.Add(playerInfo);

        // if no more players waiting to load, start the round
        if (remaining == 0) StartCoroutine(ServerStartRound());
    }

    #endregion

    /// <summary>
    /// Initializes the required variables for the round manager
    /// </summary>
    [Server] private void InitRequiredVars()
    {
        // Event manager singleton
        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");

        eventManager.EventPlayerEliminated += OnPlayerEliminated;

        room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        hexGrid = FindObjectOfType<HexGrid>();
        settings = FindObjectOfType<LobbySettings>();
    }

    /// <summary>
    /// Initializes win conditions and etc.
    /// </summary>
    [Server] private void InitRoundManager()
    {
        // -- ALL WIN CONDITIONS INITALIZED HERE -- //
        if (settings.byLastAlive) winConditions.Add(this.gameObject.AddComponent<LivesCondition>());
        if (settings.byTimerFinished) winConditions.Add(this.gameObject.AddComponent<TimerCondition>());
    }

    [Server]
    public IEnumerator ServerStartRound()
    {
        // -- Pre-game start behaviours can go here -- //

        // Set alive count to number of players
        aliveCount = playerList.Count;

        // Invoke start round event
        eventManager.OnStartRound();

        // Initialize and subscribe to all win conditions
        foreach (WinCondition c in winConditions)
        {
            c.InitWinCondition();
            c.EventWinConditionSatisfied += OnWinConditionSatisfied;
        }

        // Wait for freeze duration
        yield return new WaitForSeconds(startGameFreezeDuration + 1);

        // -- Game Starts Here -- //

        // Start all win conditions
        foreach (WinCondition c in winConditions)
        {
            c.StartWinCondition();
        }


        UpdateGridsAliveCount(0, aliveCount);
    }

    [Server]
    public IEnumerator ServerEndRound()
    {
        // -- Pre-game end behaviours can go here -- //

        // Round is over
        roundOver = true;

        // Collect the winning order of the players
        CollectWinningOrder();

        // Invoke end round event
        List<Player> players = new List<Player>();
        playerList.ForEach(pi => { players.Add(pi.player); });
        eventManager.OnEndRound(players);

        // Stop and unsubscribe from all win conditions
        foreach (WinCondition c in winConditions)
        {
            c.StopWinCondition();
            c.EventWinConditionSatisfied -= OnWinConditionSatisfied;
        }

        // Wait for freeze duration
        yield return new WaitForSeconds(endGameFreezeDuration);

        // -- Game Ends Here -- //

        RpcPrintResultsAndShowEndCard();

        Button[] buttonList = serverEndSelectionCanvas.GetComponentsInChildren<Button>();
        foreach (Button button in buttonList)
        {
            CanvasRenderer[] canvasRenderers = button.GetComponentsInChildren<CanvasRenderer>();

            button.interactable = true;
            button.gameObject.GetComponent<ButtonHoverTween>().enabled = true;
            _globalButtonSettings.ActivateButtonOpacity(canvasRenderers);
        }
    }

    [Server] private void CollectWinningOrder()
    {
        Debug.Log("-- Collecting Winning Order --");
        foreach (PlayerInfo p in playerList)
        {
            Debug.Log("Player " + p.player.playerRoomIndex + " died at " + p.timeOfElim);
        }

        playerList.Sort((p1, p2) =>
        {
            // -1 = p1 before p2
            // 0 = p1 same as p2
            // 1 = p1 after p2

            return 0;
        });

        //if (winner != null)
        //{
        //    for (int i = 1; i <= playerMaxLives; i++)
        //    {
        //        for (int j = 0; j < alivePlayers.Count; j++)
        //        {
        //            if (alivePlayers[j].health.currentLives == i)
        //            {
        //                // If two players tie in health, arbitrary rank order for now; how would we sort this by whoever has more kills or something? :/ 
        //                orderedPlayerList.Insert(0, alivePlayers[j].player.gameObject);
        //            }
        //        }
        //    }
        //}
        //else // there is no winner, so tie match
        //{
        //    if (alivePlayers.Count < 1)
        //    {
        //        // no winner but none alive either, single player game, player was already inserted into ordered list
        //        Debug.Log("single player game");
        //        // orderedPlayerList.Insert(0, playerList[0].player.gameObject);
        //    }
        //    else
        //    {
        //        Debug.Log("tie with more than one remaining");
        //        // add the rest of the alive players to eliminated players list in order of health (still arbitrary)
        //        for (int i = 1; i <= playerMaxLives; i++)
        //        {
        //            for (int j = 0; j < alivePlayers.Count; j++)
        //            {
        //                if (alivePlayers[j].health.currentLives == i)
        //                {
        //                    orderedPlayerList.Insert(0, alivePlayers[j].player.gameObject);
        //                }
        //            }
        //        }
        //    }
        //}
    }

    #region Subscriptions

    /// <summary>
    /// Called whenever a win condition is satisfied
    /// </summary>
    /// <param name="cond">The win condition that was satisfied</param>
    [Server] public void OnWinConditionSatisfied()
    {
        if (settings.endAfterFirstWinCondition)
        {
            // End the round right after a win condition is satisfied
            StartCoroutine(ServerEndRound());
        }
        else
        {
            bool notSatisfied = true;

            // Check if all conditions are satisfied
            foreach (WinCondition c in winConditions)
            {
                notSatisfied &= c.CheckWinCondition();
            }

            // If they are, end the round
            if (!notSatisfied) StartCoroutine(ServerEndRound());
        }
    }

    [Server]
    public void OnPlayerEliminated(double timeOfElim, GameObject player)
    {
        // Update alive count
        aliveCount--;

        // Set the time of elimination for this player
        playerList.Find(p => p.player.gameObject == player).timeOfElim = timeOfElim;


        //Debug.Log("inserting dead player to ordered elimination list");
        //orderedPlayerList.Insert(0, player);



        IncreaseGlobalLivesAmt();
    }

    [Client] public void UpdateGridsAliveCount(int _, int newAliveCount)
    {
        if (hexGrid) hexGrid.SetAliveCount(newAliveCount);
    }

    #endregion

    [Server]
    public void IncreaseGlobalLivesAmt()
    {
        if (aliveCount > 1)
        {
            // if there are still more than 1 person left after this death, increase global inv size
            currentGlobalInventorySize++;
            EventInventorySizeChanged?.Invoke(currentGlobalInventorySize);
        }
    }

    //// returns the player than won this round, null if there is a tie
    //public Player GetWinnerPlayerByLives()
    //{
    //    int maxLife = -1;

    //    foreach (PlayerInfo pi in alivePlayers)
    //    {
    //        int l = pi.health.currentLives;

    //        // If any two players have same lives, then no winner
    //        if (l == maxLife) { maxLife = -1; break; }

    //        // First life becomes the minLife
    //        if (maxLife == -1) maxLife = l;

    //        // If player life is new max, set that as maxLife
    //        if (l > maxLife) maxLife = l;
    //    }

    //    if (maxLife >= 0)
    //    {
    //        // There is a winner
    //        return alivePlayers.Find(e => e.health.currentLives == maxLife).player;
    //    } else 
    //    {
    //        // There was a tie (we can check further win conditions here, ex. player with most combos)
    //        return null;
    //    }
    //}

    #region End Game Results

    [ClientRpc]
    private void RpcPrintResultsAndShowEndCard()
    {
        serverEndSelectionCanvas.enabled = true;

        for (int i = 0; i < orderedPlayerList.Count; i++)
        {
            orderedPlayerList[i].GetComponent<PlayerStatTracker>().placement = i + 1;
            orderedPlayerList[i].GetComponent<PlayerStatTracker>().PrintStats();
            orderedPlayerList[i].GetComponent<PlayerStatTracker>().CreateStatsUIElement(statsElementUIAnchorObjects[i]);
        }
    }
    
    [ClientRpc]
    private void RpcShowLoadingScreen()
    {
        FindObjectOfType<GlobalLoadingScreen>().gameObject.GetComponent<Canvas>().enabled = true;
    }

    #endregion

    #region Cheats

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

    private void Update()
    {
        // TODO: Delete later
        // Cheats
        if (Input.GetKeyDown(KeyCode.Alpha5) && Input.GetKey(KeyCode.C))
        {
            ChooseRematch();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha6) && Input.GetKey(KeyCode.C))
        {
            ChooseReturnToLobby();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7) && Input.GetKey(KeyCode.C))
        {
            StartCoroutine(ServerEndRound());
        }

		// cheat for increasing inv size
		if (Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.B))
		{
			IncreaseGlobalLivesAmt();
		}
	}

    #endregion
}
