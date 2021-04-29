using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameUIManager : NetworkBehaviour
{
    // Game UIs
    [SerializeField] private RoundStartEnd roundStartEnd = null;
    [SerializeField] private RoundTimer roundTimer = null;
    [SerializeField] private LivesUI livesUI = null;
    [SerializeField] private MessageFeed messageFeed = null;
    [SerializeField] private MessageFeed warningFeed = null;
    [SerializeField] public Hotbar hotbar = null;

    private GameObject localPlayer;


    // singletons
    public static GameUIManager _instance;
    public static GameUIManager Singleton { get { return _instance; } }

    private RoundManager roundManager;
    private EventManager eventManager;

    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: GameUIManager");
        else _instance = this;
    }

    private void InitSingletons()
    {
        roundManager = RoundManager.Singleton;
        if (roundManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");
    }

    public override void OnStartServer()
    {
        InitSingletons();

        // Subscribe to server round events
        roundManager.EventPlayerConnected += ServerPlayerConnected;
        roundManager.EventRoundStart += ServerStartRound;

        eventManager.EventPlayerTookDamage += RpcOnKillEvent;
        eventManager.EventPlayerSwap += ServerOnSwapEvent;
        eventManager.EventPlayerSpin += ServerOnSpinEvent;
    }

    [Client]
    public override void OnStartClient()
    {
        InitSingletons();

        // Subscribe to client round events
        roundManager.EventRoundStart += ClientStartRound;
        roundManager.EventRoundEnd += ClientEndRound;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        localPlayer = GameObject.Find("LocalPlayer");
    }

    //private void Update()
    //{
    //    if (localPlayer == null)
    //    {
    //        localPlayer = GameObject.Find("LocalPlayer");
            
    //        if (localPlayer != null) localPlayer.GetComponent<Health>().EventLivesChanged += ClientOnDamage;
    //    }
        
    //}

    // When a player loads into the game (on server)
    [Server] public void ServerPlayerConnected(RoundManager.PlayerInfo p)
    {
        RpcPlayerConnected(roundManager.playersConnected, roundManager.totalRoomPlayers);
    }

    // When a player loads into the game (on client)
    [ClientRpc] public void RpcPlayerConnected(int connPlayers, int totalPlayers)
    {
        roundStartEnd.UpdateRoundWaitUI(connPlayers, totalPlayers);
    }

    #region Round Start & End

    [Client] public void ClientStartRound()
    {
        StartCoroutine(roundStartEnd.StartRoundFreezetime(roundManager.startGameFreezeDuration));
        StartCoroutine(roundTimer.InitTimer(roundManager.roundDuration, roundManager.startGameFreezeDuration));

    }
    [Client] public void ClientEndRound(GameObject winner)
    {
        StartCoroutine(roundStartEnd.EndRoundFreezetime(roundManager.startGameFreezeDuration, winner));
    }

    #endregion

    #region Lives

    [Server] public void ServerStartRound() { ServerEnableLivesUI(); }

    [Server]
    public void ServerEnableLivesUI()
    {
        // Convert playerinfo struct to network transportable gameObjects
        List<RoundManager.PlayerInfo> playerList = roundManager.playerList;

        GameObject[] playerObjects = new GameObject[playerList.Count];

        for (int i = 0; i < playerList.Count; i++)
        {
            playerObjects[i] = playerList[i].player.gameObject;

            // Subscribe to lives change event for specific player
            playerList[i].health.EventLivesChanged += ServerUpdateLives;
        }

        RpcEnableLivesUI(playerObjects);
    }

    [ClientRpc] public void RpcEnableLivesUI(GameObject[] players)
    {
        livesUI.EnableLivesUI(players);
    }

    // technical debt: having reference to individual player whose live changed is better
    [Server]
    public void ServerUpdateLives(int currentHealth, int _, GameObject player)
    {
        // Convert playerinfo struct to network transportable gameObjects
        List<RoundManager.PlayerInfo> playerList = roundManager.playerList;

        GameObject[] playerObjects = new GameObject[playerList.Count];

        for (int i = 0; i < playerList.Count; i++)
        {
            playerObjects[i] = playerList[i].player.gameObject;
        }

        RpcClientUpdateLives(playerObjects);
    }

    [ClientRpc]
    public void RpcClientUpdateLives(GameObject[] players)
    {
        livesUI.UpdateLives(players);
    }

    #endregion

    #region MessageFeed

    [ClientRpc]
    public void RpcOnKillEvent(int _, GameObject bomb, GameObject player)
    {
        messageFeed.OnKillEvent(bomb, player);
    }

    [Server]
    public void ServerOnSwapEvent(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
    {
        if (combo)
        {
            RpcOnSwapComboEvent(oldKey, player);
        }

        TargetOnSwapEvent(player.GetComponent<NetworkIdentity>().connectionToClient, newKey);
    }

    [ClientRpc]
    public void RpcOnSwapComboEvent(char comboKey, GameObject player)
    {
        messageFeed.OnSwapEvent(comboKey, player);

    }

    #endregion

    #region Hotbar

    [Server]
    public void ServerOnSpinEvent(GameObject player, GameObject bomb)
    {
        TargetOnSpinEvent(player.GetComponent<NetworkIdentity>().connectionToClient);
    }

    [TargetRpc]
    public void TargetOnSpinEvent(NetworkConnection target)
    {
        hotbar.StartSpinCooldown();
    }

    [TargetRpc]
    public void TargetOnSwapEvent(NetworkConnection target, char newKey)
    {
        hotbar.SwapHexes(newKey);
    }

    [Client]
    public void ClientOnInventorySelectChanged(char key, int amt)
    {
        hotbar.UpdateInventoryUI(key, amt);
    }

    #endregion

    [Client] public void ClientCreateWarningMessage(string message)
    {
        warningFeed.CreateMessage(message);
    }

    [Client] public void ClientOnDamage(int currentHealth, int maxHealth, GameObject player)
    {
        if (currentHealth == 0)
        {
            string errorMessage = "<color=#FF0000>You Died!</color>";
            ClientCreateWarningMessage(errorMessage);
        } else if (player == localPlayer)
        {
            string errorMessage = "<color=#FF0000>-1   Life</color>";
            ClientCreateWarningMessage(errorMessage);
        }
    }
}

