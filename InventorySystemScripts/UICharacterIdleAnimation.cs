using UnityEngine;
using UnityEngine.UI;

public class UICharacterIdleAnimation : MonoBehaviour
{
    [System.Serializable]
    public class AnimationClip
    {
        public string clipName; // 动画名称（如"Idle"、"Selected"）
        public Sprite[] frames; // 该动画的序列帧
        public float frameRate = 12f; // 该动画的帧率
        public bool loop = true; // 是否循环播放
    }

    [Header("动画列表")]
    public AnimationClip[] animationClips; // 可包含多个动画（待机、选中、对话等）

    [Header("默认设置")]
    public string defaultClipName = "Idle"; // 初始播放的动画名称

    private Image characterImage;
    private AnimationClip currentClip; // 当前播放的动画
    private int currentFrameIndex;
    private float frameInterval;
    private float timer;
    private bool isPlaying = true; // 是否正在播放

    void Awake()
    {
        characterImage = GetComponent<Image>();
        // 初始化默认动画
        SetAnimation(defaultClipName);
    }

    // 切换动画（外部可调用，如点击人物时切换）
    public void SetAnimation(string clipName)
    {
        // 查找对应名称的动画
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
            Debug.LogWarning($"未找到动画：{clipName}");
            return;
        }

        // 重置状态，切换到新动画
        currentClip = targetClip;
        currentFrameIndex = 0;
        frameInterval = 1f / currentClip.frameRate;
        timer = 0;
        isPlaying = true;

        // 显示第一帧
        if (currentClip.frames.Length > 0)
        {
            characterImage.sprite = currentClip.frames[0];
        }
    }

    // 暂停动画
    public void Pause() => isPlaying = false;

    // 继续动画
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

            // 处理循环/非循环
            if (currentFrameIndex >= currentClip.frames.Length)
            {
                if (currentClip.loop)
                {
                    currentFrameIndex = 0; // 循环：回到第一帧
                }
                else
                {
                    currentFrameIndex = currentClip.frames.Length - 1; // 不循环：停在最后一帧
                    isPlaying = false;
                }
            }

            characterImage.sprite = currentClip.frames[currentFrameIndex];
        }
    }
}
