using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Default win condition, round ends when timer reached zero
/// </summary>
public class TimerCondition : WinCondition
{
    private float timerDuration = 5f;

    #region Virtuals

    [Server]public override void InitWinCondition()
    {
        // we should request timerDuration here from lobby settings
    }

    [Server] public override void StartWinCondition()
    {
        StartCoroutine(TimerCoroutine());
    }

    #endregion

    [Server] private IEnumerator TimerCoroutine()
    {
        // Wait for timer to end, - 1 second for start time delay
        yield return new WaitForSeconds(timerDuration - 1);

        // Condition is satisfied after timer ends
        conditionSatisfied = true;
        InvokeWinConditionSatisfied();
    }
}
