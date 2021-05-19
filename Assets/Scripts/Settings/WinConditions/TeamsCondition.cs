using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class TeamsCondition : WinCondition
{

    private List<List<Player>> teams = new List<List<Player>>();

    #region Virtuals

    [Server]
    public override void StartWinCondition()
    {
        // Get all players
        Player[] players = FindObjectsOfType<Player>();

        // Subscribe to the lives changed event
        foreach (Player p in players)
        {
            p.GetComponent<Health>().EventLivesChanged += OnLivesChanged;
        }

        // for each team add them to the teams list
        for (int i = 0; i < TeamsGamemode.maxTeams; i++)
        {
            List<Player> t = players.Where(p => p.teamIndex == i).ToList();

            if (t.Count > 0) teams.Add(t);
        }

    }

    #endregion

    private void OnLivesChanged(int currentHealth, int maxHealth, GameObject player)
    {
        // Everytime a player is eliminated, increment eliminatedPlayers counter and 
        // check if the win condition was satisfied
        if (currentHealth <= 0)
        {
            // Remove the dead player from the list
            teams[player.GetComponent<Player>().teamIndex].Remove(player.GetComponent<Player>());

            // Check if the team is now all dead
            if (teams[player.GetComponent<Player>().teamIndex].Count <= 0) base.InvokeWinConditionSatisfied();

            //eliminatedPlayers++;

            //if (totalPlayers == 1 && eliminatedPlayers == 1) // singleplayer case
            //    base.InvokeWinConditionSatisfied();
            //else if (eliminatedPlayers == totalPlayers - 1) // multiplayer case
            //    base.InvokeWinConditionSatisfied();
        }
    }
}
