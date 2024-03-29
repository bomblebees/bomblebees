﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Mirror;

public class ComboGamemode : Gamemode
{
    [SerializeField] private string gamemodeName = "Combo Frenzy";

    [Header("Defaults")]
    [SerializeField] private float roundDuration = 0f;
    [SerializeField] private int playerLives = 0;

    [Tooltip("Number of combos to subtract from the player when they die")]
    [SerializeField] public static int comboPenalty = 10;

    [Tooltip("Number of combos to add to the player when they kill someone")]
    [SerializeField] public static int comboBonus = 10;

    // -- Fields -- //
    public override string GamemodeName { get { return gamemodeName; } }
    public override float RoundDuration { get { return roundDuration; } }
    public override int PlayerLives { get { return playerLives; } }

    // -- Methods -- //
    public override string GetDescription()
    {
        string desc = "The first bee to reach the set number of combos wins!" +
            "\n <color=#DDEF1F>" +
            "\nLonger combos count extra" +
            "\nKills award " + comboBonus + " points" +
            "\nDeaths takes away " + comboPenalty + " points" +
            "</color>";

        return desc;
    }

    public override GameObject[] GetWinningOrder(GameObject[] playerList)
    {
        GameObject[] orderedList = new GameObject[playerList.Length];

        // Order the player list by most combos
        orderedList = playerList.OrderByDescending(p => p.GetComponent<PlayerStatTracker>().totalBombCombosMade).ToArray();

        return orderedList;
    }

    #region Settings

    [Client]
    public override void EnableGamemodeSettings()
    {
        comboContainer.SetActive(true);
    }

    [Client]
    public override void DisableGamemodeSettings()
    {
        comboContainer.SetActive(false);
    }

    [Header("Settings")]
    [SyncVar(hook = nameof(OnChangeCombos))]
    public int combos;

    [SerializeField] private GameObject comboContainer;
    [SerializeField] private TMP_Text combosText;

    [SerializeField] private int[] combosList = { 10, 20, 30, 40 };
    private int comboSelected = 0;

    public override void OnStartClient()
    {
        combos = combosList[comboSelected];
        SetCombosText();
    }

    [Server]
    public void OnClickCombosButton()
    {
        if (!isServer) return; // Only host can change settings

        // Get next selected lives
        comboSelected = (comboSelected + 1) % combosList.Length;

        // New lives is now that
        combos = combosList[comboSelected];
    }

    [ClientCallback]
    private void OnChangeCombos(int _, int newCombo)
    {
        SetCombosText();
    }

    [Client]
    private void SetCombosText()
    {
        combosText.text = "First to " + combos.ToString();
    }

    #endregion
}