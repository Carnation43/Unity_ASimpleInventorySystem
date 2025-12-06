using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerState_InAir : PlayerState
{
    private PlayerSettings.InAirSettings _settings;

    private AirSubState currentSubState;
    private enum AirSubState
    {
        Rising,
        Falling,
        // Gliding,
        // AttackInAir,
        // DashInAir,
        // DefenseInAir,
    }

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private int jumpRemaining;

    public PlayerState_InAir(PlayerCharacter player, PlayerStateMachine machine) : base(player, machine)
    {
        _settings = player.Settings.InAir;
    }

    public override void Enter()
    {
        base.Enter();
        playerCharacter.Anim.SetBool("isGrounded", false);

        jumpRemaining = _settings.MaxJump - 1;
        jumpBufferCounter = 0f;

        if (playerCharacter.Motor.Velocity.y > 0.01f)
        {
            playerCharacter.Anim.SetBool("isJumping", true);
            SwitchSubState(AirSubState.Rising);
            coyoteTimeCounter = 0f;
        }
        else
        {
            // Determine if we are falling
            SwitchSubState(AirSubState.Falling);
            if(stateMachine.PreviousState is PlayerState_Grounded)
            {
                coyoteTimeCounter = _settings.CoyoteTime;
            }
            else
            {
                coyoteTimeCounter = 0f;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        // TODO: Double jump

        if (coyoteTimeCounter > 0) coyoteTimeCounter -= Time.deltaTime;
        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;

        switch (currentSubState)
        {
            case AirSubState.Rising: UpdateRisingState(); break;
            case AirSubState.Falling: UpdateFallingState(); break;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (IsGrounded() && playerCharacter.Motor.Velocity.y < 0.01f)
        {
            if (jumpBufferCounter > 0)
            {
                playerCharacter.Anim.SetBool("isJumping", true);
                playerCharacter.Anim.SetTrigger("jump");
                playerCharacter.Motor.Jump(_settings.JumpForce);
                SwitchSubState(AirSubState.Rising);
                jumpBufferCounter = 0f;
            }
            else
            {
                stateMachine.ChangeState(playerCharacter.GroundedState);
                return;
            }
        }

        ApplyAirPhysics();
        playerCharacter.Anim.SetFloat("yVelocity", playerCharacter.Motor.Velocity.y);
    }

    public override void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (coyoteTimeCounter > 0)
            {
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
                playerCharacter.Motor.Jump(_settings.JumpForce);
                playerCharacter.Anim.SetTrigger("jump");
                SwitchSubState(AirSubState.Rising);
            }
            else if (jumpRemaining > 0)
            {
                PerformAirJump();
            }
            else
            {
                jumpBufferCounter = _settings.JumpBuffer;
            }
        }
    }

    private void PerformAirJump()
    {
        jumpRemaining--;
        jumpBufferCounter = 0f;

        playerCharacter.Anim.SetTrigger("jump");
        playerCharacter.Motor.Jump(_settings.JumpForce);
        SwitchSubState(AirSubState.Rising);
    }

    private void UpdateRisingState()
    {
        if (playerCharacter.Motor.Velocity.y < 0) SwitchSubState(AirSubState.Falling);
    }

    private void UpdateFallingState()
    {
        // TODO: Check for sliding or air-attack transitions
    }

    private void SwitchSubState(AirSubState newState)
    {
        currentSubState = newState;
    }

    private void ApplyAirPhysics()
    {
        float gravityMultiplier = (currentSubState == AirSubState.Falling) ? _settings.FallGravityMultiplier : 1f;
        playerCharacter.Motor.SetGravityScale(_settings.GravityScale * gravityMultiplier);
        playerCharacter.Motor.Move(playerCharacter.MoveInput, _settings.MaxAirSpeed, _settings.AirAcceleration, _settings.AirAcceleration);
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
