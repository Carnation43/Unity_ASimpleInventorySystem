using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Upgrade secondary confirmation
/// </summary>
public class LevelUpTooltip : MonoBehaviour
{
    [Header("Broadcasting On")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("UI Reference")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private UnityAction onConfirmAction;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        gameObject.SetActive(false);

        canvasGroup = GetComponent<CanvasGroup>();

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
    }

    private void OnEnable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnConfirm += HandleConfirm;
        }
    }

    private void OnDisable()
    {
        if (inputChannel != null)
        {
            inputChannel.OnConfirm -= HandleConfirm;
        }
    }

    private void HandleConfirm(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected == confirmButton.gameObject)
        {
            OnConfirm();
        }
        else if (currentSelected == cancelButton.gameObject)
        {
            OnCancel();
        }
    }

    public void ShowDialog(int glowsCost, UnityAction confirmAction)
    {
        gameObject.SetActive(true);
        onConfirmAction = confirmAction;

        promptText.text = $"Confirm Upgrade with {glowsCost} Glows?";

        if (inputChannel != null)
        {
            inputChannel.RaiseGlobalInputLockEvent(true);
        }

        // switch to Unity navigation
        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(true);
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);

        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.2f);
    }

    private void OnConfirm()
    {
        onConfirmAction?.Invoke();
        CloseDialog();
    }

    private void OnCancel()
    {
        CloseDialog();
    }

    public void CloseDialog()
    {
        if (inputChannel != null)
        {
            inputChannel.RaiseGlobalInputLockEvent(false);
        }
        
        // switch back to custom navigation
        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.Instance.SetNavigationMode(false);
        }
        EventSystem.current.SetSelectedGameObject(null);

        canvasGroup.DOKill();
        canvasGroup.DOFade(0, 0.2f).OnComplete( () => gameObject.SetActive(false));
    }

}