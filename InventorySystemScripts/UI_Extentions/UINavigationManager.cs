using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UINavigationManager : MonoBehaviour
{
    public static UINavigationManager Instance { get; private set; }

    private InputSystemUIInputModule _uiInputModule;
    private InputActionReference _originalMoveAction;        // store original move action
    private InputActionReference _noneMoveAction = null;     // None

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _uiInputModule = GetComponent<InputSystemUIInputModule>();
        if (_uiInputModule == null)
        {
            Debug.LogError("UINavigationManager: InputSystemUIInputModule component not found on this GameObject!");
        }

        if(_uiInputModule != null)
        {
            _originalMoveAction = _uiInputModule.move;
        }
    }

    /// <summary>
    /// Set navigation mode
    /// </summary>
    /// <param name="useUnityAutomatic">True: Unity Navigation, False: Custom Navigation</param>
    public void SetNavigationMode(bool useUnityAutomatic)
    {
        if (_uiInputModule == null) return;
        
        if (useUnityAutomatic)
        {
            _uiInputModule.move = _originalMoveAction;
        }
        else
        {
            _uiInputModule.move = _noneMoveAction;
        }
    }
}