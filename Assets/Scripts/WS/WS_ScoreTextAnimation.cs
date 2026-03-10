using UnityEngine;
using TMPro;
using DG.Tweening;

public class WS_ScoreTextAnimation : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text _tmpText;

    [Header("Count Animation")]
    [SerializeField] private float _countDuration = 0.45f;
    [SerializeField] private Ease _countEase = Ease.OutCubic;

    [Header("Punch During Count")]
    [SerializeField] private float _playPunchScale = 0.12f;
    [SerializeField] private float _playPunchDuration = 0.18f;
    [SerializeField] private int _playPunchVibrato = 8;
    [SerializeField] private float _playPunchElasticity = 0.8f;

    [Header("Final Emphasis")]
    [SerializeField] private float _finalPunchScale = 0.22f;
    [SerializeField] private float _finalPunchDuration = 0.3f;
    [SerializeField] private int _finalPunchVibrato = 10;
    [SerializeField] private float _finalPunchElasticity = 1.0f;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _finalHighlightColor = new Color(1f, 0.9f, 0.25f);
    [SerializeField] private float _highlightFlashDuration = 0.12f;

    private RectTransform _rectTransform;
    private Sequence _sequence;
    private Tween _countTween;
    private Tween _colorTween;

    private int _currentScore;
    private Vector3 _defaultScale;
    private Color _defaultColor;

    private void Awake()
    {
        if (_tmpText == null)
            _tmpText = GetComponent<TMP_Text>();

        _rectTransform = _tmpText.rectTransform;
        _defaultScale = _rectTransform.localScale;
        _defaultColor = _tmpText.color;
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }

    public void SetImmediate(int score)
    {
        KillAllTweens();
        ResetVisual();

        _currentScore = Mathf.Max(0, score);
        UpdateText(_currentScore);
    }

    public void Play(int targetScore)
    {
        targetScore = Mathf.Max(0, targetScore);

        //중복 제거 로직
        KillAllTweens();
        ResetVisual();

        int startScore = _currentScore;
        int displayedScore = startScore;

        _sequence = DOTween.Sequence().SetLink(gameObject);
        
        if (targetScore != startScore)
        {
            _countTween = DOTween
                .To(() => displayedScore, x =>
                {
                    displayedScore = x;
                    UpdateText(displayedScore);
                }, targetScore, _countDuration)
                .SetEase(_countEase)
                .SetLink(gameObject);

            _sequence.Append(_countTween);

            _sequence.Join(
                _rectTransform.DOPunchScale(
                    Vector3.one * _playPunchScale,
                    _playPunchDuration,
                    _playPunchVibrato,
                    _playPunchElasticity)
                .SetLink(gameObject)
            );
        }
        else
        {
            UpdateText(targetScore);
        }

        _sequence.AppendCallback(() =>
        {
            _currentScore = targetScore;
            UpdateText(_currentScore);
        });

        _sequence.AppendCallback(PlayFinalImpact);

        _sequence.Play();
    }

    private void PlayFinalImpact()
    {
        _rectTransform.localScale = _defaultScale;
        _tmpText.color = _normalColor;

        Sequence finalSequence = DOTween.Sequence().SetLink(gameObject);

        finalSequence.Append(
            _rectTransform.DOPunchScale(
                Vector3.one * _finalPunchScale,
                _finalPunchDuration,
                _finalPunchVibrato,
                _finalPunchElasticity)
            .SetLink(gameObject)
        );

        finalSequence.Join(
            _tmpText.DOColor(_finalHighlightColor, _highlightFlashDuration)
                .SetLoops(2, LoopType.Yoyo)
                .SetLink(gameObject)
        );

        finalSequence.OnComplete(() =>
        {
            _rectTransform.localScale = _defaultScale;
            _tmpText.color = _normalColor;
        });
    }

    private void UpdateText(int value)
    {
        _tmpText.text = value.ToString("N0");
    }

    private void KillAllTweens()
    {
        _sequence?.Kill();
        _countTween?.Kill();
        _colorTween?.Kill();

        _sequence = null;
        _countTween = null;
        _colorTween = null;

        if (_rectTransform != null)
            DOTween.Kill(_rectTransform);

        if (_tmpText != null)
            DOTween.Kill(_tmpText);
    }

    private void ResetVisual()
    {
        if (_rectTransform != null)
            _rectTransform.localScale = _defaultScale;

        if (_tmpText != null)
            _tmpText.color = _normalColor;
    }
}