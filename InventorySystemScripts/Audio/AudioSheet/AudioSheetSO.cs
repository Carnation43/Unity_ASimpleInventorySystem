using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Sheet")]
public class AudioSheetSO : ScriptableObject
{
    public AudioClip audioClip;
    public List<SoundCueMarker> cues;

    public SoundCueMarker GetCue(string name)
    {
        return cues.Find(cue => cue.cueName == name);
    }
}
