using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public abstract class BaseIconAnimationController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] protected GameObject imageBackground;
    [SerializeField] protected RectTransform rectTransform;

    protected CanvasGroup canvasGroup;
    protected bool selected;
    protected bool isPointerVisible;

    protected virtual void Awake()
    {
        selected = false;
        // 确保 rectTransform 已经被赋值，或者尝试获取
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        if (imageBackground != null)
        {
            canvasGroup = imageBackground.GetComponent<CanvasGroup>();
        }
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        selected = false;

        if (rectTransform != null)
        {
            rectTransform.DOKill();

            // default status
            rectTransform.DOScale(Vector2.one, 0.2f);
            rectTransform.DOLocalRotate(Vector3.zero, 0.25f);
        }
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();

            canvasGroup.DOFade(0, 0.2f);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        isPointerVisible = Cursor.visible && Cursor.lockState == CursorLockMode.None;
        if (!isPointerVisible)
        {
            return;
        }

        if (canvasGroup != null && !selected)
        {
            canvasGroup.DOFade(1, 0.2f);
            eventData.selectedObject = gameObject;
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!selected && canvasGroup != null)
        {
            canvasGroup.DOFade(0, 0.2f);
        }
    }

    public abstract void OnSelect(BaseEventData eventData);
}