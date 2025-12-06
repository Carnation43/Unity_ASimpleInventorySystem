using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles logic when the player is on the ground
/// Sub-states: Idle, Walk, Run, TODO
/// </summary>
public class PlayerState_Grounded : PlayerState
{
    private PlayerSettings.GroundedSettings _settings;
    private enum GroundSubState
    {
        Idle,
        Walk,
        Run
    }
    private GroundSubState currentSubState;
    private float timeSinceWalkStarted; // walk_tr_run

    public PlayerState_Grounded(PlayerCharacter player, PlayerStateMachine machine) : base(player, machine)
    {
        _settings = player.Settings.Grounded;
    }

    public override void Enter()
    {
        base.Enter();
        // Reset gravity when grounded
        playerCharacter.Motor.SetGravityScale(playerCharacter.Settings.InAir.GravityScale);
        playerCharacter.Anim.SetBool("isGrounded", true);
        playerCharacter.Anim.SetBool("isJumping", false);

        // Reset to Idle unless coming from specifc states
        if (!(stateMachine.PreviousState is PlayerState_InAir))
        {
            SwitchSubState(GroundSubState.Idle);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (!IsGrounded())
        {
            stateMachine.ChangeState(playerCharacter.InAirState);
            return; // Return immediately to prevent excuting grounded logic
        }

        switch (currentSubState)
        {
            case GroundSubState.Idle:
                UpdateIdleState();
                break;
            case GroundSubState.Walk:
                UpdateWalkState();
                break;
            case GroundSubState.Run:
                UpdateRunState();
                break;
        }
        // TODO: Ground detection
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        float currentSpeed = 0f;
        if (currentSubState == GroundSubState.Walk)
        {
            currentSpeed = _settings.WalkSpeed;
        }
        else if (currentSubState == GroundSubState.Run)
        {
            currentSpeed = _settings.RunSpeed;
        }

        playerCharacter.Motor.Move(playerCharacter.MoveInput, currentSpeed, _settings.Acceleration, _settings.Deacceleration);
        playerCharacter.Anim.SetFloat("speed", Mathf.Abs(playerCharacter.MoveInput.x));
    }

    public override void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerCharacter.Anim.SetTrigger("jump");

            playerCharacter.Motor.Jump(playerCharacter.Settings.InAir.JumpForce);

            stateMachine.ChangeState(playerCharacter.InAirState);
        }
    }

    private void UpdateIdleState()
    {
        if (Mathf.Abs(playerCharacter.MoveInput.x) > 0.1f)
        {
            SwitchSubState(GroundSubState.Walk);
        }
    }

    private void UpdateWalkState()
    {
        if (Mathf.Abs(playerCharacter.MoveInput.x) < 0.1f)
        {
            SwitchSubState(GroundSubState.Idle);
            return;
        }

        timeSinceWalkStarted += Time.deltaTime;
        if (timeSinceWalkStarted >= _settings.TimeToStartRunning)
        {
            SwitchSubState(GroundSubState.Run);
        }
    }

    private void UpdateRunState()
    {
        if (Mathf.Abs(playerCharacter.MoveInput.x) < 0.1f)
        {
            SwitchSubState(GroundSubState.Idle);
        }
    }

    private void SwitchSubState(GroundSubState newState)
    {
        currentSubState = newState;

        switch (currentSubState)
        {
            case GroundSubState.Idle:
                playerCharacter.Anim.SetBool("isRunning", false);
                break;
            case GroundSubState.Walk:
                timeSinceWalkStarted = 0f;
                playerCharacter.Anim.SetBool("isRunning", false);
                break;
            case GroundSubState.Run:
                playerCharacter.Anim.SetBool("isRunning", true);
                break;
        }
    }

    private bool IsGrounded()
    {
        return playerCharacter.Motor.IsGrounded(
                playerCharacter.Settings.Detection.GroundedCheckDistance,
                playerCharacter.Settings.Detection.GroundedCheckWidth,
                playerCharacter.Settings.Detection.GroundedLayer
            );
    }
}
