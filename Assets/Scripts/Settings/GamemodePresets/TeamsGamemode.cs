using System.Collections;
using UnityEngine;

public class TeamsGamemode : Gamemode
{

    [Header("Defaults")]

    [SerializeField] private string gamemodeName = "Teams";

    // -- Game Settings -- //
    [SerializeField] private float roundDuration = 180f;

    // -- Win Conditions -- //
    [SerializeField] private bool endAfterFirstWinCondition = true;
    [SerializeField] private bool byLastAlive = true;
    [SerializeField] private bool byTimerFinished = true;


    #region Getters

    // -- Game Settings -- //
    public override float RoundDuration { get { return roundDuration; } }

    // -- Win Conditions -- //
    public override bool EndAfterFirstWinCondition { get { return endAfterFirstWinCondition; } }
    public override bool ByLastAlive { get { return byLastAlive; } }
    public override bool ByTimerFinished { get { return byTimerFinished; } }

    #endregion

    public override string ToString()
    {
        return gamemodeName;
    }

    //public override string GetDescription()
    //{
    //    string desc = "The classic free for all Bomblebees experience" +
    //        "\n\n <color=#DDEF1F>The last bee standing wins!</color>";

    //    return desc;
    //}
}