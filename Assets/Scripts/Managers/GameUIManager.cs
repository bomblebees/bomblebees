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
    private LobbySettings lobbySettings;

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

        lobbySettings = FindObjectOfType<LobbySettings>();
        if (lobbySettings == null) Debug.LogError("Cannot find Singleton: LobbySettings");
    }

    public override void OnStartServer()
    {
        InitSingletons();

        // Subscribe to relevant events
        eventManager.EventPlayerLoaded += ServerPlayerConnected;
        eventManager.EventStartRound += ServerStartRound;
        eventManager.EventEndRound += ServerEndRound;

        eventManager.EventPlayerTookDamage += RpcOnKillEvent;
		eventManager.EventMultikill += RpcOnMultikillEvent;
        // eventManager.EventPlayerSwap += ServerOnSwapEvent;
        eventManager.EventPlayerSpin += ServerOnSpinEvent;
    }

    [Client]
    public override void OnStartClient()
    {
        InitSingletons();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        localPlayer = GameObject.Find("LocalPlayer");
    }

    // When a player loads into the game (on server)
    [Server] public void ServerPlayerConnected(GameObject player, int remaining)
    {
        Debug.Log("player connected: " + player.GetComponent<Player>().playerRoomIndex);

        //ServerEnableLivesUI(player);
        RpcPlayerConnected(eventManager.playersLoaded, eventManager.totalPlayers);
    }

    // When a player loads into the game (on client)
    [ClientRpc] public void RpcPlayerConnected(int connPlayers, int totalPlayers)
    {
        roundTimer.InitTimer(lobbySettings.roundDuration);
        roundStartEnd.UpdateRoundWaitUI(connPlayers, totalPlayers);

        // If all players loaded, start freeze time
        if (totalPlayers - connPlayers == 0)
        {
            StartCoroutine(roundStartEnd.StartRoundFreezetime(roundManager.startGameFreezeDuration));
        }
    }

    #region Round Start & End

    [Server] public void ServerStartRound()
    {
        ClientStartRound();
    }

    [Server] public void ServerEndRound(List<Player> players)
    {
        ClientEndRound();
    }

    [ClientRpc] public void ClientStartRound()
    {
        roundTimer.StartTimer(lobbySettings.roundDuration);
    }

    [ClientRpc] public void ClientEndRound()
    {
        StartCoroutine(roundStartEnd.EndRoundFreezetime(roundManager.endGameFreezeDuration));
    }

    #endregion

    #region Lives

    [Server]
    public void ServerEnableLivesUI(GameObject player)
    {
        player.GetComponent<Health>().EventLivesChanged += RpcClientUpdateLives;
    }

    [ClientRpc] public void RpcClientUpdateLives(int currentHealth, int _, GameObject player)
    {
        livesUI.UpdateLives(currentHealth, player.GetComponent<Player>());
    }

    #endregion

    #region MessageFeed

    [ClientRpc]
    public void RpcOnKillEvent(int newLives, GameObject bomb, GameObject player)
    {
        messageFeed.OnKillEvent(bomb, player);
        livesUI.UpdateLives(newLives, player.GetComponent<Player>());
    }

	[ClientRpc]
	public void RpcOnMultikillEvent(GameObject player, int multiKillAmount)
	{
		messageFeed.OnMultikillEvent(player, multiKillAmount);
	}

	[Server]
    public void ServerOnSwapEvent(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
    {
        if (combo)
        {
            RpcOnSwapComboEvent(oldKey, player, numBombsAwarded);
        }

        TargetOnSwapEvent(player.GetComponent<NetworkIdentity>().connectionToClient, newKey);
    }

    [ClientRpc]
    public void RpcOnSwapComboEvent(char comboKey, GameObject player, int numBombsAwarded)
    {
        messageFeed.OnSwapEvent(comboKey, player, numBombsAwarded);

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

