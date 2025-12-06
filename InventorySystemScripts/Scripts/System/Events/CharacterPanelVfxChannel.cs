using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Character Panel VFX Event Channel")]
public class CharacterPanelVfxChannel : ScriptableObject
{
    public event Action<CharacterVfxType> OnVfxRequested;

    public void RaiseEvent(CharacterVfxType vfxType)
    {
        OnVfxRequested?.Invoke(vfxType);
    }
}
