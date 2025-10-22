using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MusicAction
{
    Play,
    Stop,
    ApplyEffect,
    RemoveEffect
}

[CreateAssetMenu(menuName = "Events/Music Event Channel")]
public class MusicEventChannel : ScriptableObject
{
    public event Action<MusicAction, AudioCueSO> OnMusicActionRequested;

    public void RaisePlayEvent(AudioCueSO musicCue)
    {
        OnMusicActionRequested?.Invoke(MusicAction.Play, musicCue);
    }

    public void RaiseStopEvent()
    {
        OnMusicActionRequested?.Invoke(MusicAction.Stop, null);
    }

    public void RaiseApplyEffectEvent()
    {
        OnMusicActionRequested?.Invoke(MusicAction.ApplyEffect, null);
    }

    public void RaiseRemoveEffectEvent()
    {
        OnMusicActionRequested?.Invoke(MusicAction.RemoveEffect, null);
    }
}
