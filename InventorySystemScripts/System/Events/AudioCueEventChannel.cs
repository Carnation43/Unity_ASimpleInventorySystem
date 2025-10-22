using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AudioCueRequest
{
    public AudioCueSO Cue;
    public bool OverridePitch;
    public float Pitch;
}

[CreateAssetMenu(menuName = "Events/Audio Cue Event Channel")]
public class AudioCueEventChannel : ScriptableObject
{
    public event Action<AudioCueRequest> OnAudioCueRequested;

    /// <summary>
    /// Default Raise Event
    /// </summary>
    /// <param name="audioCue"></param>
    public void RaiseEvent(AudioCueSO audioCue)
    {
        var request = new AudioCueRequest
        {
            Cue = audioCue,
            OverridePitch = false
        };

        Broadcast(request);
    }

    public void RaiseEventWithPitch(AudioCueSO audioCue, float pitch)
    {
        var request = new AudioCueRequest
        {
            Cue = audioCue,
            OverridePitch = true,
            Pitch = pitch,
        };

        Broadcast(request);
    }

    private void Broadcast(AudioCueRequest request)
    {
        if (OnAudioCueRequested != null)
        {
            OnAudioCueRequested?.Invoke(request);
        }
        else
        {
            Debug.LogWarning("No Listner");
        }
    }
}
