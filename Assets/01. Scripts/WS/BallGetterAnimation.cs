using DG.Tweening;
using TMPro;
using UnityEngine;

public class WS_BallGetterAnimation : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private CanvasGroup _cg;

    [Header("Canvas")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _parentRect;

    [Header("Move")]
    [SerializeField] private float _moveY = 45f;
    [SerializeField] private float _duration = 0.45f;

    [Header("Scale")]
    [SerializeField] private float _startScale = 0.8f;
    [SerializeField] private float _punchScale = 1.2f;

    private Sequence _seq;
    private RectTransform _rectTr;

    private void Awake()
    {
        _rectTr = _text.rectTransform;

        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();

        if (_parentRect == null && _rectTr != null && _rectTr.parent != null)
            _parentRect = _rectTr.parent as RectTransform;

        ForceHide();
    }

    public void Play(string ballName, Vector3 worldPosition)
    {
        if (_text == null || _cg == null || _canvas == null || _parentRect == null)
            return;

        if (_rectTr == null)
            _rectTr = _text.rectTransform;

        _seq?.Kill();

        _text.text = $"<{ballName} +1>";

        Vector2 anchoredPos;
        if (!TryConvertWorldToAnchoredPosition(worldPosition, out anchoredPos))
            return;

        _rectTr.anchoredPosition = anchoredPos;
        _rectTr.localScale = Vector3.one * _startScale;

        _cg.alpha = 1f;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;

        _seq = DOTween.Sequence();
        _seq.SetLink(gameObject);

        _seq.Append(_rectTr.DOScale(_punchScale, 0.12f).SetEase(Ease.OutBack));
        _seq.Join(_rectTr.DOAnchorPosY(anchoredPos.y + _moveY, _duration).SetEase(Ease.OutCubic));
        _seq.Append(_rectTr.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
        _seq.AppendInterval(0.05f);

        _seq.OnKill(() =>
        {
            if (this == null || _cg == null)
                return;

            ForceHide();
        });

        _seq.OnComplete(() =>
        {
            ForceHide();
        });
    }

    private bool TryConvertWorldToAnchoredPosition(Vector3 worldPosition, out Vector2 anchoredPosition)
    {
        anchoredPosition = Vector2.zero;

        Camera cam = null;
        if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = _canvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentRect,
            screenPoint,
            cam,
            out anchoredPosition
        );
    }

    private void ForceHide()
    {
        if (_cg != null)
        {
            _cg.alpha = 0f;
            _cg.interactable = false;
            _cg.blocksRaycasts = false;
        }

        if (_rectTr != null)
        {
            _rectTr.localScale = Vector3.one;
        }
    }

    private void OnDestroy()
    {
        _seq?.Kill();
    }
}