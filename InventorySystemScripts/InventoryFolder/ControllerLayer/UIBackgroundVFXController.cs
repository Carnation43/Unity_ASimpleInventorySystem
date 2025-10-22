using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class UIBackgroundVFXController : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private RectTransform _canvasRectTransform;

    [Header("��˸Ƶ������")]
    [Tooltip("������˸֮�����С���ʱ��")]
    [SerializeField] private float minInterval = 0.1f;
    [Tooltip("������˸֮��������ʱ��")]
    [SerializeField] private float maxInterval = 0.5f;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        // ��ȡ����Canvas��RectTransform
        _canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        StartCoroutine(EmitParticlesRoutine());
    }

    private IEnumerator EmitParticlesRoutine()
    {
        // ֻҪ�ű��Ǽ���ģ���һֱѭ��
        while (true)
        {
            // 1. �ȴ�һ�������ʱ����
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // 2. ����һ����Canvas�ڵ����λ��
            float randomX = Random.Range(-_canvasRectTransform.rect.width / 2, _canvasRectTransform.rect.width / 2);
            float randomY = Random.Range(-_canvasRectTransform.rect.height / 2, _canvasRectTransform.rect.height / 2);
            Vector3 randomPosition = new Vector3(randomX, randomY, 0);

            // 3. �ڸ�λ�÷���һ������
            var emitParams = new ParticleSystem.EmitParams
            {
                position = randomPosition
            };
            _particleSystem.Emit(emitParams, 1); // ����1������
        }
    }
}