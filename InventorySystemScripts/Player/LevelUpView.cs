using InstanceResetToDefault;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelUpView : MonoBehaviour, IResettable
{
    [Header("Data Source")]
    [SerializeField] private CharacterStatsData _statsData;

    [Header("Listening To")]
    [SerializeField] private InputEventChannel inputChannel;

    [Header("UI References")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text nextLevelText;
    [SerializeField] private TMP_Text currentGlowsText;
    [SerializeField] private TMP_Text requiredGlowsText;
    [SerializeField] private List<AttributeRowUI> rows;
    [SerializeField] private Button confirmButton;

    [Header("Dependencies")]
    [SerializeField] private LevelUpTooltip levelUpTooltip;

    private Animator confirmButtonAnimator;
    private Dictionary<StatType, int> previewPoints = new Dictionary<StatType, int>();
    private int totalPointsToAdd = 0;
    private int totalPreviewCost = 0;
    private int currentlySelectedRow = -1; // -1 indicates that no elements are selected
    private Color highlightArrowColor = new Color(0.6f, 0, 0, 1);
    private Color defaultArrowColor = new Color(0, 0, 0, 1);

    private bool _isInputLocked = false;

    private void Awake()
    {
        // Ensure that the list to be added is null.
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            previewPoints[type] = 0;
        }

        if (confirmButton != null)
        {
            confirmButtonAnimator = confirmButton.GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.Register(this);
        }

        if (CharacterStatsController.instance != null)
        {
            CharacterStatsController.instance.OnStatsUpdated += HandleStatsUpdated;
        }

        if (inputChannel != null)
        {
            inputChannel.OnNavigate += HandleNavigateInput;
            inputChannel.OnNavigateLeft += HandleModifyPreviewInput;
            inputChannel.OnNavigateRight += HandleModifyPreviewInput;
            inputChannel.OnConfirm += HandleConfirmInput;
            inputChannel.OnGlobalInputLock += HandleInputLock;
        }
    }

    private void OnDisable()
    {
        if (SingletonResetManager.Instance != null)
        {
            SingletonResetManager.Instance.UnRegister(this);
        }

        if (CharacterStatsController.instance != null)
        {
            CharacterStatsController.instance.OnStatsUpdated -= HandleStatsUpdated;
        }

        if (inputChannel != null)
        {
            inputChannel.OnNavigate -= HandleNavigateInput;
            inputChannel.OnNavigateLeft -= HandleModifyPreviewInput;
            inputChannel.OnNavigateRight -= HandleModifyPreviewInput;
            inputChannel.OnConfirm -= HandleConfirmInput;
            inputChannel.OnGlobalInputLock -= HandleInputLock;
        }
    }
    private void HandleInputLock(bool isLocked)
    {
        _isInputLocked = isLocked;
    }


    // Handle Q/E input
    private void HandleModifyPreviewInput(InputAction.CallbackContext context)
    {
        if (_isInputLocked || !context.performed || MenuController.instance == null || MenuController.instance.currentFocus != MenuController.MenuFocus.LevelUp)
        {
            return;
        }

        // It can only be triggered when a certain row is selected.
        if (currentlySelectedRow < 0) return;

        StatType type = (StatType)currentlySelectedRow;

        if (context.action.name == "NavigateRight")
        {
            // Predict whether the current experience points
            // meet the requirements for the next level up.
            int costForNextLevel = LevelManager.instance.CalculateCostForLevels(_statsData.level + totalPointsToAdd, 1);

            if (_statsData.currentGlows >= totalPreviewCost + costForNextLevel)
            {
                previewPoints[type]++;
                totalPointsToAdd++;
            }
        }
        else if (context.action.name == "NavigateLeft")
        {
            if(previewPoints[type] > 0)
            {
                previewPoints[type]--;
                totalPointsToAdd--;
            }
        }

        totalPreviewCost = LevelManager.instance.CalculateCostForLevels(_statsData.level, totalPointsToAdd);
        UpdateAllUI();
    }

    // confirm
    private void HandleConfirmInput(InputAction.CallbackContext context)
    {
        if (_isInputLocked || !context.performed) return;

        if (totalPointsToAdd > 0)
        {
            if (confirmButtonAnimator != null)
            {
                confirmButtonAnimator.SetTrigger("Pressed");
            }
            levelUpTooltip.ShowDialog(totalPreviewCost, PerformUpgrade);
        }
    }
    public void PerformUpgrade()
    {
        LevelManager.instance.ConfirmMultiLevelUp(previewPoints, totalPreviewCost);
    }
    // Handle W/S input event
    public void HandleNavigateInput(InputAction.CallbackContext context)
    {
        // only focus on current frame, and ignore started and canceled
        // only handle input in level up panel
        if (_isInputLocked || !context.performed || MenuController.instance == null || 
            MenuController.instance.currentFocus != MenuController.MenuFocus.LevelUp)
        {
            return;
        }

        Vector2 move = context.ReadValue<Vector2>();
        if (move.y > 0.5f) SelectRow(currentlySelectedRow - 1); // up
        else if (move.y < -0.5f) SelectRow(currentlySelectedRow + 1); // down
     }

    private void SelectRow(int currentRow)
    {

        currentRow = Mathf.Clamp(currentRow, -1, rows.Count - 1);

        if (currentRow != currentlySelectedRow)
        {
            currentlySelectedRow = currentRow;
            UpdateAllUI();
        }
    }

    // Refresh view panel after leveling up
    private void HandleStatsUpdated()
    {
        ResetPreview();
    }

    private void ResetPreview()
    {
        totalPointsToAdd = 0;
        totalPreviewCost = 0;
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            previewPoints[type] = 0;
        }
        // reset
        UpdateAllUI();
    }

    // Refresh All UI in level panel
    private void UpdateAllUI()
    {
        levelText.text = _statsData.level.ToString();
        nextLevelText.text = (_statsData.level + totalPointsToAdd).ToString();
        currentGlowsText.text = _statsData.currentGlows.ToString();
        requiredGlowsText.text = totalPreviewCost.ToString();

        for (int i = 0; i < rows.Count; i++)
        {
            StatType type = (StatType)i;
            int baseValue = GetStatBaseValue(type);
            UpdateStatRow(rows[i], baseValue, previewPoints[type], i == currentlySelectedRow);
        }

        confirmButton.interactable = totalPointsToAdd > 0;
    }

    // Update the UI effect of current row
    private void UpdateStatRow(AttributeRowUI row, int baseValue, int points, bool isSelected)
    {
        row.currentLevelText.text = baseValue.ToString();
        row.nextLevelText.text = (baseValue + points).ToString();
        row.selectedImage.gameObject.SetActive(isSelected);

        row.selectedIndicators.SetActive(isSelected);

        if (isSelected)
        {
            row.selectedIndicators.SetActive(true);
            int costForNextPoint = LevelManager.instance.CalculateCostForLevels(_statsData.level + totalPointsToAdd, 1);
            bool canAddPoint = _statsData.currentGlows >= totalPreviewCost + costForNextPoint;

            bool canRemovePoint = points > 0;

            row.rightArrow.color = canAddPoint ? highlightArrowColor : defaultArrowColor;
            row.leftArrow.color = canRemovePoint ? highlightArrowColor : defaultArrowColor;
        }
        else
        {
            row.leftArrow.color = defaultArrowColor;
            row.rightArrow.color = defaultArrowColor;
        }
    }

    // get current attribute values
    private int GetStatBaseValue(StatType type)
    {
        switch (type)
        {
            case StatType.Vigor: return _statsData.vigor;
            case StatType.Attunement: return _statsData.attunement;
            case StatType.Endurance: return _statsData.endurance;
            case StatType.Vitality: return _statsData.vitality;
            case StatType.Strength: return _statsData.strength;
            case StatType.Dexterity: return _statsData.dexterity;
            case StatType.Intelligence: return _statsData.intelligence;
            case StatType.Faith: return _statsData.faith;
            case StatType.Luck: return _statsData.luck;
            default: return 0;
        }
    }

    // The following two methods are made public to the MenuController for focus control.
    public void TakeFocus()
    {
        SelectRow(0);
    }

    public void LoseFocus()
    {
        SelectRow(-1);
    }

    public void ResetToDefaultState()
    {
        currentlySelectedRow = -1; 
        ResetPreview();
    }
}
