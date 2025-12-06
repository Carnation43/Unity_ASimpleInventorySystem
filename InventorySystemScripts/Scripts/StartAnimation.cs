using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartAnimation : MonoBehaviour
{
    [SerializeField] private InputEventChannel inputChannel;

    [SerializeField] Canvas openingCanvas;
    [SerializeField] Canvas inventoryCanvas;
    [SerializeField] Transform logo;
    [SerializeField] TMP_Text text;
    [SerializeField] CanvasGroup openingCanvasGroup;
    [SerializeField] CanvasGroup logoCanvasGroup;
    [SerializeField] CanvasGroup textCanvasGroup;

    private Sequence seq;

    private void Start()
    {
        PlayAnimation();
    }

    void PlayAnimation()
    {
        if (inputChannel != null)
        {
            inputChannel.RaiseGlobalInputLockEvent(true);
        }

        openingCanvas.gameObject.SetActive(true);

        logoCanvasGroup.alpha = 0;
        textCanvasGroup.alpha = 0;

        seq?.Kill();

        seq = DOTween.Sequence();

        seq.Append(logoCanvasGroup.DOFade(1, 0.5f));
        seq.AppendInterval(2);
        seq.Append(logoCanvasGroup.DOFade(0, 0.5f));

        seq.Append(textCanvasGroup.DOFade(1, 0.5f));
        seq.AppendInterval(2);
        seq.Append(textCanvasGroup.DOFade(0, 0.5f));

        seq.Append(openingCanvasGroup.DOFade(0, 0.5f));

        seq.OnComplete(() => {
            openingCanvas.gameObject.SetActive(false);
            inputChannel?.RaiseGlobalInputLockEvent(false); 
        });
    }

    private void OnDestroy()
    {
        seq?.Kill();
    }
}
