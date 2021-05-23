using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EliminationCondition : WinCondition
{
    private int toEliminations;

    private Player[] players;

    #region Virtuals

    [Server]
    public override void InitWinCondition()
    {
        toEliminations = FindObjectOfType<EliminationGamemode>().eliminations;
    }

    [Server]
    public override void StartWinCondition()
    {
        players = FindObjectsOfType<Player>();

        // Get all player healths
        Health[] healths = FindObjectsOfType<Health>();

        // Subscribe to the lives changed event
        foreach (Health h in healths)
        {
            h.EventLivesChanged += OnLivesChanged;
        }
    }

    #endregion

    private void OnLivesChanged(int currentHealth, int maxHealth, GameObject player)
    {
        // Everytime a player is eliminated, check if total eliminations is reached
        foreach (Player p in players)
        {
            if (p.GetComponent<PlayerStatTracker>().kills >= toEliminations)
            {
                base.InvokeWinConditionSatisfied();
            }
        }
    }
}
