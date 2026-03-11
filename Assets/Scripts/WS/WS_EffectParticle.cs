using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WS_EffectParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private ParticleSystem _particleBoom;
    [SerializeField] private ParticleSystem _particleCharge;

    [Header("Post Process")]
    [SerializeField] private Volume _targetVolume;

    [Header("Chromatic Aberration")]
    [SerializeField] private bool _useChromaticAberrationPunch = true;
    [SerializeField] private float _defaultChromaticIntensity = 0.11f;
    [SerializeField] private float _chargeChromaticIntensity = 0.5f;
    [SerializeField] private float _chromaticDuration = 0.8f;
    [SerializeField] private float _chromaticPunchUpDuration = 0.12f;
    [SerializeField] private Ease _chromaticPunchUpEase = Ease.OutQuad;
    [SerializeField] private Ease _chromaticReturnEase = Ease.OutCubic;
    [SerializeField] private float _fullChargeThreshold = 0.999f;

    private ChromaticAberration _chromaticAberration;
    private Tween _chromaticTween;

    private void Awake()
    {
        InitializePostProcess();
    }

    private void OnDestroy()
    {
        _chromaticTween?.Kill();
    }

    private void InitializePostProcess()
    {
        if (_targetVolume == null || _targetVolume.profile == null)
            return;

        if (_targetVolume.profile.TryGet(out ChromaticAberration chromatic))
        {
            _chromaticAberration = chromatic;
            _defaultChromaticIntensity = _chromaticAberration.intensity.value;
        }
    }

    public void Play(float chargePercent)
    {
        if (_particle != null)
            PlayParticle(_particle);

        if (_particleBoom != null)
            PlayParticle(_particleBoom);

        bool isFullCharge = chargePercent >= _fullChargeThreshold;

        if (isFullCharge && _particleCharge != null)
            PlayParticle(_particleCharge);

        if (isFullCharge && _useChromaticAberrationPunch)
            PlayChromaticAberrationPunch();
    }

    private void PlayParticle(ParticleSystem system)
    {
        if (system == null)
            return;

        system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        system.Clear(false);
        system.Play(false);
    }

    private void PlayChromaticAberrationPunch()
    {
        if (_chromaticAberration == null)
        {
            InitializePostProcess();
            if (_chromaticAberration == null)
                return;
        }

        _chromaticTween?.Kill();

        float startValue = _chromaticAberration.intensity.value;
        float returnDuration = Mathf.Max(0.01f, _chromaticDuration - _chromaticPunchUpDuration);

        Sequence seq = DOTween.Sequence();
        seq.SetLink(gameObject);

        seq.Append(
            DOTween.To(
                () => _chromaticAberration.intensity.value,
                value => _chromaticAberration.intensity.Override(value),
                _chargeChromaticIntensity,
                _chromaticPunchUpDuration
            ).SetEase(_chromaticPunchUpEase)
        );

        seq.Append(
            DOTween.To(
                () => _chromaticAberration.intensity.value,
                value => _chromaticAberration.intensity.Override(value),
                _defaultChromaticIntensity,
                returnDuration
            ).SetEase(_chromaticReturnEase)
        );

        seq.OnKill(() =>
        {
            if (_chromaticAberration != null)
                _chromaticAberration.intensity.Override(_defaultChromaticIntensity);
        });

        seq.OnComplete(() =>
        {
            if (_chromaticAberration != null)
                _chromaticAberration.intensity.Override(_defaultChromaticIntensity);

            _chromaticTween = null;
        });

        _chromaticTween = seq;
    }
}