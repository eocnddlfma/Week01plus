using UnityEngine;

/// <summary>
/// 작은 공: 기본 OrbitalWeapon에 고배율 데미지 업그레이드 추가
/// 업그레이드: 다윗과 골리앗(DamageMult=10), 핵앤슬래시(DamageMult=15 + 홀수번째 1/10)
///
/// ※ SmallSatellite Variant 프리팹의 OrbitalWeapon 컴포넌트를 이 컴포넌트로 교체하세요.
/// </summary>
public class SmallOrbitalWeapon : OrbitalWeapon
{
    // ── 업그레이드 스태틱 ──
    public static float DamageMult = 1f;   // 다윗과 골리앗: 10f, 핵앤슬래시: 15f
    public static bool HackSlash = false;  // 핵앤슬래시: 홀수 번째 충돌 1/10 데미지

    private int _hitCount = 0;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_state != BallState.Orbit)
            _hitCount++;
        base.OnTriggerEnter2D(other);
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        _hitCount = 0;
        base.RejoinOrbit(currentPos);
    }

    protected override int CalculateDamage(float chargePercent, float attackPower = 1f)
    {
        int damage = Mathf.RoundToInt(base.CalculateDamage(chargePercent, attackPower) * DamageMult);
        // 홀수 번째(1, 3, 5...) 충돌은 1/10 데미지
        if (HackSlash && _hitCount % 2 == 1)
            damage = Mathf.Max(1, damage / 10);
        return damage;
    }
}
