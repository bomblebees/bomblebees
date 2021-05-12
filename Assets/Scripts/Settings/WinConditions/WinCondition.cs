using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WinCondition : MonoBehaviour
{

    public delegate void WinConditionDelegate();

    /// <summary>
    /// An event that can be invoked when the win condition is satisfied
    /// </summary>
    public event WinConditionDelegate EventWinConditionSatisfied;

    /// <summary>
    /// Sets up any necessary behaviours for this win condition.
    /// <para>Called when the round starts</para>
    /// </summary>
    public abstract void StartWinCondition();

    /// <summary>
    /// Deconstructs any behaviours for this win condition.
    /// <para>Called when the round ends</para>
    /// </summary>
    public abstract void StopWinCondition();

    /// <summary>
    /// Checks whether this win condition is satisfied.
    /// </summary>
    /// <returns>Whether or not the win condition was satisfied.</returns>
    public abstract bool CheckWinCondition();
}
