using System.Collections;
using UnityEngine;

public class HitStopController : MonoBehaviour
{
    [SerializeField] private float _defaultDuration = 0.035f;
    [SerializeField] private float _slowedTimeScale = 0.08f;

    private Coroutine _routine;

    public bool TryPlay()
    {
        float duration = _defaultDuration;

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(HitStopRoutine(duration));
        return true;
    }

    // 차지량에 따라 슬로우 강도와 시간을 동적으로 조정하는 메서드
    public bool TryPlayWithCharge(float chargePercent)
    {
        chargePercent = Mathf.Clamp01(chargePercent);
        
        // 차지량에 따라 슬로우 시간 조정 (0% = 0.035초 → 100% = 0.1초)
        float duration = Mathf.Lerp(1, 0.1f, chargePercent);
        
        // 차지량에 따라 슬로우 강도 조정 (0% = 0.5 → 100% = 0.1)
        float slowedTimeScale = Mathf.Lerp(1f, 0.1f, chargePercent);

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(HitStopRoutine(duration, slowedTimeScale));
        return true;
    }

    /// <summary>
    /// 차지량과 타격 횟수에 따라 슬로우 조정
    /// 타격이 많을수록 슬로우 시간이 짧아짐 (연속 타격 느낌)
    /// 10개 타격 시 약 0.7배 정도로 감소
    /// </summary>
    public bool TryPlayWithChargeAndHitCount(float chargePercent, int hitCount, float durationMultiplier = 1f)
    {
        chargePercent = Mathf.Clamp01(chargePercent);
        hitCount = Mathf.Max(1, hitCount); // 최소 1

        // 차지량에 따라 기본 슬로우 시간 계산 (0% = 0.035초 → 100% = 0.1초)
        float baseDuration = Mathf.Lerp(_defaultDuration, 0.1f, chargePercent);

        // 타격 횟수에 따라 슬로우 시간 감소 (천천히 감소)
        // 1타: 100%, 10타: 70%, 더 많으면 더 감소
        float hitCountMultiplier = Mathf.Clamp01(1f - Mathf.Sqrt(hitCount - 1) * 0.1f);

        float durationMult = baseDuration * hitCountMultiplier * durationMultiplier;

        // 차지량에 따라 슬로우 강도 조정 (0% = 0.5 → 100% = 0.1)
        float slowedTimeScale = Mathf.Lerp(1f, 0.1f, chargePercent);

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(HitStopRoutine(durationMult, slowedTimeScale));
        return true;
    }

    private IEnumerator HitStopRoutine(float durationMult, float mult = 0f)
    {
        float newTimeScale = _slowedTimeScale * mult;
        Time.timeScale = newTimeScale;
        yield return new WaitForSecondsRealtime(_defaultDuration * durationMult);
        // UI 등 외부에서 timeScale을 0으로 내린 상태면 복원하지 않음
        if (Time.timeScale <= newTimeScale)
            Time.timeScale = 1f;
        _routine = null;
    }

    private void OnDisable()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    private void OnDestroy()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }
}