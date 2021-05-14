using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;


/// <summary>
/// Settings for a game, these values only have effect if they are
/// changed on the host. Otherwise
/// </summary>
public class LobbySettings : NetworkBehaviour
{
    [Header("Diagnostics - Server")]

    // -- Game Settings -- //
    public float roundDuration = 120f;

    // -- Win Conditions -- //
    public bool endAfterFirstWinCondition = true;
    public bool byLastAlive = true;
    public bool byTimerFinished = true;

    private void Awake()
    {
        // Persist this object across scenes
        DontDestroyOnLoad(this.gameObject);

        // Make sure the settings menu is hidden
        //screenSettings.SetActive(false);
    }

    public override void OnStartServer()
    {
        
    }

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
    [SyncVar(hook = nameof(OnChangeRoundDuration))] public int durationSelected = 2;

    private void InitRoundDuration()
    {
        SetRoundDurationText(durationSelected);
    }

    [Server] public void OnClickRoundDurationButton()
    {
        if (!isServer) return; // Only host can change settings

        // Get next selected duration
        durationSelected = (durationSelected + 1) % durations.Length;
    }

    [ClientCallback] private void OnChangeRoundDuration(int _, int newSelection)
    {
        // If new duration is 0, disable the timer win condition
        if (newSelection == 0) byTimerFinished = false;

        // New duration is now that
        roundDuration = durations[newSelection];

        SetRoundDurationText(newSelection);
    }

    private void SetRoundDurationText(int select)
    {
        switch (select)
        {
            case 0: roundDurationText.text = "Disabled"; break;
            case 1: roundDurationText.text = "1 Minute"; break;
            case 2: roundDurationText.text = "2 Minutes"; break;
            case 3: roundDurationText.text = "3 Minutes"; break;
            case 4: roundDurationText.text = "4 Minutes"; break;
        }
    }

    #endregion
}
