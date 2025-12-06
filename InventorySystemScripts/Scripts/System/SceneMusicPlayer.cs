using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Broadcasting On")]
    [SerializeField] private MusicEventChannel musicChannel;

    [Header("Music To Play")]
    [SerializeField] private AudioCueSO sceneMusic;

    private void Start()
    {
        musicChannel?.RaisePlayEvent(sceneMusic);    
    }
}
