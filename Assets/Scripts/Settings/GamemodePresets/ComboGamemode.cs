using System.Linq;
using TMPro;
using UnityEngine;
using Mirror;

public class ComboGamemode : Gamemode
{
    [SerializeField] private string gamemodeName = "Combo Frenzy";

    [Header("Defaults")]
    [SerializeField] private float roundDuration;
    [SerializeField] private int playerLives;

    [Tooltip("Number of combos to add to the player when they kill someone")]
    public static int comboBonus = 20;
    
    [Tooltip("Number of combos to subtract from the player when they die")]
    public static int comboPenalty = 20;

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
        // Order the player list by most combos
        var orderedList = playerList.OrderByDescending(p => p.GetComponent<PlayerStatTracker>().totalPoints).ToArray();

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
    public int points;

    [SerializeField] private GameObject comboContainer;
    [SerializeField] private TMP_Text combosText;

    [SerializeField] private int[] winPointList = { 120, 150, 180, 90 };
    private int _comboSelected;

    public override void OnStartClient()
    {
        points = winPointList[_comboSelected];
        SetCombosText();
    }

    [Server]
    public void OnClickCombosButton()
    {
        if (!isServer) return; // Only host can change settings

        // Get next selected lives
        _comboSelected = (_comboSelected + 1) % winPointList.Length;

        // New lives is now that
        points = winPointList[_comboSelected];
    }

    [ClientCallback]
    private void OnChangeCombos(int _, int newCombo)
    {
        SetCombosText();
    }

    [Client]
    private void SetCombosText()
    {
        combosText.text = $"First to {points} points";
    }

    #endregion
}