using UnityEngine;

public enum SequenceType {
    Text,
    Image,
    // Other Components
}

[System.Serializable]
public class ItemSequence
{
    public SequenceType sequenceType;
    public GameObject target;
    public TypewriterEffect typewriterEffect;
    public float displayItem = 0.5f;
}
