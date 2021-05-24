using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ComboCondition : WinCondition
{
    private int toCombos;

    private Player[] players;

    #region Virtuals

    [Server]
    public override void InitWinCondition()
    {
        toCombos = FindObjectOfType<ComboGamemode>().combos;
    }

    [Server]
    public override void StartWinCondition()
    {
        players = FindObjectsOfType<Player>();

        // Subscribe to the swap event
        FindObjectOfType<EventManager>().EventPlayerSwap += OnSwapEvent;
    }

    #endregion

    private void OnSwapEvent(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
    {
        // If a combo was not made, return;
        if (!combo) return;

        // Everytime a player is eliminated, check if total combos is reached
        foreach (Player p in players)
        {
            if (p.GetComponent<PlayerStatTracker>().totalBombCombosMade >= toCombos)
            {
                base.InvokeWinConditionSatisfied();
            }
        }
    }
}
