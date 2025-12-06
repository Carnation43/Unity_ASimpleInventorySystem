using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RestSystemController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("How long the entrance animation takes")]
    [SerializeField] private float _sequenceDuration = 1.0f;
    [Tooltip("Target zoom size for the camera during rest.")]
    [SerializeField] private float _targetZoomSize = 7.0f;
    [Tooltip("The player's position(0.5 is middle, less than 0.5 is left)")]
    [SerializeField] private float _targetScreenX = 0.7f;
    [SerializeField] private float _targetScreenY = 0.75f;

    [Header("Dependencies")]
    [Tooltip("Input channel")]
    [SerializeField] private InputEventChannel _inputChannel;
    [SerializeField] private PlayerCharacter _playerCharacter;

    // inner parameter
    private bool _isResting = false;

    private bool _isPlayerInRestZone = false;

    private bool _isTransitioning = false;

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        if (_inputChannel != null)
        {
            _inputChannel.OnAttack += HandleKeyJ;
        }
    }

    private void OnDisable()
    {
        if (_inputChannel != null)
        {
            _inputChannel.OnAttack -= HandleKeyJ;
        }
    }

    // --- API for PlayerTriggerZone START ---

    public void SetPlayerInRestZone(bool isInZone)
    {
        _isPlayerInRestZone = isInZone;
    }

    // --- API for PlayerTriggerZone END ---

    private void HandleKeyJ(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (_isTransitioning) return;
            if (_isResting) return;
            if (!_isPlayerInRestZone) return;
            if (_playerCharacter == null && _playerCharacter.StateMachine.CurrentState != _playerCharacter.GroundedState) return;

            StartRestSequence();
        }
    }

    private void StartRestSequence()
    {
        if (_isResting) return;
        _isTransitioning = true;

        if (MenuController.instance != null)
        {
            MenuController.instance.OnSideMenuExit += HandleRestModeExited;
        }

        StartCoroutine(StartRestRoutine());
    }

    private IEnumerator StartRestRoutine()
    {
        _isResting = true;

        // 1. Lock Player Input immediately && Starting Resting Animation
        if (_inputChannel != null) _inputChannel.RaiseGlobalInputLockEvent(true);

        if (_playerCharacter != null)
        {
            _playerCharacter.StartResting();
        }

        // 2. Cinematic Visuals
        CinematicManager.Instance.ShowBars(_sequenceDuration);
        CinematicManager.Instance.MoveAndZoom(_targetZoomSize, _targetScreenX, _targetScreenY, _sequenceDuration);

        yield return CinematicManager.Instance.FadeScreen(1f, _sequenceDuration);

        // --- FULL BLACK SCREEN ---

        // 3. Logic processing
        Debug.Log("<color=yellow>[RestSystem] Resetting World Data...</color>");
        Debug.Log("<color=yellow> Saving...</color>");

        // Simulate a short pause for "saving"
        yield return new WaitForSeconds(0.5f);

        // 4. Prepare UI (Open Side Menu while screen is still black)
        if (MenuController.instance != null)
        {
            MenuController.instance.OpenRestMenu();
        }

        // 5. Fade In
        yield return CinematicManager.Instance.FadeScreen(0f, _sequenceDuration);

        // 6. Unlock Input
        if (_inputChannel != null) _inputChannel.RaiseGlobalInputLockEvent(false);

        _isTransitioning = false;
    }

    /// <summary>
    /// subscribe to :MenuController -> OnSideMenuExit
    /// </summary>
    private void HandleRestModeExited()
    {
        if (_isTransitioning) return;
        Debug.Log("<color=green>[RestSystem] Receive exit signal, reset to rest state</color>");

        if (MenuController.instance != null)
        {
            MenuController.instance.OnSideMenuExit -= HandleRestModeExited;
        }

        StartCoroutine(ExitRestRoutine());
    }

    private IEnumerator ExitRestRoutine()
    {
        _isTransitioning = true;

        // 1. Lock input
        _inputChannel?.RaiseGlobalInputLockEvent(true);

        // 2. Hide SideMenu UI
        if (MenuController.instance != null)
        {
            MenuController.instance.CloseResetUI();
        }

        // 3. Call Cinematic Visual
        if (CinematicManager.Instance != null)
        {
            CinematicManager.Instance.ExitCutsceneMode(_sequenceDuration);
        }

        // 4. Player stand up
        if (_playerCharacter != null)
        {
            _playerCharacter.StopResting();
        }

        // Wait for animation to complete
        yield return new WaitForSeconds(_sequenceDuration);


        _isResting = false;
        _isTransitioning = false;

        _inputChannel.RaiseGlobalInputLockEvent(false);

        Debug.Log("[RestSystemController] Exited completely");
        
    }

    private void OnDestroy()
    {
        if (MenuController.instance != null)
        {
            MenuController.instance.OnSideMenuExit -= HandleRestModeExited;
        }
    }
}
