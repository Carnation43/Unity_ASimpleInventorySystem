using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Defines the standard lifecycle methods
/// </summary>
public abstract class PlayerState
{
    protected PlayerCharacter playerCharacter;
    protected PlayerStateMachine stateMachine;

    public PlayerState(PlayerCharacter player, PlayerStateMachine machine)
    {
        this.playerCharacter = player;
        this.stateMachine = machine;
    } 

    public virtual void Enter()
    {

    }

    public virtual void Exit()
    {

    }
    
    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void OnJumpInput(InputAction.CallbackContext context) { }
}
