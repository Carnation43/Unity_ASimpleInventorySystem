using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPanelVfxPlayer : MonoBehaviour
{
    [Header("Listening To")]
    [SerializeField] private CharacterPanelVfxChannel characterVfxChannel;
    [SerializeField] private CharacterStatsEventChannel statsChannel;

    [Header("VFX Settings")]
    [SerializeField] private Transform vfxSpawnPoint;
    [SerializeField] private List<VfxMapping> vfxMappings;

    [System.Serializable]
    public class VfxMapping
    {
        public CharacterVfxType type;
        public GameObject vfxPrefab;
    }

    private Dictionary<CharacterVfxType, ParticleSystem> _vfxPool;

    private void Awake()
    {
        InitializeVfxPool();
    }

    private void OnEnable()
    {
        if (characterVfxChannel != null)
        {
            characterVfxChannel.OnVfxRequested += PlayVfx;
        }
        if (statsChannel != null)
        {
            // 当已有的升级事件发生时，我们让它也触发一个特效
            statsChannel.OnStatsLeveledUp += PlayLevelUpVfx;
        }
    }

    private void OnDisable()
    {
        if (characterVfxChannel != null)
        {
            characterVfxChannel.OnVfxRequested -= PlayVfx;
        }
        if (statsChannel != null)
        {
            statsChannel.OnStatsLeveledUp -= PlayLevelUpVfx;
        }
    }

    /// <summary>
    /// 初始化特效对象池。
    /// 这个方法会在游戏开始时，根据Inspector中的配置，一次性创建好所有特效。
    /// </summary>
    private void InitializeVfxPool()
    {
        _vfxPool = new Dictionary<CharacterVfxType, ParticleSystem>();
        Transform spawnPoint = vfxSpawnPoint != null ? vfxSpawnPoint : transform;

        foreach (var mapping in vfxMappings)
        {
            if (mapping.vfxPrefab != null && !_vfxPool.ContainsKey(mapping.type))
            {
                // 1. 创建实例，并设置好父物体
                GameObject vfxInstance = Instantiate(mapping.vfxPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

                // 2. 获取粒子系统组件
                ParticleSystem particleSystem = vfxInstance.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    // 3. 将实例存入池中
                    _vfxPool.Add(mapping.type, particleSystem);

                    // 4. 默认设置为不活动状态
                    vfxInstance.SetActive(false);
                }
                else
                {
                    Debug.LogWarning($"特效预制体 '{mapping.vfxPrefab.name}' 缺少根级的ParticleSystem组件，无法加入对象池。");
                }
            }
        }
    }

    private void PlayLevelUpVfx()
    {
        // 收到升级广播后，直接调用播放特效的方法
        PlayVfx(CharacterVfxType.LevelUp);
    }

    private void PlayVfx(CharacterVfxType vfxType)
    {
        if (_vfxPool.TryGetValue(vfxType, out ParticleSystem particleSystemToPlay))
        {
            // 1. 激活特效物体
            particleSystemToPlay.gameObject.SetActive(true);

            // 2. 播放粒子特效
            particleSystemToPlay.Play();
        }
        else
        {
            Debug.LogWarning($"对象池中未找到类型为 '{vfxType}' 的特效。");
        }
    }
}
