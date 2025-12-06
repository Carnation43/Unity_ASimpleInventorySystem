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

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        if (imageBackground != null)
        {
            canvasGroup = imageBackground.GetComponent<CanvasGroup>();
        }
    }

    protected virtual void OnDisable()
    {
        // OnDeselect is not called when the object is disabled.
        // This ensures that animations stop and reset immediately
        // so the item looks clean the next time the menu opens.

        selected = false;

        if (rectTransform != null)
        {
            rectTransform.DOKill();

            rectTransform.localScale = Vector2.one;
            rectTransform.localRotation = Quaternion.identity;
        }
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0; 
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

    protected virtual void OnDestroy()
    {
        rectTransform?.DOKill();

        canvasGroup?.DOKill();

        selected = false;
    }
}