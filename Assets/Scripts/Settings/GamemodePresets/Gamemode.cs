using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gamemode : MonoBehaviour
{
    // -- Game Settings -- //
    public virtual float RoundDuration { get; } = 0f;

    // -- Win Conditions -- //
    public virtual bool EndAfterFirstWinCondition { get; } = false;
    public virtual bool ByLastAlive { get; } = false;
    public virtual bool ByTimerFinished { get; } = false;



    /// <summary>
    /// Gets a description of the gamemode to be displayed on the settings menu
    /// </summary>
    /// <returns> A description of the gamemode</returns>
    public virtual string GetDescription()
    {
        return "Description WIP";
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

        // Load win conditions
        settings.endAfterFirstWinCondition = EndAfterFirstWinCondition;
        settings.byLastAlive = ByLastAlive;
        settings.byTimerFinished = ByTimerFinished;
    }

}
