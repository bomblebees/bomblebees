using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Settings for a game, these values only have effect if they are
/// changed on the host. Otherwise
/// </summary>
public class LobbySettings : NetworkBehaviour
{
    [Header("Diagnostics - Game Settings")]

    [SyncVar(hook = nameof(OnChangeRoundDuration))]
    public float roundDuration;

    [SyncVar(hook = nameof(OnChangePlayerLives))]
    public int playerLives;

    // -------------------- SERVER ONLY -------------------- //
    [Header("Diagnostics - Win Conditions (Server only)")]
    public bool endAfterFirstWinCondition;
    public bool byLastAlive;
    public bool byTimerFinished;

    // local team index, client only
    public int localTeamIndex;

    private void Awake()
    {
        // Persist this object across scenes
        DontDestroyOnLoad(this.gameObject);

        // Make sure the settings menu is hidden
        //screenSettings.SetActive(false);
    }

    public override void OnStartServer()
    {
        LoadGamemodePreset(gamemodeSelected);
    }

    public override void OnStartClient()
    {
        InitGamemode();
        InitRoundDuration();
        InitPlayerLives();
    }

    #region Gamemodes

    [Header("Gamemodes")]
    [SerializeField] private TMP_Text gamemodeButtonText;
    [SerializeField] private TMP_Text gamemodeDescriptionText;

    [SerializeField] private Gamemode[] gamemodes;
    [SyncVar(hook = nameof(OnChangeGamemode))] public int gamemodeSelected = 0;

    /// <summary>
    /// The gamemode object that is selected.
    /// <para>Note: this is only synced on the server!</para>
    /// </summary>
    public Gamemode gamemode;

    [Client] private void InitGamemode()
    {
        SetGamemodeText();
    }

    [Server] private void LoadGamemodePreset(int select)
    {
        gamemode = gamemodes[select];
        gamemode.LoadGamemode();
    }

    [Server] public void OnClickGamemode()
    {
        if (!isServer) return; // Only host can change settings

        // Get next selected duration
        gamemodeSelected = (gamemodeSelected + 1) % gamemodes.Length;

        // Load the selected gamemode settings
        LoadGamemodePreset(gamemodeSelected);
    }

    [ClientCallback] private void OnChangeGamemode(int _, int newSelection)
    {
        SetGamemodeText();
    }

    [Client] private void SetGamemodeText()
    {
        gamemodeButtonText.text = gamemodes[gamemodeSelected].ToString();
        gamemodeDescriptionText.text = gamemodes[gamemodeSelected].GetDescription();
    }

    [Client] public Gamemode GetGamemode()
    {
        return gamemodes[gamemodeSelected];
    }

    #endregion

    #region Settings Menu

    [Header("Settings Menu")]
    [SerializeField] private GameObject screenSettings;

    public void OnClickOpenSettings()
    {
        screenSettings.SetActive(true);
    }
    
    public void OnClickCloseSettings()
    {
        screenSettings.SetActive(false);
    }

    #endregion

    #region Round Duration

    [Header("Round Duration")]
    [SerializeField] private TMP_Text roundDurationText;

    [SerializeField] private float[] durations = { 0f, 60f, 120f, 180f, 240f };
    private int durationSelected = 2;

    [Client] private void InitRoundDuration()
    {
        SetRoundDurationText();
    }

    [Server] public void OnClickRoundDurationButton()
    {
        if (!isServer) return; // Only host can change settings

        // Get next selected duration
        durationSelected = (durationSelected + 1) % durations.Length;

        // New duration is now that
        roundDuration = durations[durationSelected];
    }

    [ClientCallback] private void OnChangeRoundDuration(float _, float newDuration)
    {
        // If new duration is 0, disable the timer win condition
        if (newDuration == 0) byTimerFinished = false;
        else byTimerFinished = true;

        SetRoundDurationText();
    }

    [Client] private void SetRoundDurationText()
    {
        if (roundDuration == 0)
        {
            roundDurationText.text = "Disabled"; return;
        }

        roundDurationText.text = roundDuration.ToString() + " seconds";
    }

    #endregion

    #region Player Lives

    [Header("Player Lives")]
    [SerializeField] private TMP_Text playerLivesText;

    [SerializeField] private int[] lives = { 0, 1, 2, 3 };
    private int livesSelected = 3;

    [Client] private void InitPlayerLives()
    {
        SetPlayerLivesText();
    }

    [Server] public void OnClickPlayerLivesButton()
    {
        if (!isServer) return; // Only host can change settings

        // Get next selected lives
        livesSelected = (livesSelected + 1) % lives.Length;

        // New lives is now that
        playerLives = lives[livesSelected];
    }

    [ClientCallback] private void OnChangePlayerLives(int _, int newLives)
    {
        // If new lives is 0, disable the last alive condition
        if (newLives == 0) byLastAlive = false;
        else byLastAlive = true;

        SetPlayerLivesText();
    }

    [Client] private void SetPlayerLivesText()
    {
        if (playerLives == 0)
        {
            playerLivesText.text = "Infinite"; return;
        }

        playerLivesText.text = playerLives.ToString() + " lives";
    }

    #endregion
}
