using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// A server-only object that specifies a win condition of the game.
/// </summary>
public abstract class WinCondition : NetworkBehaviour
{
    /// <summary>
    /// Whether this condition is satisfied
    /// </summary>
    public bool conditionSatisfied = false;

    /// <summary>
    /// Whether this condition is stopped
    /// </summary>
    public bool conditionStopped = false;

    /// <summary>
    /// An event that can be invoked when the win condition is satisfied
    /// </summary>
    public event WinConditionDelegate EventWinConditionSatisfied;
    public delegate void WinConditionDelegate();

    /// <summary>
    /// Initializes any necessary behaviours for this win condition.
    /// This would be a good place to setup variables used in StartWinCondition()
    /// <para>Called when this object is created</para>
    /// </summary>
    public virtual void InitWinCondition() { }

    /// <summary>
    /// Sets up any necessary behaviours for this win condition.
    /// <para>Called when the round starts</para>
    /// </summary>
    public virtual void StartWinCondition() { }

    /// <summary>
    /// For any thing that needs to be done after the condition is satisfied.
    /// <para>Called right after this win condition is satisfied</para>
    /// </summary>
    public virtual void FinishWinCondition() { }

    /// <summary>
    /// Deconstructs any behaviours for this win condition
    /// <para>Called after the round has ended</para>
    /// </summary>
    public virtual void StopWinCondition() { conditionStopped = true;  }

    /// <summary>
    /// Checks whether this win condition is satisfied.
    /// </summary>
    /// <returns>Whether or not the win condition was satisfied.</returns>
    public virtual bool CheckWinCondition() { return conditionSatisfied; }

    /// <summary>
    /// Invokes the event EventWinConditionSatisfied.
    /// <para>Should be called by child in CheckWinCondition</para>
    /// </summary>
    public void InvokeWinConditionSatisfied()
    {
        conditionSatisfied = true;

        // Invoke the event before we finish win condition, order may matter
        if (!conditionStopped)
        {
            EventWinConditionSatisfied.Invoke();
            FinishWinCondition();
        }
    }
}
