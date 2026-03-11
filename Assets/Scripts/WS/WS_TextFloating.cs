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
    }

    public void Play(int damage, Vector2 anchoredPosition)
    {
        if (_tmpText == null)
            _tmpText = GetComponent<TMP_Text>();

        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        _sequence?.Kill();

        gameObject.SetActive(true);

        _tmpText.text = damage.ToString();
        _tmpText.color = new Color(_originColor.r, _originColor.g, _originColor.b, 1f);

        _rectTransform.anchoredPosition = anchoredPosition;
        _rectTransform.localScale = _originScale;

        Vector2 startPos = _rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * _moveY;

        _sequence = DOTween.Sequence();
        _sequence.Append(_rectTransform.DOAnchorPos(endPos, _duration).SetEase(Ease.OutCubic));
        _sequence.Join(_rectTransform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, _vibrato, _elasticity));
        _sequence.Join(_tmpText.DOFade(0f, _duration - _fadeStartDelay).SetDelay(_fadeStartDelay).SetEase(Ease.InQuad));

        _sequence.OnComplete(() =>
        {
            _sequence = null;
            gameObject.SetActive(false);
        });
    }
}