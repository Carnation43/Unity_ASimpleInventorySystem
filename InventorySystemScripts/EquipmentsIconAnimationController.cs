using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class EquipmentsIconAnimationController : BaseIconAnimationController
{
    public override void OnSelect(BaseEventData eventData)
    {
        if (canvasGroup == null || rectTransform == null || imageBackground == null)
        {
            return;
        }

        selected = true;

        // 通用动画：淡入和缩放
        canvasGroup.DOFade(1, 0.2f);
        rectTransform.DOScale(Vector2.one * 1.2f, 0.2f);

        // imageBackground 的旋转动画
        // 停止之前可能存在的动画，防止冲突
        imageBackground.transform.DOKill();
        imageBackground.transform.DOLocalRotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental); // 持续旋转
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData); // 调用基类的 OnDeselect 方法处理通用逻辑

        // 停止 imageBackground 的旋转动画并重置
        if (imageBackground != null)
        {
            imageBackground.transform.DOKill();
            imageBackground.transform.DOLocalRotate(Vector3.zero, 0.25f); // 旋转回初始状态
        }
    }
}