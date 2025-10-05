using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;   
using InstanceResetToDefault;
using TMPro;
using UnityEngine.InputSystem;
using System;

public class DetailsContent : MonoBehaviour, IResettable
{
    public static DetailsContent instance;

    [Header("Listening To")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("Panels")]
    [SerializeField] private GameObject levelPanel1_Go;       // Upgrade panel
    [SerializeField] private GameObject detailsContent_Go;    // Item details panel

    [Header("Item Details Text Fields")] 
    [SerializeField] private TMP_Text itemNameText; 
    [SerializeField] private TMP_Text specificDescriptionText; 
    [SerializeField] private TMP_Text storyDescriptionText; 

    private CanvasGroup panel1_cg1;
    private CanvasGroup detailsContent_cg2;

    private Sequence seq;
    private Sequence _textUpdateSequence;
    private bool isAnmating = false;

    /// <summary>
    /// Whether currently in the details panel
    /// </summary>
    public bool IsChanged2Details { get; private set; } = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        panel1_cg1 = levelPanel1_Go.GetComponent<CanvasGroup>();
        detailsContent_cg2 = detailsContent_Go.GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if(SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.Register(this);
        }
        if (inputChannel != null)
        {
            // inputChannel.OnShowDetails += HandleShowDetails;
            inputChannel.OnSkip += HandleSkipDetails;
        }
        ResetToDefaultState();
    }

    private void OnDisable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.UnRegister(this);
        }
        if (inputChannel != null)
        {
            // inputChannel.OnShowDetails -= HandleShowDetails;
            inputChannel.OnSkip -= HandleSkipDetails;
        }
    }

    private void Start()
    {
 

        if (panel1_cg1 != null && detailsContent_cg2 != null)
        {
            panel1_cg1.alpha = 1;
            detailsContent_cg2.alpha = 0;
            detailsContent_Go.SetActive(false);
        }
    }

    private void HandleSkipDetails(InputAction.CallbackContext context)
    {
        if (IsChanged2Details)
        {
            if (SequenceController.instance != null)
            {
                SequenceController.instance.SkipCurrentTypewriter();
            }
        }
    }

    //private void HandleShowDetails(InputAction.CallbackContext context)
    //{
    //    ToggleByInput();
    //}

    /// <summary>
    /// Called by InputSystem callback, contains no input detection logic
    /// </summary>
    public void ToggleByInput()
    {
        if (isAnmating) return;
        // If currently in details panel -> unconditionally return to upgrade panel
        if (IsChanged2Details)
        {
            ShowPanel1_Content();
            return;
        }

        // If currently in upgrade panel -> check whether we can enter details panel
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && !TooltipViewController.instance.IsHidden)
        {
            var slotUI = selected.GetComponent<InventorySlotUI>();
            if (slotUI != null && slotUI.slot != null)
            {
                ShowPanel1_DetailsContent(slotUI.slot.item);
                return;
            }
        }
    }

    /// <summary>
    /// Show upgrade panel
    /// </summary>
    public void ShowPanel1_Content()
    {
        PlaySwitchAnimation(detailsContent_Go, levelPanel1_Go, detailsContent_cg2, panel1_cg1);
        IsChanged2Details = false;
    }

    /// <summary>
    /// Show item details panel
    /// </summary>
    public void ShowPanel1_DetailsContent(Item item)
    {
        SetupItemDetails(item);

        PlaySwitchAnimation(levelPanel1_Go, detailsContent_Go, panel1_cg1, detailsContent_cg2);
        IsChanged2Details = true;

        if (SequenceController.instance != null && !SequenceController.instance.IsPlaying)
        {
            SequenceController.instance.StartPlaySequence();
        }
    }

    /// <summary>
    /// Play switching animation between two panels
    /// </summary>
    private void PlaySwitchAnimation(GameObject fromPanel, GameObject toPanel, CanvasGroup fromCg, CanvasGroup toCg)
    {
        if (isAnmating) return;

        isAnmating = true;
        // Activate target panel, start invisible
        toPanel.SetActive(true);
        toCg.alpha = 0;

        // Kill previous sequence
        seq?.Kill();

        seq = DOTween.Sequence();

        /** DOTween Bug
        // Fade out old panel
        // This way of writing will cause the subsequent OnComplete to overwrite the OnComplete here,
        // because the OnComplete of Sequence is called after the entire animation ends,
        // while the OnComplete of Tween is called after the current animation ends
        //seq.Append(fromCg.DOFade(0, 0.2f))
        //   .OnComplete(() => { 
        //       fromPanel.SetActive(false); 
        //   });
        */

        // Fade out old panel
        seq.Append(fromCg.DOFade(0, 0.2f));
        seq.AppendCallback(() => { 
               fromPanel.SetActive(false); 
           });

        // Fade in new panel
        seq.Append(toCg.DOFade(1, 0.2f));

        toPanel.transform.localScale = Vector3.one * 0.95f;
        seq.Join(toPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutSine));

        seq.OnComplete(() => { 
            isAnmating = false; 
        });
    }

    public void SetupItemDetails(Item item)
    {
        if (item == null)
        {
            return;
        }

        bool playAnimation = !IsChanged2Details;

        if (itemNameText != null) itemNameText.text = item.itemName;
        if (specificDescriptionText != null) specificDescriptionText.text = item.specificDescription;
        if (storyDescriptionText != null) storyDescriptionText.text = item.storyDescription;

        if (playAnimation)
        {
            // avoid race condition
            if (specificDescriptionText != null) specificDescriptionText.maxVisibleCharacters = 0;
            if (storyDescriptionText != null) storyDescriptionText.maxVisibleCharacters = 0;

            // Reset and start the typewriter sequence
            if (SequenceController.instance != null)
            {
                SequenceController.instance.ResetToDefaultState();
                SequenceController.instance.StartPlaySequence();
            }
        }
        else
        {
            AnimateItemDetailsSwitch(item);
        }
    }
    private void AnimateItemDetailsSwitch(Item item)
    {
        _textUpdateSequence?.Kill(); 

        _textUpdateSequence = DOTween.Sequence();

        _textUpdateSequence.Append(detailsContent_cg2.DOFade(0, 0.2f).SetEase(Ease.OutCirc));

        _textUpdateSequence.AppendCallback(() => {
            if (itemNameText != null) itemNameText.text = item.itemName;
            if (specificDescriptionText != null)
            {
                specificDescriptionText.text = item.specificDescription;
                specificDescriptionText.maxVisibleCharacters = int.MaxValue;
            }
            if (storyDescriptionText != null)
            {
                storyDescriptionText.text = item.storyDescription;
                storyDescriptionText.maxVisibleCharacters = int.MaxValue;
            }

            if (SequenceController.instance != null)
            {
                SequenceController.instance.ShowAllInstantly();
            }
        });

        _textUpdateSequence.Append(detailsContent_cg2.DOFade(1, 0.2f).SetEase(Ease.InCirc));
    }

    /// <summary>
    /// reset to default state
    /// </summary>
    public void ResetToDefaultState()
    {
        if (isAnmating) return;

        if(panel1_cg1 != null && detailsContent_cg2 != null)
        {
            levelPanel1_Go.SetActive(true);
            panel1_cg1.alpha = 1;

            detailsContent_Go.SetActive(false);
            detailsContent_cg2.alpha = 0;
        }

        IsChanged2Details = false;

        // Clear item details when resetting state
        if (itemNameText != null) itemNameText.text = "";
        if (specificDescriptionText != null) specificDescriptionText.text = "";
        if (storyDescriptionText != null) storyDescriptionText.text = "";
    }
}
