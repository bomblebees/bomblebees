using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class RoundManager : NetworkBehaviour
{
    public class PlayerInfo
    {
        public Health health;
        public Player player;
    }

    // whether the round is over
    [SyncVar] public bool roundOver;

    // Lists
    public List<GameObject> playerList = new List<GameObject>();
    public List<WinCondition> winConditions = new List<WinCondition>();

    [SyncVar(hook = nameof(UpdateGridsAliveCount))]
    public int aliveCount;

    [SyncVar] public int currentGlobalInventorySize = 3;

    [Header("Server End Selection")] [SerializeField]
    private Canvas serverEndSelectionCanvas;

    private GlobalButtonSettings _globalButtonSettings;

    // potential to-do: separate stat tracker UI stuff into separate script away from round manager
    [Header("Stat Tracker")] [SerializeField]
    public GameObject[] statsElementUIAnchorObjects;

    [Header("Round Settings")] [SerializeField]
    public int startGameFreezeDuration = 5;

    [SerializeField] public int endGameFreezeDuration = 5;

    // events
    public delegate void GlobalInventorySizeChangeDelegate(int newSize);

    public event GlobalInventorySizeChangeDelegate EventInventorySizeChanged;

    // singletons
    private static RoundManager _instance;

    public static RoundManager Singleton
    {
        get { return _instance; }
    }

    // Required Variables
    private EventManager _eventManager;
    private NetworkRoomManagerExt _room;
    private HexGrid _hexGrid;
    private LobbySettings _settings;

    private void Awake()
    {
        if (_instance is null || _instance == this)
        {
            _instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of singleton: RoundManager");
        }

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
        _eventManager.EventPlayerLoaded += OnPlayerLoadedIntoRound;

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
    [Server]
    private void OnPlayerLoadedIntoRound(GameObject playerObject, int remaining)
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
    [Server]
    private void InitRequiredVars()
    {
        // Event manager singleton
        _eventManager = EventManager.Singleton;

        if (_eventManager is null)
        {
            Debug.LogError("Cannot find Singleton: EventManager");
        }
        else
        {
            _eventManager.EventPlayerEliminated += OnPlayerEliminated;
        }

        _room = NetworkManager.singleton as NetworkRoomManagerExt;
        _hexGrid = FindObjectOfType<HexGrid>();
        _settings = FindObjectOfType<LobbySettings>();
    }

    /// <summary>
    /// Initializes win conditions and etc.
    /// </summary>
    [Server]
    private void InitRoundManager()
    {
        if (_settings.practiceMode) startGameFreezeDuration = -1;

        // -- ALL WIN CONDITIONS INITIALIZED HERE -- //
        if (_settings.byLastAlive) winConditions.Add(gameObject.AddComponent<LivesCondition>());
        if (_settings.byTimerFinished) winConditions.Add(gameObject.AddComponent<TimerCondition>());
        if (_settings.GetGamemode() is TeamsGamemode) winConditions.Add(gameObject.AddComponent<TeamsCondition>());
        if (_settings.GetGamemode() is KillsGamemode)
            winConditions.Add(gameObject.AddComponent<EliminationCondition>());
        if (_settings.GetGamemode() is ComboGamemode) winConditions.Add(gameObject.AddComponent<ComboCondition>());
    }

    [Server]
    public IEnumerator ServerStartRound()
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
        _eventManager.OnStartRound();

        // Start all win conditions
        foreach (WinCondition c in winConditions)
        {
            c.StartWinCondition();
        }

        // Start round on client
        ClientStartRound();

        UpdateGridsAliveCount(0, aliveCount);
    }

    [ClientRpc]
    public void ClientStartRound()
    {
        // Start camera follow
        FindObjectOfType<CameraFollow>().InitCameraFollow();
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
        _eventManager.OnEndRound(players);

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
            (_settings.GetGamemode() is TeamsGamemode)
                ? "Team " + winningOrder[0].GetComponent<Player>().teamIndex + " won!"
                : winningOrder[0].GetComponent<Player>().steamName + " won!");

        Button[] buttonList = serverEndSelectionCanvas.GetComponentsInChildren<Button>();
        foreach (Button button in buttonList)
        {
            CanvasRenderer[] canvasRenderers = button.GetComponentsInChildren<CanvasRenderer>();

            button.interactable = true;
            button.gameObject.GetComponent<ButtonHoverTween>().enabled = true;
            _globalButtonSettings.ActivateButtonOpacity(canvasRenderers);
        }
    }

    [Server]
    private GameObject[] CollectWinningOrder()
    {
        // Retrieve the winning order from the current gamemode
        GameObject[] orderedArray = _settings.gamemode.GetWinningOrder(playerList.ToArray());

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
    [Server]
    private void OnWinConditionSatisfied()
    {
        if (_settings.endAfterFirstWinCondition)
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

    [Client]
    public void UpdateGridsAliveCount(int _, int newAliveCount)
    {
        if (_hexGrid) _hexGrid.SetAliveCount(newAliveCount);
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

    [ClientRpc]
    private void RpcPrintResultsAndShowEndCard(GameObject[] winningOrder, string winText)
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
        _eventManager.OnReturnToLobby(); // invoke event

        if (NetworkManager.singleton is NetworkRoomManagerExt room) room.ServerChangeScene(room.RoomScene);
    }

    [Server]
    public void ChooseRematch()
    {
        RpcShowLoadingScreen();
        serverEndSelectionCanvas.enabled = true;

        if (NetworkManager.singleton is NetworkRoomManagerExt room)
        {
            room.ServerChangeScene(room.RoomScene);
            room.ServerChangeScene(room.GameplayScene);
        }
    }

    // Move all cheats to DebugCheats.cs

    #endregion
}