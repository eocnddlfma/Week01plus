using UnityEngine;

/// <summary>
/// 무거운 공: 기본 OrbitalWeapon에 중량 특화 효과 추가
/// 업그레이드: 압사(MaxHpDamagePercent), 컬링 마스터(Curling)
///
/// ※ HeavySatellite Variant 프리팹의 OrbitalWeapon 컴포넌트를 이 컴포넌트로 교체하세요.
/// </summary>
public class HeavyOrbitalWeapon : OrbitalWeapon
{
    // ── 업그레이드 스태틱 ──
    public static float MaxHpDamagePercent = 0f; // 압사: 최대 체력의 X% 추가 데미지 (예: 0.03)
    public static bool Curling = false;           // 컬링 마스터: 충돌 시 주변 공 날려보냄

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_state == BallState.Orbit) return;

        base.OnTriggerEnter2D(other);

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null) return;

        // 압사: 최대 체력 % 추가 데미지
        if (MaxHpDamagePercent > 0f && !enemy.IsDead)
        {
            int extraDamage = Mathf.Max(1, Mathf.RoundToInt(enemy.MaxHp * MaxHpDamagePercent));
            enemy.TakeDamage(extraDamage);
        }

        // 컬링 마스터: 주변 공들 날려보냄
        if (Curling)
            ApplyCurling();
    }

    private void ApplyCurling()
    {
        if (_velocity.sqrMagnitude < 0.001f) return;
        Vector2 dir = _velocity.normalized;

        var allOrbitals = FindObjectsByType<OrbitalWeapon>(FindObjectsSortMode.None);
        foreach (var orbital in allOrbitals)
        {
            if (orbital == this) continue;
            float dist = Vector2.Distance(orbital.transform.position, transform.position);
            if (dist < 4f)
                orbital.ForceKnockOff(dir, 15f);
        }
    }
}
