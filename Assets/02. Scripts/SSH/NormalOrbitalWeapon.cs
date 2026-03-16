using UnityEngine;

/// <summary>
/// 평범한 공: 기본 OrbitalWeapon에 고정 데미지 업그레이드 추가
/// 업그레이드: 애도(100고정) → 기도(500고정) → 회고(1000고정)
///
/// ※ NormalSatellite Variant 프리팹의 OrbitalWeapon 컴포넌트를 이 컴포넌트로 교체하세요.
/// </summary>
public class NormalOrbitalWeapon : OrbitalWeapon
{
    // ── 업그레이드 스태틱 ──
    // 0 = 고정 데미지 비활성, 양수 = 해당 값으로 데미지 고정
    public static int FixedDamage = 0;

    protected override int CalculateDamage(float chargePercent, float attackPower = 1f)
    {
        if (FixedDamage > 0)
            return FixedDamage;
        return base.CalculateDamage(chargePercent, attackPower);
    }
}
