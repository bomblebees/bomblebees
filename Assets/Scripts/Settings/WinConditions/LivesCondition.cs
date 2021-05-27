using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Default win condition, last player alive wins
/// </summary>
public class LivesCondition : WinCondition
{
    private int totalPlayers = 0;
    private int eliminatedPlayers = 0;

    #region Virtuals

    [Server] public override void StartWinCondition()
    {
        // Get all player healths
        Health[] healths = FindObjectsOfType<Health>();

        // Subscribe to the lives changed event
        foreach (Health h in healths)
        {
            h.EventLivesChanged += OnLivesChanged;
        }

        // Set total number of players
        totalPlayers = healths.Length;
    }

    #endregion

    private void OnLivesChanged(int currentHealth, int maxHealth, GameObject player)
    {
        // Everytime a player is eliminated, increment eliminatedPlayers counter and 
        // check if the win condition was satisfied
        if (currentHealth <= 0)
        {
            eliminatedPlayers++;

            if (totalPlayers == 1 && eliminatedPlayers == 1) // singleplayer case
                base.InvokeWinConditionSatisfied();
            else if (eliminatedPlayers == totalPlayers - 1) // multiplayer case
                base.InvokeWinConditionSatisfied();
        }
    }
}
