using UnityEngine;
using UnityEngine.UI;

public class UICharacterIdleAnimation : MonoBehaviour
{
    [System.Serializable]
    public class AnimationClip
    {
        public string clipName; // �������ƣ���"Idle"��"Selected"��
        public Sprite[] frames; // �ö���������֡
        public float frameRate = 12f; // �ö�����֡��
        public bool loop = true; // �Ƿ�ѭ������
    }

    [Header("�����б�")]
    public AnimationClip[] animationClips; // �ɰ������������������ѡ�С��Ի��ȣ�

    [Header("Ĭ������")]
    public string defaultClipName = "Idle"; // ��ʼ���ŵĶ�������

    private Image characterImage;
    private AnimationClip currentClip; // ��ǰ���ŵĶ���
    private int currentFrameIndex;
    private float frameInterval;
    private float timer;
    private bool isPlaying = true; // �Ƿ����ڲ���

    void Awake()
    {
        characterImage = GetComponent<Image>();
        // ��ʼ��Ĭ�϶���
        SetAnimation(defaultClipName);
    }

    // �л��������ⲿ�ɵ��ã���������ʱ�л���
    public void SetAnimation(string clipName)
    {
        // ���Ҷ�Ӧ���ƵĶ���
        AnimationClip targetClip = null;
        foreach (var clip in animationClips)
        {
            if (clip.clipName == clipName)
            {
                targetClip = clip;
                break;
            }
        }

        if (targetClip == null)
        {
            Debug.LogWarning($"δ�ҵ�������{clipName}");
            return;
        }

        // ����״̬���л����¶���
        currentClip = targetClip;
        currentFrameIndex = 0;
        frameInterval = 1f / currentClip.frameRate;
        timer = 0;
        isPlaying = true;

        // ��ʾ��һ֡
        if (currentClip.frames.Length > 0)
        {
            characterImage.sprite = currentClip.frames[0];
        }
    }

    // ��ͣ����
    public void Pause() => isPlaying = false;

    // ��������
    public void Resume() => isPlaying = true;

    void Update()
    {
        if (!isPlaying || currentClip == null || currentClip.frames.Length <= 1)
            return;

        timer += Time.deltaTime;

        if (timer >= frameInterval)
        {
            timer -= frameInterval;
            currentFrameIndex++;

            // ����ѭ��/��ѭ��
            if (currentFrameIndex >= currentClip.frames.Length)
            {
                if (currentClip.loop)
                {
                    currentFrameIndex = 0; // ѭ�����ص���һ֡
                }
                else
                {
                    currentFrameIndex = currentClip.frames.Length - 1; // ��ѭ����ͣ�����һ֡
                    isPlaying = false;
                }
            }

            characterImage.sprite = currentClip.frames[currentFrameIndex];
        }
    }
}
