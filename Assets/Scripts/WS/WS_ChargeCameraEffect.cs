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

    private bool _isCharging;
    private float _noiseTime;
    private float _baseSize;

    private float _endShakeTimer;

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

        Vector3 targetPos = _basePosition;
        float targetSize = _baseSize;

        if (_isCharging && _player != null)
        {
            Vector3 dir = (_player.position - Vector3.zero).normalized;
            targetPos += new Vector3(dir.x, dir.y, 0f) * _moveDistance;
            targetSize = _baseSize + _zoomInOffset;
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

            float t = _endShakeTimer / _endShakeDuration;
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

    public void SetBaseSize(float size)
    {
        _baseSize = size;
    }

    public void BeginCharge()
    {
        _isCharging = true;
    }

    public void EndCharge()
    {
        _isCharging = false;
        _endShakeTimer = _endShakeDuration;
    }
}