using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/ Audio Cue")]
public class AudioCueSO : ScriptableObject
{
    public AudioClip audioClip;

    public bool loop = false;

    [Range(0, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;
}
