using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Active when the player interact with a Rest Point
/// </summary>
public class PlayerState_Resting : PlayerState
{
    public PlayerState_Resting(PlayerCharacter player, PlayerStateMachine machine) : base(player, machine)
    {

    }

    public override void Enter()
    {
        base.Enter();

        // Force stop immediately
        playerCharacter.Motor.Move(Vector2.zero, 0, 0, 1000f);

        playerCharacter.Anim.SetBool("isResting", true);

        Debug.Log("<color=cyan>[PlayerState] starting rest state: sitting down");
    }

    public override void Exit()
    {
        base.Exit();

        playerCharacter.Anim.SetBool("isResting", false);

        Debug.Log("<color=cyan>[PlayerState] exited rest state: standing up");
    }

}
