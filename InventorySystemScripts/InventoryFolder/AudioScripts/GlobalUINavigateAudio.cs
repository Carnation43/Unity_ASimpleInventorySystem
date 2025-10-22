using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GlobalUINavigateAudio : MonoBehaviour
{
    [Header("Listening To")]
    [SerializeField] private AudioCueEventChannel audioChannel;

    [Header("Audio Clips")]
    [SerializeField] private AudioCueSO _defaultNavigateCue;
    [SerializeField] private AudioCueSO _defaultSubmitCue;

    private GameObject lastSelected;

    private void Start()
    {
        lastSelected = EventSystem.current.currentSelectedGameObject;
    }

    private void Update()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected != lastSelected)
        {
            if (currentSelected != null && UserInput.UIMoveInput != Vector2.zero)
            {
                RaiseAudioCue(_defaultNavigateCue);
            }

            lastSelected = currentSelected;
        }
    }

    private void RaiseAudioCue(AudioCueSO cue)
    {
        if (audioChannel != null && cue != null)
        {
            audioChannel.RaiseEvent(cue);
        }
    }
}
