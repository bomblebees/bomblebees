using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/*
 * Event manager class for logging game events, these events will 
 * only be called on the host/server.
 * Useful for running something when a particular event happens. 
 */
public class EventManager : NetworkBehaviour
{
    public float roundStartTime;

    // delegates
    public delegate void PlayerLoadedDelegate(GameObject player, int remainingPlayers);
    public delegate void StartRoundDelegate();
    public delegate void EndRoundDelegate(List<Player> players);
    public delegate void ReturnToLobbyDelegate();
    public delegate void BombPlacedDelegate(GameObject bomb, GameObject player);
    public delegate void PlayerTookDamageDelegate(int newLives, GameObject bomb, GameObject player);
    public delegate void PlayerSwapDelegate(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded);
    public delegate void PlayerSpinDelegate(GameObject player, GameObject bomb);
	public delegate void PlayerMultikillDelegate(GameObject player, int killNumber);

    // events
    public event PlayerLoadedDelegate EventPlayerLoaded;
    public event StartRoundDelegate EventStartRound;
    public event EndRoundDelegate EventEndRound;
    public event ReturnToLobbyDelegate EventReturnToLobby;
    public event BombPlacedDelegate EventBombPlaced;
    public event PlayerTookDamageDelegate EventPlayerTookDamage;
    public event PlayerSwapDelegate EventPlayerSwap;
    public event PlayerSpinDelegate EventPlayerSpin;
	public event PlayerMultikillDelegate EventMultikill;

    // singletons
    public static EventManager _instance;
    public static EventManager Singleton { get { return _instance; } }

    private SessionLogger sessionLogger;
    private NetworkRoomManagerExt room;


    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: EventManager");
        else _instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        sessionLogger = SessionLogger.Singleton;
        if (sessionLogger == null) Debug.LogError("Cannot find Singleton: SessionLogger");

        room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        if (room == null) Debug.LogError("Cannot find Singleton: NetworkRoomManagerExt");

        totalPlayers = room.roomSlots.Count;
    }

    #region Events

    [NonSerialized] public int playersLoaded;
    [NonSerialized] public int totalPlayers;

    /// <summary>
    /// Called when a player has loaded into the game scene
    /// </summary>
    /// <param name="player">The player object that was loaded.</param>
    [Server]
    public void OnPlayerLoadedIntoGame(GameObject player)
    {
        playersLoaded++;
        EventPlayerLoaded?.Invoke(player, totalPlayers - playersLoaded);
    }

    /// <summary>
    /// Called when the round starts
    /// </summary>
    [Server]
    public void OnStartRound()
    {
        roundStartTime = Time.time;
        sessionLogger.InitializeSession();

        EventStartRound?.Invoke();
    }

    /// <summary>
    /// Called when the round ends (before scene changed)
    /// </summary>
    [Server]
    public void OnEndRound(List<Player> players)
    {
        sessionLogger.CollectData(Time.time - roundStartTime, players);
        //Debug.Log("end round time: " + Time.time);

        EventEndRound?.Invoke(players);
    }

    /// <summary>
    /// Called just before players are sent back to the lobby from the game
    /// </summary>
    [Server]
    public void OnReturnToLobby()
    {
        sessionLogger.ShowPermsPopup();

        EventReturnToLobby?.Invoke();
    }

    /// <summary>
    /// Called when a player has placed a bomb.
    /// </summary>
    /// <param name="bomb">The bomb object that was placed</param>
    /// <param name="player">The player object who placed the bomb</param>
    [Server]
    public void OnBombPlaced(GameObject bomb, GameObject player)
    {
        sessionLogger.CreateEventBOM(bomb, player, Time.time - roundStartTime);

        //Debug.Log("bomb placed: " + code);
        //Debug.Log("player who placed the bomb: " + player.name);

        EventBombPlaced?.Invoke(bomb, player);
    }

    /// <summary>
    /// Called when a player takes damage (from a bomb)
    /// </summary>
    /// /// <param name="newLives">The new lives of the player after taking damage</param>
    /// <param name="bomb">The bomb object that caused the damage</param>
    /// <param name="player">The player object who took damage</param>
    [Server]
    public void OnPlayerTookDamage(int newLives, GameObject bomb, GameObject player)
    {

        if (bomb && bomb.GetComponent<ComboObject>()) {
            sessionLogger.CreateEventDMG(
                bomb,
                player,
                bomb.GetComponent<ComboObject>().GetOwnerPlayer(),
                Time.time - roundStartTime);
            EventPlayerTookDamage?.Invoke(newLives, bomb, player);
        }

		// Take the NetworkTime.time and pass it into a method on Player for detecting multi kills
		// this is server code!

		// The killer is stored in the bomb object, so get the player component there and call the method on that player
		// if not self destruct, count towards multikill: 
		if (!ReferenceEquals(bomb.GetComponent<ComboObject>().triggeringPlayer, player))
		{
			bomb.GetComponent<ComboObject>().triggeringPlayer.GetComponent<Player>().TrackMultiKill(NetworkTime.time);
		}
		else
		{
			Debug.Log("Player took damage from own bomb");
		}
		

        //Debug.Log("player who took damage: " + player.name);
        //Debug.Log("player lives: " + newLives.ToString());
        //Debug.Log("bomb that hit the player: " + bomb);
        //Debug.Log("original person who placed the bomb " + bomb.GetComponent<ComboObject>().GetOwnerPlayer());
    }

    /// <summary>
    /// Called when player swaps a tile
    /// </summary>
    /// <param name="oldKey">The hex cell key that was in the players hand before swapping</param>
    /// <param name="newKey">The hex cell key that was swapped with the players hand</param>
    /// <param name="combo">Whether or not the swap made a combo</param>
    /// <param name="player">The player who swapped</param>
    [Server]
    public void OnPlayerSwap(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
    {
        sessionLogger.CreateEventSWP(oldKey, newKey, combo, player, Time.time - roundStartTime);

        //Debug.Log("player swapped hand " + oldKey + " with " + newKey + ", player: " + player.name);
        //if (combo) Debug.Log("MADE A COMBO " + oldKey);
        EventPlayerSwap?.Invoke(oldKey, newKey, combo, player, numBombsAwarded);
    }

    /// <summary>
    /// Called when a player spins
    /// </summary>
    /// <param name="player">The player who spun</param>
    /// <param name="bomb">The bomb object that was hit, null if it did not hit anything</param>
    [Server]
    public void OnPlayerSpin(GameObject player, GameObject bomb = null)
    {
        if (bomb == null) // If the spin did not hit anything
        {
            sessionLogger.CreateEventSPN(
                player,
                bomb,
                Time.time - roundStartTime - player.GetComponent<PlayerSpin>().spinHitboxDuration); // subtract the time waited for event time
        } else // if the spin hit a bomb
        {
            sessionLogger.CreateEventSPN(player, bomb, Time.time - roundStartTime);
        }
        EventPlayerSpin?.Invoke(player, bomb);
    }

	/// <summary>
	/// Called when a player gets a multikill
	/// </summary>
	/// <param name="player">The player that achieved the multikill</param>
	/// <param name="killNumber">The current multikill number</param>
	[Server]
	public void OnPlayerMultikill(GameObject player, int killNumber)
	{
		Debug.Log("Multikill recorded! By " + player.GetComponent<Player>().steamName + ", multikill size of " + killNumber);
		EventMultikill?.Invoke(player, killNumber);
	}
    #endregion
}
