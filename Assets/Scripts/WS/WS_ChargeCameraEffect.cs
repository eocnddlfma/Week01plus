using DG.Tweening;
using UnityEngine;

public class WS_ChargeCameraEffect : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private Transform _player;

    [Header("Base")]
    [SerializeField] private Vector3 _basePosition = new Vector3(0f, 0f, -10f);

    [Header("Charge")]
    [SerializeField] private float _moveDistance = 0.6f;
    [SerializeField] private float _positionLerpSpeed = 8f;
    [SerializeField] private float _zoomInOffset = -0.6f;
    [SerializeField] private float _sizeLerpSpeed = 8f;

    [Header("Charge Shake")]
    [SerializeField] private float _shakeAmplitude = 0.05f;
    [SerializeField] private float _shakeSpeed = 30f;

    [Header("End Impact Shake")]
    [SerializeField] private float _endShakeAmplitude = 0.18f;
    [SerializeField] private float _endShakeSpeed = 55f;
    [SerializeField] private float _endShakeDuration = 0.12f;
    [SerializeField] private float _fullChargeThreshold = 0.999f;

    [Header("End Impact Zoom")]
    [SerializeField] private float _endImpactMoveDistance = 1.1f;
    [SerializeField] private float _endImpactZoomOffset = -1.0f;
    [SerializeField] private float _endImpactDuration = 0.14f;

    private bool _isCharging;
    private float _noiseTime;
    private float _baseSize;
    private float _endShakeTimer;
    private float _endImpactTimer;

    public float sizeOffset = 0;
    public Vector3 positionOffset = Vector3.zero;

    private void Reset()
    {
        _targetCamera = GetComponent<Camera>();
    }

    private void Awake()
    {
        if (_targetCamera == null)
            _targetCamera = GetComponent<Camera>();

        if (_targetCamera != null)
            _baseSize = _targetCamera.orthographicSize;
    }

    private void LateUpdate()
    {
        if (_targetCamera == null)
            return;

        Vector3 targetPos = _basePosition + positionOffset;
        float targetSize = _baseSize + sizeOffset;

        if (_isCharging && _player != null)
        {
            Vector3 dir = (_player.position - Vector3.zero).normalized;
            targetPos += new Vector3(dir.x, dir.y, 0f) * _moveDistance;
            targetSize = _baseSize + _zoomInOffset;
        }

        if (_endImpactTimer > 0f && _player != null)
        {
            _endImpactTimer -= Time.deltaTime;

            float t = 1f - Mathf.Clamp01(_endImpactTimer / _endImpactDuration);
            float punch = 1f - Mathf.Pow(1f - t, 3f);

            Vector3 dir = (_player.position - Vector3.zero).normalized;
            targetPos += new Vector3(dir.x, dir.y, 0f) * (_endImpactMoveDistance * punch);
            targetSize += _endImpactZoomOffset * punch;
        }

        Vector3 finalPos = Vector3.Lerp(transform.position, targetPos, _positionLerpSpeed * Time.deltaTime);

        if (_isCharging)
        {
            _noiseTime += Time.deltaTime * _shakeSpeed;

            float shakeX = (Mathf.PerlinNoise(_noiseTime, 0f) - 0.5f) * 2f * _shakeAmplitude;
            float shakeY = (Mathf.PerlinNoise(0f, _noiseTime) - 0.5f) * 2f * _shakeAmplitude;

            finalPos += new Vector3(shakeX, shakeY, 0f);
        }

        if (_endShakeTimer > 0f)
        {
            _endShakeTimer -= Time.deltaTime;
            _noiseTime += Time.deltaTime * _endShakeSpeed;

            float t = Mathf.Clamp01(_endShakeTimer / _endShakeDuration);
            float amplitude = _endShakeAmplitude * t;

            float shakeX = (Mathf.PerlinNoise(_noiseTime, 10f) - 0.5f) * 2f * amplitude;
            float shakeY = (Mathf.PerlinNoise(10f, _noiseTime) - 0.5f) * 2f * amplitude;

            finalPos += new Vector3(shakeX, shakeY, 0f);
        }

        transform.position = finalPos;
        _targetCamera.orthographicSize = Mathf.Lerp(
            _targetCamera.orthographicSize,
            targetSize,
            _sizeLerpSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// positionOffset을 DOTween으로 보간
    /// </summary>
    public void TweenToPositionOffset(Vector3 offset, float duration)
    {
        DOTween.To(() => positionOffset, x => positionOffset = x, offset, duration)
               .SetEase(Ease.OutCubic);
    }

    public void SetBaseSize(float size)
    {
        _baseSize = size;
    }

    public void SetSizeOffset(float offset)
    {
        sizeOffset = offset;
    }

    /// <summary>
    /// targetSize까지 sizeOffset을 DOTween으로 보간
    /// </summary>
    public void TweenToSize(float targetSize, float duration)
    {
        float targetOffset = targetSize - _baseSize;
        DOTween.To(() => sizeOffset, x => sizeOffset = x, targetOffset, duration)
               .SetEase(Ease.OutCubic);
    }

    public void BeginCharge()
    {
        _isCharging = true;
        _endShakeTimer = 0f;
        _endImpactTimer = 0f;
    }

    public void EndCharge(float chargePercent)
    {
        _isCharging = false;

        bool isFullCharge = chargePercent >= _fullChargeThreshold;

        _endShakeTimer = isFullCharge ? _endShakeDuration : 0f;
        _endImpactTimer = isFullCharge ? _endImpactDuration : 0f;
    }
}