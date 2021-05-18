using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Gamemode : MonoBehaviour
{
    public abstract string GamemodeName { get; }

    // -- Game Settings -- //
    public virtual float RoundDuration { get; } = 180f; // default 3 minutes
    public virtual int PlayerLives { get; } = 3; // default 3 lives
    public virtual bool EndAfterFirstWinCondition { get; } = true; // default true



    /// <summary>
    /// Overrides the ToString function to return the gamemode name
    /// </summary>
    public override string ToString()
    {
        return GamemodeName;
    }

    /// <summary>
    /// Gets a description of the gamemode to be displayed on the settings menu
    /// </summary>
    /// <returns> A description of the gamemode</returns>
    public virtual string GetDescription()
    {
        return "Gamemode WIP";
    }


    /// <summary>
    /// Loads this gamemode preset into the current lobby settings
    /// <para> This function must be called on the server! </para>
    /// </summary>
    public virtual void LoadGamemode()
    {
        // Get the lobby settings component
        LobbySettings settings = this.GetComponent<LobbySettings>();

        // Load game settings
        settings.roundDuration = RoundDuration;
        if (RoundDuration > 0) settings.byTimerFinished = true;
        else settings.byTimerFinished = false;

        settings.playerLives = PlayerLives;
        if (PlayerLives > 0) settings.byLastAlive = true;
        else settings.byLastAlive = false;

        settings.endAfterFirstWinCondition = EndAfterFirstWinCondition;
    }



    /// <summary>
    /// Gets the winning order of the player based on this gamemode
    /// </summary>
    /// <param name="playerList"> The unordered list of all players</param>
    /// <returns> The ordered array of player gameobjects</returns>
    public abstract GameObject[] GetWinningOrder(List<RoundManager.PlayerInfo> playerList);

}
