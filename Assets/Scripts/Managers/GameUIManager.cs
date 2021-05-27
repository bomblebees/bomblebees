using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameUIManager : NetworkBehaviour
{
    // Game UIs
    [SerializeField] public RoundStartEnd roundStartEnd;
    [SerializeField] public RoundTimer roundTimer;
    [SerializeField] public LivesUI livesUI;
    [SerializeField] public MessageFeed messageFeed;
    [SerializeField] public MessageFeed warningFeed;
    [SerializeField] public Hotbar hotbar;
    [SerializeField] public Announcer announcer;

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
        RpcPlayerConnected(player, eventManager.playersLoaded, eventManager.totalPlayers);
    }

    // When a player loads into the game (on client)
    [ClientRpc] public void RpcPlayerConnected(GameObject player, int connPlayers, int totalPlayers)
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

    #region MessageFeed

    [ClientRpc]
    public void RpcOnKillEvent(int newLives, GameObject bomb, GameObject player)
    {
        messageFeed.OnKillEvent(bomb, player);
    }

	[ClientRpc]
	public void RpcOnMultikillEvent(GameObject player, int multiKillAmount)
	{
		messageFeed.OnMultikillEvent(player, multiKillAmount);
	}

    #endregion

    #region Client Events

    [Client]
    public void OnInventorySelectChanged(char key, int amt)
    {
        hotbar.UpdateInventoryUI(key, amt);
    }

    [Client] public void OnChangeLives(int prevLives, int newLives, GameObject player)
    {
        if (lobbySettings.GetGamemode() is StandardGamemode ||
            lobbySettings.GetGamemode() is TeamsGamemode)
        {
            livesUI.UpdateLives(newLives, player.GetComponent<Player>());
            livesUI.UpdateOrdering();
        }


        if (player.transform.name == "LocalPlayer" && lobbySettings.GetGamemode() is StandardGamemode)
        {
            if (newLives == 0)
            {
                string errorMessage = "<color=#FF0000>You Died!</color>";
                ClientCreateWarningMessage(errorMessage);
            }
            else
            {
                string errorMessage = "<color=#FF0000>-1   Life</color>";
                ClientCreateWarningMessage(errorMessage);
            }
        }
    }

    [Client] public void OnChangeKills(int prevKills, int newKills, GameObject player)
    {
        if (lobbySettings.GetGamemode() is KillsGamemode)
        {
            livesUI.UpdateEliminations(newKills, player.GetComponent<Player>());
            livesUI.UpdateOrdering();
        }
    }

    [Client]
    public void OnSpin(bool prevHeld, bool newHeld, GameObject player)
    {
        //hotbar.StartSpinCooldown();
    }

    [Client]
    public void OnSwap(char oldSwapKey, char newSwapKey, GameObject player)
    {
        //hotbar.SwapHexes(newSwapKey);
    }

    [Client]
    public void OnChangeCombos(int prevCombos, int newCombos, GameObject player)
    {
        if (lobbySettings.GetGamemode() is ComboGamemode)
        {
            livesUI.UpdateCombos(newCombos, player.GetComponent<Player>());
            livesUI.UpdateOrdering();
        }
    }

    #endregion

    [Client]
    public void ClientCreateWarningMessage(string message)
    {
        warningFeed.CreateMessage(message);
    }

    public void Announce(string message)
    {
        if (isServer) RpcAnnounce(message);
        else CmdAnnounce(message);
    }

    [Command(requiresAuthority = false)]
    public void CmdAnnounce(string message)
    {
        RpcAnnounce(message);
    }

    [ClientRpc]
    public void RpcAnnounce(string message)
    {
        announcer.Announce(message);
    }

}

