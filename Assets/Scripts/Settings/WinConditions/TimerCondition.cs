using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Default win condition, round ends when timer reached zero
/// </summary>
public class TimerCondition : WinCondition
{
    /// <summary>
    /// Duration of the timer in seconds, defaults to 2 minutes
    /// </summary>
    private float timerDuration = 120f;

    #region Virtuals

    [Server]public override void InitWinCondition()
    {
        // Update the timer duration with the duration specified in lobby settings
        timerDuration = FindObjectOfType<LobbySettings>().roundDuration;
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
        base.InvokeWinConditionSatisfied();
    }
}
