using System.Collections;
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
}