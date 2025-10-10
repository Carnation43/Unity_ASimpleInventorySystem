using UnityEngine;
using DG.Tweening;

/// <summary>
/// It listens to the OnMenuStateChanged event from the RadialMenuModel and plays animation.
/// </summary>
public class RadialMenuAnimator : MonoBehaviour
{
    private RadialMenuModel _model;
    private RadialMenuView _view;
    private RectTransform _radialPartRectTransform;
    private CanvasGroup _canvasGroup;
    private Sequence _currentSeq;
    
    public void Initialize(RadialMenuModel model, RadialMenuView view)
    {
        _model = model;
        _view = view;

        _model.OnMenuStateChanged += PlayOpenCloseAnimation;

        if (_view != null)
        {
            _radialPartRectTransform = _view.radialPartTransform.GetComponent<RectTransform>();
            _canvasGroup = _view.GetComponent<CanvasGroup>();
        }
    }

    private void OnDestroy()
    {
        if(_model != null)
        {
            _model.OnMenuStateChanged -= PlayOpenCloseAnimation;
        }
        if (_currentSeq != null)
        {
            _currentSeq.Kill();
        }
    }

    private void PlayOpenCloseAnimation(bool isOpen)
    {
        if (_radialPartRectTransform == null || _canvasGroup == null) return;
        _currentSeq?.Kill();

        if (isOpen)
        {
            Debug.Log("Play Open Animation");
            _radialPartRectTransform.localScale = Vector3.one * 0.5f;
            _canvasGroup.alpha = 0;

            _currentSeq = DOTween.Sequence();

            _currentSeq.Append(_radialPartRectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            _currentSeq.Join(_canvasGroup.DOFade(1f, 0.2f));
        }
        else
        {
            Debug.Log("Play Close Animation");

            _currentSeq = DOTween.Sequence();
            _currentSeq.Append(_radialPartRectTransform.DOScale(Vector3.one * 0.5f, 0.2f).SetEase(Ease.InBack));
            _currentSeq.Join(_canvasGroup.DOFade(0f, 0.15f));
            _currentSeq.OnComplete(() => { _view.DestroyMenuAndParts(); });
        }
    }
}