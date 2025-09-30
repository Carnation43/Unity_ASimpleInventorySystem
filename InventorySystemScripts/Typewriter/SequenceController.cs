using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InstanceResetToDefault;

public class SequenceController : MonoBehaviour, IResettable
{
    public static SequenceController instance;

    [SerializeField] List<ItemSequence> itemSequences;
    private TypewriterEffect _currentActiveTypewriter;
    private Coroutine _playSequenceCoroutine;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.Register(this);
        }
    }

    private void OnDisable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.UnRegister(this);
        }
    }

    public void StartPlaySequence()
    {
        if (_playSequenceCoroutine != null)
        {
            StopCoroutine(_playSequenceCoroutine);
        }
        IsPlaying = false;
        _playSequenceCoroutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        IsPlaying = true;

        foreach(var item in itemSequences)
        {
            item.target.SetActive(true);

            if(item.sequenceType == SequenceType.Text && item.typewriterEffect != null)
            {
                yield return null;
                // set current typewriter
                _currentActiveTypewriter = item.typewriterEffect;
                // start typewriter
                item.typewriterEffect.PrepareForNewText(item.typewriterEffect);

                // wait for typewriter completed
                yield return new WaitUntil(() => item.typewriterEffect._readyForNewText);
                _currentActiveTypewriter = null;
            }
            else
            {
                yield return new WaitForSeconds(item.displayItem);
            }
        }
        // Debug: Log itemSequences content
        foreach (var item in itemSequences)
        {
            Debug.Log($"ItemSequence target: {item.target?.name}, typewriterEffect: {item.typewriterEffect?.gameObject.name}");
        }
        IsPlaying = false;
        _playSequenceCoroutine = null;
    }

    // skipping the text that is currently being typed
    public void SkipCurrentTypewriter()
    {
        if (_currentActiveTypewriter != null && !_currentActiveTypewriter._readyForNewText)
        {
            _currentActiveTypewriter.ToggleSkip();
        }
    }


    // Summary: Resets the SequenceController to its default state.
    // This method is called when the inventory menu is closed or when a full system reset is required.
    // It ensures that any ongoing sequence playback is stopped, all associated UI elements (item.target) are deactivated,
    // and internal states (_currentActiveTypewriter, IsPlaying) are cleared.
    // This prevents issues like coroutines running on inactive GameObjects and ensures a clean start for subsequent sequence playback.
    public void ResetToDefaultState()
    {
        if (_playSequenceCoroutine != null) 
        {
            StopCoroutine(_playSequenceCoroutine);
            _playSequenceCoroutine = null; 
        }

        foreach (var item in itemSequences) 
        {
            if (item.target != null) 
            {
                item.target.SetActive(false); 
            }
            if (item.sequenceType == SequenceType.Text && item.typewriterEffect != null)
            {
                item.typewriterEffect.ResetToDefaultState();
            }
        }

        _currentActiveTypewriter = null;
        IsPlaying = false;
    }
}
