using UnityEngine;

public class ButtonEffectHelper : MonoBehaviour
{
    [Tooltip("�뽫����ť�µ���ЧGameObject��ק������")]
    [SerializeField] private GameObject selectionEffect;

    // ����������ᱻ��Selected����������
    public void ActivateEffect()
    {
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(true);
        }
    }

    // ����������ᱻ��Normal����������
    public void DeactivateEffect()
    {
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(false);
        }
    }

    // ȷ����Ϸ��ʼʱ��Ч�ǹرյ�
    private void Start()
    {
        DeactivateEffect();
    }
}