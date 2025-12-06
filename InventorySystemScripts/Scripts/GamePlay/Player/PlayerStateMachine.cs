using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages transitions between different states
/// </summary>
public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }
    public PlayerState PreviousState { get; private set; }

    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        // TODO:

        CurrentState.Exit();
        PreviousState = CurrentState;
        CurrentState = newState;
        CurrentState.Enter();
    }
}
