using DG.Tweening;
using UnityEngine;

public class WS_BoundaryRingAnimator : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Rotate")]
    [SerializeField] private bool _useRotate = true;
    [SerializeField] private RotationAxis _rotationAxis = RotationAxis.Y;
    [SerializeField] private bool _clockwise = true;
    [SerializeField] private float _baseRotationDegreesPerSecond = 14f;

    [Header("Idle Pulse")]
    [SerializeField] private bool _useIdlePulse = true;
    [SerializeField] private float _idlePulseStrength = 0.015f;
    [SerializeField] private float _idlePulseDuration = 2.4f;
    [SerializeField] private float _startDelay = 0f;
    [SerializeField] private Ease _idlePulseEase = Ease.InOutSine;

    [Header("Kill Boom")]
    [SerializeField] private bool _useKillBoom = true;
    [SerializeField] private float _boomAddPerKill = 0.08f;
    [SerializeField] private float _boomMax = 0.35f;
    [SerializeField] private float _boomInDuration = 0.06f;
    [SerializeField] private float _boomRecoverSpeed = 0.7f;

    [Header("Rotation Boost By Boom")]
    [SerializeField] private float _rotationBoostPerBoom01 = 55f;

    private Sequence _idlePulseSequence;
    private Tween _boomInTween;

    private Vector3 _baseLocalScale;
    private Vector3 _idlePulseScaleOffset;

    private float _boomScaleOffset;
    private float _currentRotationSpeed;

    private void Awake()
    {
        _baseLocalScale = transform.localScale;
        _currentRotationSpeed = _baseRotationDegreesPerSecond;
    }

    private void Start()
    {
        GameEvents.OnScoreChanged += BoomPulse;  // Phase 6: GameEvents로 변경
    }
    private void OnEnable()
    {
        ResetState();
        PlayIdlePulse();
    }

    private void OnDisable()
    {
        KillTweens();
        ResetTransformImmediate();
    }

    private void OnDestroy()
    {
        GameEvents.OnScoreChanged -= BoomPulse;  // Phase 6: GameEvents 구독 해제 추가 (기존 누락 버그 수정)
        KillTweens();
    }

    private void Update()
    {
        UpdateBoomRecovery();
        UpdateRotation();
        ApplyScale();
    }

    private void BoomPulse(int score)
    {
        if (_useKillBoom == false)
            return;

        _boomInTween?.Kill();

        float targetBoom = Mathf.Min(_boomScaleOffset + Mathf.Max(0f, _boomAddPerKill), _boomMax);

        _boomInTween = DOTween.To(
                () => _boomScaleOffset,
                value => _boomScaleOffset = value,
                targetBoom,
                _boomInDuration
            )
            .SetEase(Ease.OutQuad)
            .OnKill(() => _boomInTween = null)
            .OnComplete(() => _boomInTween = null);
    }

    private void PlayIdlePulse()
    {
        _idlePulseSequence?.Kill();
        _idlePulseScaleOffset = Vector3.zero;

        if (_useIdlePulse == false)
            return;

        _idlePulseSequence = DOTween.Sequence();
        _idlePulseSequence.SetDelay(_startDelay);

        float half = _idlePulseDuration * 0.5f;
        Vector3 targetOffset = _baseLocalScale * _idlePulseStrength;

        _idlePulseSequence.Append(
            DOTween.To(
                () => _idlePulseScaleOffset,
                value => _idlePulseScaleOffset = value,
                targetOffset,
                half
            ).SetEase(_idlePulseEase)
        );

        _idlePulseSequence.Append(
            DOTween.To(
                () => _idlePulseScaleOffset,
                value => _idlePulseScaleOffset = value,
                Vector3.zero,
                half
            ).SetEase(_idlePulseEase)
        );

        _idlePulseSequence.SetLoops(-1, LoopType.Restart);
    }

    private void UpdateBoomRecovery()
    {
        if (_boomScaleOffset <= 0f)
        {
            _boomScaleOffset = 0f;
            return;
        }

        _boomScaleOffset = Mathf.MoveTowards(
            _boomScaleOffset,
            0f,
            _boomRecoverSpeed * Time.deltaTime
        );
    }

    private void UpdateRotation()
    {
        if (_useRotate == false)
            return;

        float direction = _clockwise ? -1f : 1f;
        float boostedSpeed = _baseRotationDegreesPerSecond + (_boomScaleOffset * _rotationBoostPerBoom01);
        _currentRotationSpeed = boostedSpeed;

        Vector3 axis = GetAxisVector(_rotationAxis);
        transform.Rotate(axis, _currentRotationSpeed * direction * Time.deltaTime, Space.Self);
    }

    private void ApplyScale()
    {
        float boomMultiplier = 1f + _boomScaleOffset;
        Vector3 boomScale = _baseLocalScale * boomMultiplier;

        transform.localScale = boomScale + _idlePulseScaleOffset;
    }

    private Vector3 GetAxisVector(RotationAxis axis)
    {
        switch (axis)
        {
            case RotationAxis.X:
                return Vector3.right;
            case RotationAxis.Y:
                return Vector3.up;
            case RotationAxis.Z:
                return Vector3.forward;
            default:
                return Vector3.up;
        }
    }

    private void ResetState()
    {
        _boomScaleOffset = 0f;
        _idlePulseScaleOffset = Vector3.zero;
        _currentRotationSpeed = _baseRotationDegreesPerSecond;
    }

    private void ResetTransformImmediate()
    {
        transform.localScale = _baseLocalScale;
    }

    private void KillTweens()
    {
        _idlePulseSequence?.Kill();
        _boomInTween?.Kill();

        _idlePulseSequence = null;
        _boomInTween = null;
    }
}