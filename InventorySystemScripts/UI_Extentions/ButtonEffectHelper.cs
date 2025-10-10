using UnityEngine;

public class ButtonEffectHelper : MonoBehaviour
{
    [Tooltip("请将您按钮下的特效GameObject拖拽到这里")]
    [SerializeField] private GameObject selectionEffect;

    // 这个方法将会被“Selected”动画调用
    public void ActivateEffect()
    {
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(true);
        }
    }

    // 这个方法将会被“Normal”动画调用
    public void DeactivateEffect()
    {
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(false);
        }
    }

    // 确保游戏开始时特效是关闭的
    private void Start()
    {
        DeactivateEffect();
    }
}