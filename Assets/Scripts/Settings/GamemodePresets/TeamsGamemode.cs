using System.Collections;
using UnityEngine;

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
}