using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StandardGamemode : Gamemode
{
    [SerializeField] private string gamemodeName = "Free for all";

    [Header("Defaults")]
    [SerializeField] private float roundDuration = 180f;
    [SerializeField] private int playerLives = 3;

    // -- Fields -- //
    public override string GamemodeName { get { return gamemodeName; } }
    public override float RoundDuration { get { return roundDuration; } }
    public override int PlayerLives { get { return playerLives; } }

    // -- Methods -- //
    public override string GetDescription()
    {
        string desc = "The Bomblebees battle royale experience, last bee standing wins!" +
            "\n <color=#DDEF1F>" +
            "\n Player lives limited to 3" +
            "</color>";

        return desc;
    }

    public override GameObject[] GetWinningOrder(GameObject[] playerList)
    {
        GameObject[] orderedList = new GameObject[playerList.Length];

        // Order the player list by winning order
        orderedList = playerList.OrderByDescending(p => p.GetComponent<Health>().currentLives) // order by most health first
                    .ThenByDescending(p => p.GetComponent<PlayerStatTracker>().timeOfElimination) // then by latest time of death (if applicable)
                    .ThenByDescending(p => p.GetComponent<PlayerStatTracker>().kills) // then by most kills
                    .ThenByDescending(p => p.GetComponent<PlayerStatTracker>().totalCombosMade) // then by most combos
                    .ThenByDescending(p => p.GetComponent<PlayerStatTracker>().doubleKills) // then by most double kills
                    .ToArray(); // turn into list

        return orderedList;
    }
}