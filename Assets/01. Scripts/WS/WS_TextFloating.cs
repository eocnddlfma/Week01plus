using TMPro;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(TMP_Text))]
public class WS_TextFloating : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float _moveY = 60f;
    [SerializeField] private float _duration = 0.7f;

    [Header("Punch")]
    [SerializeField] private float _punchScale = 0.3f;
    [SerializeField] private float _punchDuration = 0.18f;
    [SerializeField] private int _vibrato = 8;
    [SerializeField] private float _elasticity = 0.8f;

    [Header("Fade")]
    [SerializeField] private float _fadeStartDelay = 0.1f;

    [Header("Critical")]
    [SerializeField] private float _criticalThreshold = 0.999f;
    [SerializeField] private Color _criticalColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float _criticalMoveY = 90f;
    [SerializeField] private float _criticalDuration = 0.55f;
    [SerializeField] private float _criticalStartScaleMultiplier = 1.25f;
    [SerializeField] private float _criticalPunchScale = 0.55f;
    [SerializeField] private float _criticalPunchDuration = 0.22f;
    [SerializeField] private int _criticalVibrato = 10;
    [SerializeField] private float _criticalElasticity = 0.95f;
    [SerializeField] private float _criticalFadeStartDelay = 0.05f;
    [SerializeField] private float _criticalPrePopYOffset = -20f;

    private TMP_Text _tmpText;
    private RectTransform _rectTransform;
    private Sequence _sequence;

    private Vector3 _originScale;
    private Color _originColor;

    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
        _rectTransform = GetComponent<RectTransform>();

        _originScale = _rectTransform.localScale;
        _originColor = _tmpText.color;
    }

    private void OnDisable()
    {
        _sequence?.Kill();
        _sequence = null;
    }

    public void Play(int damage, Vector2 anchoredPosition, bool isCritical)
    {
        if (_tmpText == null)
            _tmpText = GetComponent<TMP_Text>();

        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        _sequence?.Kill();
        _sequence = null;

        gameObject.SetActive(true);

        _tmpText.text = damage.ToString();

        if (isCritical)
        {
            PlayCritical(anchoredPosition);
        }
        else
        {
            PlayNormal(anchoredPosition);
        }
    }

    private void PlayNormal(Vector2 anchoredPosition)
    {
        _tmpText.color = new Color(_originColor.r, _originColor.g, _originColor.b, 1f);

        _rectTransform.anchoredPosition = anchoredPosition;
        _rectTransform.localScale = _originScale;

        Vector2 startPos = _rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * _moveY;

        _sequence = DOTween.Sequence();
        _sequence.Append(_rectTransform.DOAnchorPos(endPos, _duration).SetEase(Ease.OutCubic));
        _sequence.Join(_rectTransform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, _vibrato, _elasticity));
        _sequence.Join(_tmpText.DOFade(0f, _duration - _fadeStartDelay).SetDelay(_fadeStartDelay).SetEase(Ease.InQuad));

        _sequence.OnComplete(FinishEffect);
    }

    private void PlayCritical(Vector2 anchoredPosition)
    {
        _tmpText.color = new Color(_criticalColor.r, _criticalColor.g, _criticalColor.b, 1f);

        Vector2 startPos = anchoredPosition + Vector2.up * _criticalPrePopYOffset;
        Vector2 endPos = anchoredPosition + Vector2.up * _criticalMoveY;

        _rectTransform.anchoredPosition = startPos;
        _rectTransform.localScale = _originScale * _criticalStartScaleMultiplier;

        _sequence = DOTween.Sequence();

        _sequence.Append(_rectTransform.DOAnchorPos(anchoredPosition, 0.06f).SetEase(Ease.OutQuad));
        _sequence.Append(_rectTransform.DOAnchorPos(endPos, _criticalDuration).SetEase(Ease.OutExpo));

        _sequence.Join(_rectTransform.DOPunchScale(
            Vector3.one * _criticalPunchScale,
            _criticalPunchDuration,
            _criticalVibrato,
            _criticalElasticity));

        _sequence.Join(_tmpText.DOFade(0f, _criticalDuration - _criticalFadeStartDelay)
            .SetDelay(_criticalFadeStartDelay)
            .SetEase(Ease.InQuad));

        _sequence.Append(_rectTransform.DOScale(_originScale, 0.08f).SetEase(Ease.OutQuad));

        _sequence.OnComplete(FinishEffect);
    }

    private void FinishEffect()
    {
        _sequence = null;
        _rectTransform.localScale = _originScale;
        gameObject.SetActive(false);
    }
}