using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using InstanceResetToDefault;
using UnityEngine.InputSystem;

public class TooltipInstance : MonoBehaviour, IResettable
{
    public static TooltipInstance instance;

    [Header("Listen to")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Components")]
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text amount;
    [SerializeField] Button exitButton;
    public Button detailsButton;
    public Button equipButton;
    public TMP_Text equipButtonText;
    [Tooltip("Calculate the position of the tooltip relative to the inventory")]
    [SerializeField] RectTransform scrollAreaRect;

    [Header("Components - stats")]
    [SerializeField] GameObject attackGo;
    [SerializeField] TMP_Text attackValue;
    [SerializeField] TMP_Text attackCorrectionValue;        // Not set for the time being
    [SerializeField] GameObject defenceGo;
    [SerializeField] TMP_Text defenceValue;
    [SerializeField] TMP_Text defenceCorrectionValue;       // Not set for the time being
    [SerializeField] GameObject hpGo;
    [SerializeField] TMP_Text hpValue;

    [SerializeField] TMP_Text description;

    CanvasGroup canvasGroup;


    private RectTransform tooltipRectTransform;
    public RectTransform _trackedRectTransform;

    private Sequence currentSeq;

    public bool IsHidden { get; private set; } = false;
    public bool IsAnimating { get; private set; }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        canvasGroup = GetComponent<CanvasGroup>();
        tooltipRectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.Register(this);
        }

        if (inputChannel != null)
        {
            inputChannel.OnHide += HandleHide;
        }
    }

    private void OnDisable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.UnRegister(this);
        }

        if (inputChannel != null)
        {
            inputChannel.OnHide -= HandleHide;
        }
    }

    private void Update()
    {
     
        if (_trackedRectTransform != null)
        {
            tooltipRectTransform.position = Vector3.Lerp(tooltipRectTransform.position, _trackedRectTransform.position, Time.deltaTime * 15);
        }
        CalculatePivotPosition();
    }

    private void HandleHide(InputAction.CallbackContext context)
    {
        ToggleTooltip();
    }

    public void Show(RectTransform _rectTransform)
    {
        if (_rectTransform == null) return;

        _trackedRectTransform = _rectTransform;

        if (IsHidden) return;

        if (canvasGroup.alpha == 0)
        {
            tooltipRectTransform.position = _rectTransform.position;
            canvasGroup.DOFade(1, 0.15f); 
        }
    }

    public void Hide()
    {
        canvasGroup.DOFade(0, 0.15f);
    }

    public void ToggleTooltip()
    {
        if (_trackedRectTransform == null) return;
        else
            Debug.Log("Tracked!");

        if (IsHidden)
            ShowFull();
        else
            HideFull();
    }

    private void HideFull()
    {
        if (IsAnimating) return;
        IsAnimating = true;
        IsHidden = true;

        currentSeq?.Kill();

        currentSeq = DOTween.Sequence();
        currentSeq.Append(canvasGroup.DOFade(0, 0.15f));
        currentSeq.Join(tooltipRectTransform.DOScaleY(0, 0.5f).SetEase(Ease.InQuad));
        currentSeq.OnComplete(() => { IsAnimating = false; });
    }

    private void ShowFull()
    {
        if (IsAnimating) return;
        IsAnimating = true;
        IsHidden = false;

        currentSeq?.Kill();

        tooltipRectTransform.localScale = new Vector3(1, 0, 1);
        canvasGroup.alpha = 0;

        tooltipRectTransform.position = _trackedRectTransform.position;

        currentSeq = DOTween.Sequence();
        currentSeq.Append(canvasGroup.DOFade(1, 0.15f));
        currentSeq.Join(tooltipRectTransform.DOScaleY(1, 0.5f).SetEase(Ease.OutQuad));
        currentSeq.OnComplete(() =>
        {
            IsAnimating = false;
        });

    }

    public void setTooltip(InventorySlot slot)
    {
        title.text = slot.item.itemName;
        amount.text = "Amount: " + slot.count.ToString();

        // stats
        attackGo.SetActive(slot.item.attack > 0);
        attackValue.text = slot.item.attack.ToString();
        defenceGo.SetActive(slot.item.defence > 0);
        defenceValue.text = slot.item.defence.ToString();
        hpGo.SetActive(slot.item.hp != 0);
        if(slot.item.hp > 0)
        {
            hpValue.text = "+" + slot.item.hp.ToString();
            hpValue.color = new Color(0, 0.6f, 0, 1);
        }
        else
        {
            hpValue.text = slot.item.hp.ToString();
            hpValue.color = new Color(0.6f, 0, 0, 1);
        }
            
        description.text = slot.item.GeneralDescription;

        // Consumable = 1 | weapon = 2 | Equipment = 3 | Accessory = 4 | Material = 5 |

        equipButton.gameObject.SetActive(true);
        switch (slot.item.category)
        {
            case ItemCategory.Weapon:
            case ItemCategory.Accessory:
            case ItemCategory.Equipment: equipButtonText.text = "Equip (J)"; break;
            case ItemCategory.Consumable: equipButtonText.text = "Consume (J)"; break;
            case ItemCategory.Material: equipButtonText.text = "Craft (J)"; break;
            default: equipButton.gameObject.SetActive(false); break;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
    }

    void CalculatePivotPosition()
    {
        Vector2 localPos;
        Vector2 screenPoint;

        // Convert the tooltip's world coordinates to screen coordinates
        screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, tooltipRectTransform.position);

        // Convert the tooltip's screen coordinates to the scrollArea's local coordinates
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            scrollAreaRect,
            screenPoint,
            Camera.main,
            out localPos
            );

        // Debug.Log("The local position of the Tooltip relative to the ScrollArea:" + localPos);

        Vector2 tooltipSize = tooltipRectTransform.sizeDelta;

        float rightEdgeDelta = localPos.x + tooltipSize.x / 2;
        // Debug.Log("rightEdgeDelta: " + rightEdgeDelta);
        bool flipX = rightEdgeDelta > scrollAreaRect.sizeDelta.x / 2;

        // Debug.Log("flipX = " + flipX);

        float bottomEdgeDelta = -localPos.y + tooltipSize.y / 2;
        // Debug.Log("bottomEdgeDelta: " + bottomEdgeDelta);
        // Debug.Log("scrollAreaRect.sizeDelta.y: " + scrollAreaRect.sizeDelta.y / 2);
        bool flipY = bottomEdgeDelta > scrollAreaRect.sizeDelta.y / 2;

        // Debug.Log("flipY = " + flipY);
        tooltipRectTransform.DOKill();
        tooltipRectTransform.DOPivot(new Vector2(flipX ? 1 : 0, flipY ? 0 : 1), 0.05f);
    }

    public void ResetToDefaultState()
    {
        if(currentSeq != null)
        {
            currentSeq.Kill();
        }
        canvasGroup.alpha = 0;
        _trackedRectTransform = null;
        IsAnimating = false;
    }
}
