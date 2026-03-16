using UnityEngine;

/// <summary>
/// 직선 공: 기본 OrbitalWeapon에 재발사/넉백 업그레이드 추가
/// 업그레이드: 찌찌르기!(Relaunch), 직선넘네(KnockbackBonus=2)
///
/// ※ StraightSatellite Variant 프리팹의 OrbitalWeapon 컴포넌트를 이 컴포넌트로 교체하세요.
/// </summary>
public class StraightOrbitalWeapon : OrbitalWeapon
{
    // ── 업그레이드 스태틱 ──
    public static bool Relaunch = false;       // 찌찌르기!: 복귀 시 한 번 더 발사
    public static float KnockbackBonus = 1f;   // 직선넘네: 넉백 배율 (예: 2f)

    private bool _hasRelaunched = false;

    protected override float GetKnockbackMultiplier() => KnockbackBonus;

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        base.RejoinOrbit(currentPos);

        if (Relaunch && !_hasRelaunched)
        {
            _hasRelaunched = true;
            Launch(0f); // 무차지 재발사
        }
        else
        {
            _hasRelaunched = false;
        }
    }
}
