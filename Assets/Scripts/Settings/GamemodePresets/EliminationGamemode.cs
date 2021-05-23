using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Mirror;

public class EliminationGamemode : Gamemode
{
    [SerializeField] private string gamemodeName = "Elimination";

    [Header("Defaults")]
    [SerializeField] private float roundDuration = 0f;
    [SerializeField] private int playerLives = 0;

    // -- Fields -- //
    public override string GamemodeName { get { return gamemodeName; } }
    public override float RoundDuration { get { return roundDuration; } }
    public override int PlayerLives { get { return playerLives; } }

    // -- Methods -- //
    public override string GetDescription()
    {
        string desc = "The mode for those that are blood thirsty" +
            "\n\n <color=#DDEF1F>The first bee to reach the set number of eliminations wins!</color>";

        return desc;
    }

    public override GameObject[] GetWinningOrder(GameObject[] playerList)
    {
        GameObject[] orderedList = new GameObject[playerList.Length];

        // Order the player list by winning order
        orderedList = playerList.OrderByDescending(p => p.GetComponent<PlayerStatTracker>().kills) // order by most kills
                    .ThenByDescending(p => p.GetComponent<PlayerStatTracker>().doubleKills) // then by most double kills
                    .ThenByDescending(p => p.GetComponent<PlayerStatTracker>().totalCombosMade) // then by most combos
                    .ThenBy(p => p.GetComponent<PlayerStatTracker>().deaths) // then by least deaths
                    .ToArray();

        return orderedList;
    }

    #region Settings

    [Client] public override void EnableGamemodeSettings()
    {
        eliminationContainer.SetActive(true);
    }

    [Client] public override void DisableGamemodeSettings()
    {
        eliminationContainer.SetActive(false);
    }

    [Header("Settings")]
    [SyncVar(hook = nameof(OnChangeEliminations))]
    public int eliminations;

    [SerializeField] private GameObject eliminationContainer;
    [SerializeField] private TMP_Text eliminationsText;

    [SerializeField] private int[] eliminationsList = { 5, 10, 15, 20 };
    private int eliminationSelected = 0;

    public override void OnStartClient()
    {
        eliminations = eliminationsList[eliminationSelected];
        SetEliminationsText();
    }

    [Server] public void OnClickEliminationsButton()
    {
        if (!isServer) return; // Only host can change settings

        // Get next selected lives
        eliminationSelected = (eliminationSelected + 1) % eliminationsList.Length;

        // New lives is now that
        eliminations = eliminationsList[eliminationSelected];
    }

    [ClientCallback] private void OnChangeEliminations(int _, int newElim)
    {
        SetEliminationsText();
    }

    [Client] private void SetEliminationsText()
    {
        eliminationsText.text = "First to " + eliminations.ToString();
    }

    #endregion
}