using UnityEngine;

/// <summary>
/// 대시 중 파티클 시스템으로 잔상 효과를 재생합니다.
/// PlayerDash.OnDashStateChanged 이벤트를 구독합니다.
/// </summary>
public class DashAfterImage : MonoBehaviour
{
    [SerializeField] private ParticleSystem _afterImageParticle;
    [SerializeField] private PlayerDash _dash;

    private void Awake()
    {
        if (_dash == null) _dash = GetComponent<PlayerDash>();
        if (_afterImageParticle == null) _afterImageParticle = GetComponentInChildren<ParticleSystem>();

        if (_afterImageParticle != null)
            _afterImageParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnEnable()
    {
        if (_dash != null)
            _dash.OnDashStateChanged += OnDashStateChanged;
    }

    private void OnDisable()
    {
        if (_dash != null)
            _dash.OnDashStateChanged -= OnDashStateChanged;
    }

    private void OnDashStateChanged(bool isDashing, float duration)
    {
        if (_afterImageParticle == null) return;

        if (isDashing)
            _afterImageParticle.Play();
        else
            _afterImageParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
