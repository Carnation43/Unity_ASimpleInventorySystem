using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using InstanceResetToDefault;

public class TypewriterEffect : MonoBehaviour, IResettable
{
    private TMP_Text _textBox;

    // Basic Typewriter Functionality
    private int _currentVisibleCharacterIndex;
    private Coroutine _typewriterCoroutine;
    public bool _readyForNewText = true;

    private WaitForSeconds _simpleDelay;
    private WaitForSeconds _interpunctuationDelay;
    private WaitForSeconds _skipDelay;

    [Header("Typewriter settings")]
    [SerializeField] private float charactersPerSecond = 20;
    [SerializeField] private float interpunctuationDelay = 0.5f;

    [Header("Skip options")]
    [SerializeField] private bool quickSkip;
    [SerializeField] [Min(1)] private int skipSpeedup = 5;
    public bool CurrentlySkipping { get; private set; }


    // Event Functionality
    private WaitForSeconds _textboxFullEventDelay;
    [SerializeField] [Range(0.1f, 0.5f)] private float sendDoneDelay = 0.25f;

    public static event Action CompleteTextRevealed;
    public static event Action<char> CharacterRevealed;

    private void Awake()
    {
        _textBox = GetComponent<TMP_Text>();

        _simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
        _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);

        _skipDelay = new WaitForSeconds(1 / (charactersPerSecond * skipSpeedup));
        _textboxFullEventDelay = new WaitForSeconds(sendDoneDelay);
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

    /// <summary>
    /// This event is global. 
    /// If the scene requires controlling both text A and text B to display the typewriter effect simultaneously
    /// It will be triggered whenever the content of either text changes.
    /// This may cause the text to be executed repeatedly.
    /// </summary>

    //private void OnEnable()
    //{
    //    TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
    //}

    //private void OnDisable()
    //{
    //    TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(PrepareForNewText);
    //}

    public void PrepareForNewText(UnityEngine.Object obj)
    {
        if (!_readyForNewText)
            return;

        if (_textBox == null)
        {
            _textBox = GetComponent<TMP_Text>(); // retry to get component
            if (_textBox == null)
            {
                Debug.LogError("TMP_Text component is missing on the GameObject or _textBox could not be initialized.", this);
                return;
            }
        }

        // Avoid starting coroutines in an inactive state
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"Cannot start typewriter coroutine because GameObject '{gameObject.name}' is inactive.", this);
            return;
        }

        _readyForNewText = false;

        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        
        _textBox.maxVisibleCharacters = 0;
        _currentVisibleCharacterIndex = 0;
        _typewriterCoroutine = StartCoroutine(Typewriter());
    }

    private IEnumerator Typewriter()
    {
        // avoid race condition
        _textBox.ForceMeshUpdate();

        TMP_TextInfo textInfo = _textBox.textInfo;
        // _textBox.ForceMeshUpdate();
        Debug.Log("textInfo: " + textInfo.characterCount);
        while(_currentVisibleCharacterIndex < textInfo.characterCount + 1)
        {
            var lastCharacterIndex = textInfo.characterCount - 1;

            if (_currentVisibleCharacterIndex == lastCharacterIndex)
            {
                _textBox.maxVisibleCharacters++;
                yield return _textboxFullEventDelay;
                CompleteTextRevealed?.Invoke();
                _readyForNewText = true;
                yield break;
            }
            // Tags can be obtained
            char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;

            _textBox.maxVisibleCharacters++; // display next character

            // CurrentlySkipping here to ensure skipping without waiting for the punctuation delay.
            if (!CurrentlySkipping &&
                (character == '?' || character == '.' || character == ',' || character == ':' ||
                 character == ';' || character == '!' || character == '-')
                )
            {
                yield return _interpunctuationDelay;
            }
            yield return CurrentlySkipping ? _skipDelay : _simpleDelay;

            CharacterRevealed?.Invoke(character);
            _currentVisibleCharacterIndex++;
        }
    }

    public void ShowAllCharacters()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        if (_textBox != null)
        {
            _textBox.ForceMeshUpdate();
            _textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
        }

        _readyForNewText = true;
        CurrentlySkipping = false;
    }

    public void ToggleSkip()
    {
        if (_textBox.maxVisibleCharacters != _textBox.textInfo.characterCount - 1)
        {
            Skip();
        }
    }

    private void Skip()
    { 
        if (CurrentlySkipping)
        {
            return;
        }
       
        CurrentlySkipping = true;

        if (!quickSkip)
        {
            StartCoroutine(SkipSpeedupReset());
            return;
        }
        StopCoroutine(_typewriterCoroutine);
        _textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
        _readyForNewText = true;
        CompleteTextRevealed?.Invoke();
    }

    private IEnumerator SkipSpeedupReset()
    {
        yield return new WaitUntil(() => _textBox.maxVisibleCharacters == _textBox.textInfo.characterCount - 1);
        CurrentlySkipping = false;
    }

    public void ResetToDefaultState()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }
        _currentVisibleCharacterIndex = 0;
        _readyForNewText = true;
        CurrentlySkipping = false;
        if (_textBox != null)
        {
            _textBox.maxVisibleCharacters = 0;
        }
    }
}
