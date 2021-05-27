using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Mirror;

public class TeamsGamemode : Gamemode
{
    [SerializeField] private string gamemodeName = "Teams";

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
        string desc = "Standard teams versus teams mode, the last team standing wins!";

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

    #region Settings

    [Client]
    public override void EnableGamemodeSettings()
    {
        teamsContainer.SetActive(true);
    }

    [Client]
    public override void DisableGamemodeSettings()
    {
        teamsContainer.SetActive(false);
    }

    [Header("Settings")]
    [SyncVar(hook = nameof(OnChangeTeams))]
    public int teams;

    [SerializeField] private GameObject teamsContainer;
    [SerializeField] private TMP_Text teamsText;

    [SerializeField] private int[] teamsList = { 2,3 };
    private int teamsSelected = 0;

    public override void OnStartClient()
    {
        teams = teamsList[teamsSelected];
        SetTeamsText();
    }

    [Server]
    public void OnClickTeamsButton()
    {
        if (!isServer) return; // Only host can change settings

        // Get next selected lives
        teamsSelected = (teamsSelected + 1) % teamsList.Length;

        // New lives is now that
        teams = teamsList[teamsSelected];
    }

    [ClientCallback]
    private void OnChangeTeams(int _, int newElim)
    {
        SetTeamsText();
    }

    [Client]
    private void SetTeamsText()
    {
        teamsText.text = teams.ToString() + " Teams";
    }

    #endregion
}