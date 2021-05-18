using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StandardGamemode : Gamemode
{
    [SerializeField] private string gamemodeName = "Standard";

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
        string desc = "The classic free for all Bomblebees experience" +
            "\n\n <color=#DDEF1F>The last bee standing wins!</color>";

        return desc;
    }

    public override GameObject[] GetWinningOrder(List<RoundManager.PlayerInfo> playerList)
    {
        List<RoundManager.PlayerInfo> orderedList = new List<RoundManager.PlayerInfo>();

        // Order the player list by winning order
        orderedList = playerList.OrderByDescending(p => p.health.currentLives) // order by most health first
                    .ThenByDescending(p => p.timeOfElim) // then by latest time of death (if applicable)
                    .ThenByDescending(p => p.player.GetComponent<PlayerStatTracker>().kills) // then by most kills
                    .ThenByDescending(p => p.player.GetComponent<PlayerStatTracker>().totalCombosMade) // then by most combos
                    .ThenByDescending(p => p.player.GetComponent<PlayerStatTracker>().doubleKills) // then by most double kills
                    .ToList(); // turn into list

        // Convert orderedList into orderedArray
        GameObject[] orderedArray = new GameObject[orderedList.Count];
        for (int i = 0; i < orderedArray.Length; i++)
        {
            orderedArray[i] = orderedList[i].player.gameObject;
        }

        return orderedArray;
    }
}