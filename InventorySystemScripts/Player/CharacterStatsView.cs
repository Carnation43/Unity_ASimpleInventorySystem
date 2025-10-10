using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterStatsView : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField] private CharacterStatsData _statsData;

    [Header("UI Referneces")]
    [SerializeField] private TMP_Text vigorText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private Slider defenceSlider;

    private Tweener _defenceTween;

    private void OnEnable()
    {
        if (CharacterStatsController.instance != null)
        {
            CharacterStatsController.instance.OnStatsUpdated += UpdateStatsUI;
        }
    }

    private void OnDisable()
    {
        if (CharacterStatsController.instance != null)
        {
            CharacterStatsController.instance.OnStatsUpdated -= UpdateStatsUI;
        }
    }

    private void Start()
    {
        if (CharacterStatsController.instance != null)
        {
            CharacterStatsController.instance.OnStatsUpdated += UpdateStatsUI;
            UpdateStatsUI();
        }

        Initialize();
    }

    private void UpdateStatsUI()
    {
        if (_statsData == null) return;

        attackText.text = _statsData.attackPower.ToString();
        vigorText.text = $"{_statsData.currentHealth} / {_statsData.maxHealth}";

        if(defenceSlider != null)
        {
            _defenceTween?.Kill();

            _defenceTween = defenceSlider.DOValue(_statsData.physicalDefence, 0.5f);
        }
    }

    private void Initialize()
    {
        Debug.Log("UpdateStatsUI");
        if (_statsData == null) return;

        if (attackText != null)
        {
            attackText.text = _statsData.attackPower.ToString();
        }

        if (vigorText != null)
        {
            vigorText.text = $"{_statsData.currentHealth} / {_statsData.maxHealth}";
        }

        if (defenceSlider != null)
        {
            defenceSlider.maxValue = _statsData.maxDefence;
            defenceSlider.value = _statsData.physicalDefence;
        }
    }
}
