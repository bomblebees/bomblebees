﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;
using Mirror;
using Steamworks;
using System.Linq;

public class RoundManager : NetworkBehaviour
{
    public class PlayerInfo
    {
        public Health health;
        public Player player;
    }

    // whether the round is over
    [SyncVar] public bool roundOver = false;

    // Lists
    public List<GameObject> playerList = new List<GameObject>();
    public List<WinCondition> winConditions = new List<WinCondition>();

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
        // Add the player to list of all players
        playerList.Add(playerObject);

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
        if (settings.practiceMode) startGameFreezeDuration = -1;

        // -- ALL WIN CONDITIONS INITALIZED HERE -- //
        if (settings.byLastAlive) winConditions.Add(this.gameObject.AddComponent<LivesCondition>());
        if (settings.byTimerFinished) winConditions.Add(this.gameObject.AddComponent<TimerCondition>());
        if (settings.GetGamemode() is TeamsGamemode) winConditions.Add(this.gameObject.AddComponent<TeamsCondition>());
        if (settings.GetGamemode() is KillsGamemode) winConditions.Add(this.gameObject.AddComponent<EliminationCondition>());
        if (settings.GetGamemode() is ComboGamemode) winConditions.Add(this.gameObject.AddComponent<ComboCondition>());
    }

    [Server] public IEnumerator ServerStartRound()
    {
        // -- Pre-game start behaviours can go here -- //

        // Set alive count to number of players
        aliveCount = playerList.Count;

        // Initialize and subscribe to all win conditions
        foreach (WinCondition c in winConditions)
        {
            c.InitWinCondition();
            c.EventWinConditionSatisfied += OnWinConditionSatisfied;
        }

        // Wait for freeze duration
        yield return new WaitForSeconds(startGameFreezeDuration + 1);

        // -- Game Starts Here -- //

        // Unfreeze all players
        foreach (GameObject p in playerList)
        {
            p.GetComponent<Player>().isFrozen = false;
        }

        // Invoke start round event
        eventManager.OnStartRound();

        // Start all win conditions
        foreach (WinCondition c in winConditions)
        {
            c.StartWinCondition();
        }

        // Start round on client
        ClientStartRound();

        UpdateGridsAliveCount(0, aliveCount);
    }

    [ClientRpc] public void ClientStartRound()
    {
        if (!settings.practiceMode)
        {
            // Start camera follow
            FindObjectOfType<CameraFollow>().InitCameraFollow();
        }
    }

    [Server]
    public IEnumerator ServerEndRound()
    {
        // -- Pre-game end behaviours can go here -- //

        // Round is over
        roundOver = true;

        // Collect the winning order of the players
        GameObject[] winningOrder = CollectWinningOrder();

        // Invoke end round event
        List<Player> players = new List<Player>();
        playerList.ForEach(pi => { players.Add(pi.GetComponent<Player>()); });
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

        RpcPrintResultsAndShowEndCard(winningOrder,
            (settings.GetGamemode() is TeamsGamemode) ?
              "Team " + winningOrder[0].GetComponent<Player>().teamIndex + " won!":
               winningOrder[0].GetComponent<Player>().steamName + " won!");

        Button[] buttonList = serverEndSelectionCanvas.GetComponentsInChildren<Button>();
        foreach (Button button in buttonList)
        {
            CanvasRenderer[] canvasRenderers = button.GetComponentsInChildren<CanvasRenderer>();

            button.interactable = true;
            button.gameObject.GetComponent<ButtonHoverTween>().enabled = true;
            _globalButtonSettings.ActivateButtonOpacity(canvasRenderers);
        }
    }

    [Server] private GameObject[] CollectWinningOrder()
    {
        // Retrieve the winning order from the current gamemode
        GameObject[] orderedArray = settings.gamemode.GetWinningOrder(playerList.ToArray());

        // Debugging the array
        for (int i = 0; i < orderedArray.Length; i++)
        {
            Player player = orderedArray[i].GetComponent<Player>();
            Health health = orderedArray[i].GetComponent<Health>();

            Debug.Log("Pos " + i + " | Player " + player.playerRoomIndex + " with lives " + health.currentLives);
        }

        // Return the array
        return orderedArray;
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

    #region End Game Results

    [ClientRpc] private void RpcPrintResultsAndShowEndCard(GameObject[] winningOrder, string winText)
    {
        serverEndSelectionCanvas.enabled = true;

        for (int i = 0; i < winningOrder.Length; i++)
        {
            PlayerStatTracker stat = winningOrder[i].GetComponent<PlayerStatTracker>();

            stat.placement = i + 1;
            stat.PrintStats();
            stat.CreateStatsUIElement(statsElementUIAnchorObjects[i]);
        }



        FindObjectOfType<ServerEndSelectionTitle>().GetComponent<TMP_Text>().SetText(winText);

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
