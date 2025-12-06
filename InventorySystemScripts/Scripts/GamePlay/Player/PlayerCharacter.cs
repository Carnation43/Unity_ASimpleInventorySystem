using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Acts as the single point of contact for external systems
/// </summary>
public class PlayerCharacter : MonoBehaviour
{
    [Header("Channel References")]
    public InputEventChannel InputChannel;

    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerMotor Motor { get; private set; }
    public Animator Anim { get; private set; }
    public PlayerSettings Settings { get; private set; }

    public PlayerState_Grounded GroundedState { get; private set; }
    public PlayerState_InAir InAirState { get; private set; }
    public PlayerState_Resting RestingState {get; private set; }
    // TODO: SFX, VFX, System references

    private Vector2 _rawMoveInput;

    public Vector2 MoveInput => (InputChannel != null && InputChannel.IsInputLocked) ? Vector2.zero : _rawMoveInput;

    private void Awake()
    {
        StateMachine = new PlayerStateMachine();
        Motor = GetComponent<PlayerMotor>();
        Anim = GetComponentInChildren<Animator>();
        Settings = GetComponent<PlayerSettings>();

        GroundedState = new PlayerState_Grounded(this, StateMachine);
        InAirState = new PlayerState_InAir(this, StateMachine);
        RestingState = new PlayerState_Resting(this, StateMachine);
    }

    private void OnEnable()
    {
        InputChannel.OnMove += OnMove;
        InputChannel.OnJump += OnJump;
    }

    private void OnDisable()
    {
        InputChannel.OnMove -= OnMove;
        InputChannel.OnJump -= OnJump;
    }

    private void Start()
    {
        StateMachine.Initialize(GroundedState);
    }

    private void Update()
    {
        StateMachine.CurrentState.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.FixedUpdate();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _rawMoveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if ((InputChannel != null && InputChannel.IsInputLocked)) return;

        StateMachine.CurrentState.OnJumpInput(context);
    }

    // --- Called by RestSystemController ---
    public void StartResting()
    {
        StateMachine.ChangeState(RestingState);
    }

    public void StopResting()
    {
        StateMachine.ChangeState(GroundedState);
    }
}
