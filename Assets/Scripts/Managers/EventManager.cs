using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
 * Event manager class for logging game events, these events will 
 * only be called on the host/server.
 * Useful for running something when a particular event happens. 
 */
public class EventManager : NetworkBehaviour
{
    public float roundStartTime;

    // singleton
    public static EventManager _instance;
    public static EventManager Singleton { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: EventManager");
        else _instance = this;
    }

    #region Events

    /// <summary>
    /// Called when the round starts
    /// </summary>
    [Server]
    public void EventStartRound()
    {
        roundStartTime = Time.time;
    }

    /// <summary>
    /// Called when the round ends
    /// </summary>
    [Server]
    public void EventEndRound()
    {
        //Debug.Log("end round time: " + Time.time);
    }

    /// <summary>
    /// Called when a player has placed a bomb.
    /// List of Bomb Codes: 
    /// DEF - default bomb
    /// LAS - laser bomb
    /// PLA - plasma bomb
    /// BLK - blink bomb
    /// GRA - gravity bomb
    /// </summary>
    /// <param name="code">Code of the bomb that was placed, refer to list of bomb codes above</param>
    /// <param name="bomb">The bomb object that was placed</param>
    /// <param name="player">The player object who placed the bomb</param>
    [Server]
    public void EventBombPlaced(string code, GameObject bomb, GameObject player)
    {
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
        //Debug.Log("player swapped hand " + oldKey + " with " + newKey + ", player: " + player.name);
        //if (combo) Debug.Log("MADE A COMBO " + oldKey);
    }

    /// <summary>
    /// Called when a player spins
    /// </summary>
    /// <param name="player">The player who spun</param>
    [Server]
    public void EventPlayerSpin(GameObject player)
    {
        //Debug.Log("spun miss at " + Time.time);
        //Debug.Log("player who spun: " + player.name);
    }

    /// <summary>
    /// Called when a player spins, and it hits a bomb
    /// </summary>
    /// <param name="bomb">The bomb object that was hit</param>
    /// <param name="player">The player object who hit the bomb</param>
    [Server]
    public void EventPlayerSpinHit(GameObject bomb, GameObject player)
    {
        //Debug.Log("spun hit at " + Time.time);
        //Debug.Log("play spun hit | player: " + player.name + ", bomb: " + bomb.name);
    }

    #endregion
}
