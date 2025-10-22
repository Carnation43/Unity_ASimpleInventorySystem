using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Listening To")]
    [SerializeField] private AudioCueEventChannel uiAudioChannel;
    [SerializeField] private MusicEventChannel musicChannel;

    [Header("Mixer Control")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string musicBaseVolumeParam = "MusicBaseVolume";
    [SerializeField] private string musicMenuVolumeParam = "MusicMenuVolume";

    [Header("Audio Players")]
    [SerializeField] private List<AudioSource> virtualChannels;
    [SerializeField] private AudioSource musicSource;
    // [SerializeField] private AudioSource _sheetPlayerSource;

    private AudioLowPassFilter _musicLowPassFilter;
    private int _channelIndex = 0;

    private void OnEnable()
    {
        if (uiAudioChannel != null)
        {
            uiAudioChannel.OnAudioCueRequested += PlayUIAudio;
        }

        if (musicChannel != null)
        {
            musicChannel.OnMusicActionRequested += HandleMusicAction;
        }

        mainMixer?.SetFloat(musicBaseVolumeParam, 0f);
        mainMixer?.SetFloat(musicMenuVolumeParam, 0f);

    }

    private void OnDisable()
    {
        if (uiAudioChannel != null)
        {
            uiAudioChannel.OnAudioCueRequested -= PlayUIAudio;
        }

        if (musicChannel != null)
        {
            musicChannel.OnMusicActionRequested -= HandleMusicAction;
        }
    }

    private void HandleMusicAction(MusicAction action, AudioCueSO musicCue)
    {
        if (musicSource == null) return;

        switch (action)
        {
            case MusicAction.Play:
                if (musicCue == null) break;
                float newBaseVolume = (musicCue.volume > 0) ? (20f * Mathf.Log10(musicCue.volume)) : -80f;
                mainMixer.SetFloat(musicBaseVolumeParam, newBaseVolume);

                musicSource.volume = 1f;

                musicSource.clip = musicCue.audioClip;
                musicSource.loop = musicCue.loop;
                musicSource.Play();
                break;

            case MusicAction.Stop:
                musicSource.Stop();
                break;

            case MusicAction.ApplyEffect:
                float currentValue;
                mainMixer.GetFloat(musicMenuVolumeParam, out currentValue);
                DOTween.To(() => currentValue, x => mainMixer.SetFloat(musicMenuVolumeParam, x), -10f, 0.5f).SetUpdate(true);
                break;

            case MusicAction.RemoveEffect:
                mainMixer.GetFloat(musicMenuVolumeParam, out currentValue);
                DOTween.To(() => currentValue, x => mainMixer.SetFloat(musicMenuVolumeParam, x), -0, 0.5f).SetUpdate(true);
                break;
        }
    }

    private void PlayUIAudio(AudioCueRequest request)
    {
        if (request.Cue == null || virtualChannels == null || virtualChannels.Count == 0)
            return;

        AudioSource channel = virtualChannels[_channelIndex];

        channel.clip = request.Cue.audioClip;
        channel.volume = request.Cue.volume;

        if (request.OverridePitch)
        {
            channel.pitch = request.Pitch;
        }
        else
        {
            channel.pitch = request.Cue.pitch;
        }

        channel.Play();

        _channelIndex = (_channelIndex + 1) % virtualChannels.Count;
    }

    #region [deprecated method]
    //public void PlayRandomCueFromSheet(AudioSheetSO sheet, float pitch)
    //{
    //    if (sheet == null || sheet.cues.Count == 0 || _sheetPlayerSource == null)
    //        return;

    //    SoundCueMarker randomMarker = sheet.cues[UnityEngine.Random.Range(0, sheet.cues.Count)];

    //    StartCoroutine(PlaySheetCueCoroutine(sheet.audioClip, randomMarker, pitch));
    //}

    //private IEnumerator PlaySheetCueCoroutine(AudioClip clip, SoundCueMarker marker, float pitch)
    //{
    //    _sheetPlayerSource.clip = clip;
    //    _sheetPlayerSource.pitch = pitch;

    //    _sheetPlayerSource.time = marker.startTime;

    //    _sheetPlayerSource.Play();

    //    yield return new WaitForSeconds(marker.duration / pitch);

    //    if (_sheetPlayerSource.clip == clip)
    //    {
    //        _sheetPlayerSource.Stop();
    //    }
    //}
    #endregion
}