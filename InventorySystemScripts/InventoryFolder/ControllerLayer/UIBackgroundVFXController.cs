using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class UIBackgroundVFXController : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private RectTransform _canvasRectTransform;

    [Header("闪烁频率设置")]
    [Tooltip("两次闪烁之间的最小间隔时间")]
    [SerializeField] private float minInterval = 0.1f;
    [Tooltip("两次闪烁之间的最大间隔时间")]
    [SerializeField] private float maxInterval = 0.5f;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        // 获取父级Canvas的RectTransform
        _canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        StartCoroutine(EmitParticlesRoutine());
    }

    private IEnumerator EmitParticlesRoutine()
    {
        // 只要脚本是激活的，就一直循环
        while (true)
        {
            // 1. 等待一个随机的时间间隔
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // 2. 计算一个在Canvas内的随机位置
            float randomX = Random.Range(-_canvasRectTransform.rect.width / 2, _canvasRectTransform.rect.width / 2);
            float randomY = Random.Range(-_canvasRectTransform.rect.height / 2, _canvasRectTransform.rect.height / 2);
            Vector3 randomPosition = new Vector3(randomX, randomY, 0);

            // 3. 在该位置发射一个粒子
            var emitParams = new ParticleSystem.EmitParams
            {
                position = randomPosition
            };
            _particleSystem.Emit(emitParams, 1); // 发射1个粒子
        }
    }
}