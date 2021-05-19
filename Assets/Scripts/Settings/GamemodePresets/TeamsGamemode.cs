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
        string desc = "Standard teams versus teams mode. " +
            "\n\n <color=#DDEF1F>The last team standing wins!</color>";

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