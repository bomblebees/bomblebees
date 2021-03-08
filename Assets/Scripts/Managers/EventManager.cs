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

    private GameObject missSpinPlayer;
    private float missSpinTime;
    private bool voidSpin = false;

    // singletons
    public static EventManager _instance;
    public static EventManager Singleton { get { return _instance; } }

    private SessionLogger sessionLogger;

    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: EventManager");
        else _instance = this;

        sessionLogger = SessionLogger.Singleton;
        if (sessionLogger == null) Debug.LogError("Cannot find Singleton: SessionLogger");
    }

    #region Events

    /// <summary>
    /// Called when the round starts
    /// </summary>
    [Server]
    public void EventStartRound()
    {
        roundStartTime = Time.time;
        sessionLogger.InitializeSession();
    }

    /// <summary>
    /// Called when the round ends (before scene changed)
    /// </summary>
    [Server]
    public void EventEndRound(List<Player> players)
    {
        sessionLogger.CollectData(Time.time - roundStartTime, players);
        //Debug.Log("end round time: " + Time.time);
    }

    /// <summary>
    /// Called just before players are sent back to the lobby from the game
    /// </summary>
    [Server]
    public void EventBackToLobby()
    {
        sessionLogger.ShowPermsPopup();
    }

    /// <summary>
    /// Called when a player has placed a bomb.
    /// </summary>
    /// <param name="code">Code of the bomb that was placed, refer to list of bomb codes above</param>
    /// <param name="bomb">The bomb object that was placed</param>
    /// <param name="player">The player object who placed the bomb</param>
    [Server]
    public void EventBombPlaced(GameObject bomb, GameObject player)
    {
        sessionLogger.CreateEventBOM(bomb, player, Time.time - roundStartTime);

        //Debug.Log("bomb placed: " + code);
        //Debug.Log("player who placed the bomb: " + player.name);
    }

    /// <summary>
    /// Called when a player takes damage (from a bomb)
    /// </summary>
    /// /// <param name="newLives">The new lives of the player after taking damage</param>
    /// <param name="bomb">The bomb object that caused the damage</param>
    /// <param name="player">The player object who took damage</param>
    [Server]
    public void EventPlayerTookDamage(int newLives, GameObject bomb, GameObject player)
    {
        sessionLogger.CreateEventDMG(
            bomb,
            player,
            bomb.GetComponent<ComboObject>().GetOwnerPlayer(),
            Time.time - roundStartTime);

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
    public void EventPlayerSwap(char oldKey, char newKey, bool combo, GameObject player)
    {
        sessionLogger.CreateEventSWP(oldKey, newKey, combo, player, Time.time - roundStartTime);

        //Debug.Log("player swapped hand " + oldKey + " with " + newKey + ", player: " + player.name);
        //if (combo) Debug.Log("MADE A COMBO " + oldKey);
    }

    /// <summary>
    /// Called when a player spins
    /// </summary>
    /// <param name="player">The player who spun</param>
    /// <param name="bomb">The bomb object that was hit, null if it did not hit anything</param>
    [Server]
    public void EventPlayerSpin(GameObject player, GameObject bomb = null)
    {
        if (bomb == null) // If the spin did not hit anything
        {
            sessionLogger.CreateEventSPN(
                player,
                bomb,
                Time.time - roundStartTime - player.GetComponent<Player>().spinHitboxDuration); // subtract the time waited for event time
        } else // if the spin hit a bomb
        {
            sessionLogger.CreateEventSPN(player, bomb, Time.time - roundStartTime);
        }
    }

    #endregion
}
