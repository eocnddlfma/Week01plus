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

    [Header("Damaged Shake")]
    [SerializeField] private float _damagedShakeAmplitude = 0.22f;
    [SerializeField] private float _damagedShakeSpeed = 70f;
    [SerializeField] private float _damagedShakeDuration = 0.18f;
    [SerializeField] private float _damagedKickDistance = 0.18f;
    [SerializeField] private bool _useDamageKickBack = true;

    private Jaein_PlayerController _playerController;

    private bool _isCharging;
    private float _baseSize;
    private float _endShakeTimer;
    private float _endImpactTimer;

    public float sizeOffset = 0;
    public Vector3 positionOffset = Vector3.zero;

    private float _chargeNoiseTime;
    private float _endNoiseTime;
    private float _damagedNoiseTime;
    private float _damagedShakeTimer;

    private Vector3 _currentBasePos;
    private float _currentSize;

    private void Reset()
    {
        _targetCamera = GetComponent<Camera>();
    }

    private void Awake()
    {
        if (_targetCamera == null)
            _targetCamera = GetComponent<Camera>();

        if (_targetCamera != null)
        {
            _baseSize = _targetCamera.orthographicSize;
            _currentSize = _baseSize;
        }

        _currentBasePos = _basePosition;
        transform.position = _basePosition;

        CachePlayerController();
    }

    private void OnEnable()
    {
        RegisterPlayerEvent();
    }

    private void OnDisable()
    {
        UnregisterPlayerEvent();
    }

    private void OnDestroy()
    {
        UnregisterPlayerEvent();
    }

    private void LateUpdate()
    {
        if (_targetCamera == null)
            return;

        Vector3 targetBasePos = _basePosition + positionOffset;
        float targetSize = _baseSize + sizeOffset;

        if (_isCharging && _player != null)
        {
            Vector3 dir = GetPlayerDirFromCenter();
            targetBasePos += new Vector3(dir.x, dir.y, 0f) * _moveDistance;
            targetSize = _baseSize + _zoomInOffset;
        }

        if (_endImpactTimer > 0f && _player != null)
        {
            _endImpactTimer -= Time.deltaTime;

            float t = 1f - Mathf.Clamp01(_endImpactTimer / _endImpactDuration);
            float punch = 1f - Mathf.Pow(1f - t, 3f);

            Vector3 dir = GetPlayerDirFromCenter();
            targetBasePos += new Vector3(dir.x, dir.y, 0f) * (_endImpactMoveDistance * punch);
            targetSize += _endImpactZoomOffset * punch;
        }

        _currentBasePos = Vector3.Lerp(_currentBasePos, targetBasePos, _positionLerpSpeed * Time.deltaTime);
        _currentSize = Mathf.Lerp(_currentSize, targetSize, _sizeLerpSpeed * Time.deltaTime);

        Vector3 additiveOffset = Vector3.zero;

        if (_isCharging)
        {
            _chargeNoiseTime += Time.deltaTime * _shakeSpeed;

            float shakeX = (Mathf.PerlinNoise(_chargeNoiseTime, 0f) - 0.5f) * 2f * _shakeAmplitude;
            float shakeY = (Mathf.PerlinNoise(0f, _chargeNoiseTime) - 0.5f) * 2f * _shakeAmplitude;

            additiveOffset += new Vector3(shakeX, shakeY, 0f);
        }

        if (_endShakeTimer > 0f)
        {
            _endShakeTimer -= Time.deltaTime;

            _endNoiseTime += Time.deltaTime * _endShakeSpeed;

            float t = Mathf.Clamp01(_endShakeTimer / _endShakeDuration);
            float amplitude = _endShakeAmplitude * t;

            float shakeX = (Mathf.PerlinNoise(_endNoiseTime, 10f) - 0.5f) * 2f * amplitude;
            float shakeY = (Mathf.PerlinNoise(10f, _endNoiseTime) - 0.5f) * 2f * amplitude;

            additiveOffset += new Vector3(shakeX, shakeY, 0f);
        }

        if (_damagedShakeTimer > 0f)
        {
            _damagedShakeTimer -= Time.deltaTime;
            _damagedNoiseTime += Time.deltaTime * _damagedShakeSpeed;

            float t = Mathf.Clamp01(_damagedShakeTimer / _damagedShakeDuration);
            float amplitude = _damagedShakeAmplitude * t;

            float shakeX = (Mathf.PerlinNoise(_damagedNoiseTime, 20f) - 0.5f) * 2f * amplitude;
            float shakeY = (Mathf.PerlinNoise(20f, _damagedNoiseTime) - 0.5f) * 2f * amplitude;

            additiveOffset += new Vector3(shakeX, shakeY, 0f);

            if (_useDamageKickBack && _player != null)
            {
                Vector3 playerDir = GetPlayerDirFromCenter();
                float kick = 1f - Mathf.Pow(1f - t, 2f);
                additiveOffset -= new Vector3(playerDir.x, playerDir.y, 0f) * (_damagedKickDistance * kick);
            }
        }

        transform.position = _currentBasePos + additiveOffset;
        _targetCamera.orthographicSize = _currentSize;
    }

    /// <summary>
    /// positionOffset을 DOTween으로 보간
    /// </summary>
    public void TweenToPositionOffset(Vector3 offset, float duration)
    {
        DOTween.To(() => positionOffset, x => positionOffset = x, offset, duration)
               .SetEase(Ease.OutCubic);
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

    private void CachePlayerController()
    {
        if (_player == null)
        {
            _playerController = null;
            return;
        }

        _playerController = _player.GetComponent<Jaein_PlayerController>();
    }

    private void RegisterPlayerEvent()
    {
        if (_playerController == null)
            CachePlayerController();

        if (_playerController != null)
        {
            _playerController.OnDamaged += HandleDamaged;
        }
    }

    private void UnregisterPlayerEvent()
    {
        if (_playerController != null)
        {
            _playerController.OnDamaged -= HandleDamaged;
        }
    }

    private void HandleDamaged(int currentHp)
    {
        _damagedShakeTimer = _damagedShakeDuration;
    }

    private Vector3 GetPlayerDirFromCenter()
    {
        if (_player == null)
            return Vector3.zero;

        Vector3 dir = _player.position - Vector3.zero;
        if (dir.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        return dir.normalized;
    }
}