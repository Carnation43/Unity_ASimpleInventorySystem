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
            // �����е������¼�����ʱ����������Ҳ����һ����Ч
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
    /// ��ʼ����Ч����ء�
    /// �������������Ϸ��ʼʱ������Inspector�е����ã�һ���Դ�����������Ч��
    /// </summary>
    private void InitializeVfxPool()
    {
        _vfxPool = new Dictionary<CharacterVfxType, ParticleSystem>();
        Transform spawnPoint = vfxSpawnPoint != null ? vfxSpawnPoint : transform;

        foreach (var mapping in vfxMappings)
        {
            if (mapping.vfxPrefab != null && !_vfxPool.ContainsKey(mapping.type))
            {
                // 1. ����ʵ���������úø�����
                GameObject vfxInstance = Instantiate(mapping.vfxPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

                // 2. ��ȡ����ϵͳ���
                ParticleSystem particleSystem = vfxInstance.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    // 3. ��ʵ���������
                    _vfxPool.Add(mapping.type, particleSystem);

                    // 4. Ĭ������Ϊ���״̬
                    vfxInstance.SetActive(false);
                }
                else
                {
                    Debug.LogWarning($"��ЧԤ���� '{mapping.vfxPrefab.name}' ȱ�ٸ�����ParticleSystem������޷��������ء�");
                }
            }
        }
    }

    private void PlayLevelUpVfx()
    {
        // �յ������㲥��ֱ�ӵ��ò�����Ч�ķ���
        PlayVfx(CharacterVfxType.LevelUp);
    }

    private void PlayVfx(CharacterVfxType vfxType)
    {
        if (_vfxPool.TryGetValue(vfxType, out ParticleSystem particleSystemToPlay))
        {
            // 1. ������Ч����
            particleSystemToPlay.gameObject.SetActive(true);

            // 2. ����������Ч
            particleSystemToPlay.Play();
        }
        else
        {
            Debug.LogWarning($"�������δ�ҵ�����Ϊ '{vfxType}' ����Ч��");
        }
    }
}
