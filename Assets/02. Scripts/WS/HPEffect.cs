using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WS_HPEffect : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private Volume _targetVolume;

    [Header("Vignette")]
    [SerializeField] private Color _safeColor = Color.black;
    [SerializeField] private Color _dangerColor = Color.red;
    [SerializeField] private float _maxIntensity = 0.7f;
    [SerializeField] private float _colorLerpSpeed = 5f;
    [SerializeField] private float _intensityLerpSpeed = 5f;

    [Header("Pulse")]
    [SerializeField] private float _pulseAmplitude = 0.08f;
    [SerializeField] private float _pulseSpeed = 3.5f;
    [SerializeField] private AnimationCurve _dangerCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vignette _vignette;

    private Color _currentColor;
    private float _currentIntensity;

    private Color _targetColor;
    private float _targetBaseIntensity;

    private float _pulseTime;

    private void Awake()
    {
        InitializeVolume();
    }

    private void OnEnable()
    {
        if (_player != null)
            _player.OnDamaged += HandleDamaged;
    }

    private void Start()
    {
        if (_player != null)
            RefreshFromCurrentHP();
    }

    private void OnDisable()
    {
        if (_player != null)
            _player.OnDamaged -= HandleDamaged;
    }

    private void Update()
    {
        if (_vignette == null)
            return;

        float danger01 = GetDanger01();
        float pulse = 0f;

        if (danger01 > 0.001f)
        {
            _pulseTime += Time.deltaTime * Mathf.Lerp(0f, _pulseSpeed, danger01);
            pulse = Mathf.Sin(_pulseTime) * (_pulseAmplitude * danger01);
        }

        float finalTargetIntensity = Mathf.Clamp01(_targetBaseIntensity + pulse);
        finalTargetIntensity = Mathf.Min(finalTargetIntensity, _maxIntensity);

        _currentIntensity = Mathf.Lerp(_currentIntensity, finalTargetIntensity, _intensityLerpSpeed * Time.deltaTime);
        _currentColor = Color.Lerp(_currentColor, _targetColor, _colorLerpSpeed * Time.deltaTime);

        _vignette.color.Override(_currentColor);
        _vignette.intensity.Override(_currentIntensity);
    }

    private void InitializeVolume()
    {
        if (_targetVolume == null || _targetVolume.profile == null)
            return;

        if (_targetVolume.profile.TryGet(out Vignette vignette))
        {
            _vignette = vignette;
            _currentColor = _vignette.color.value;
            _currentIntensity = _vignette.intensity.value;

            _targetColor = _currentColor;
            _targetBaseIntensity = _currentIntensity;
        }
    }

    private void HandleDamaged(int currentHP)
    {
        UpdateTargets(currentHP);
    }

    private void RefreshFromCurrentHP()
    {
        UpdateTargets(_player.Hp);
    }

    private void UpdateTargets(int currentHP)
    {
        if (_player == null || _vignette == null)
            return;

        int maxHP = Mathf.Max(1, _player.MaxHp);

        float hp01 = Mathf.Clamp01((float)currentHP / maxHP);
        float danger01 = 1f - hp01;
        float curvedDanger = _dangerCurve.Evaluate(danger01);

        _targetColor = Color.Lerp(_safeColor, _dangerColor, curvedDanger);
        _targetBaseIntensity = Mathf.Lerp(0f, _maxIntensity, curvedDanger);
    }

    private float GetDanger01()
    {
        if (_player == null)
            return 0f;

        int maxHP = Mathf.Max(1, _player.MaxHp);
        float hp01 = Mathf.Clamp01((float)_player.Hp / maxHP);
        return 1f - hp01;
    }
}