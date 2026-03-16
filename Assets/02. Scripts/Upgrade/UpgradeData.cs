using System.Collections.Generic;
using UnityEngine;

public enum UpgradeRarity
{
    Common,
    Rare,
    Epic
}

public enum UpgradeStatType
{
    MoveSpeed,      // 이동 속도 배율
    BatDamage,      // 배트 데미지 배율
    BallDamage,     // 공 데미지 배율
    Knockback,      // 넉백 배율
    ChargeSpeed,    // 차지 속도 배율
    BallSpeed,      // 공 발사 속도 배율
    AttackRange,    // 공격 범위 배율
    DashCooldown,   // 대시 쿨다운 배율 (음수 = 감소)
    Invincibility,  // 무적 시간 배율
    MaxHp,          // 최대 체력 (정수 증가)
    HpRegen,        // 초당 체력 회복 (초당 회복량)
    BatAttackSpeed, // 배트 공격 속도 (휘두르는 속도)
    BatAttackCooldown, // 배트 공격 쿨다운 (음수 = 감소)

    // ── 공 특화 업그레이드 ──
    Split4Way,          // 분열공: 사방미인 – 4방향 분열
    Split8Way,          // 분열공: 팔방미인 – 8방향 분열
    BombRadiusMult,     // 폭탄공: 폭탄 받아라 – 범위/데미지 배율
    BombAutoExplode,    // 폭탄공: 터져버렷 – 자동 주기 폭발
    BatSwingRadius,     // 빠따공: 홈런 – 스윙 반경 배율
    BatHalfHp,          // 빠따공: 빠따로 맞아볼래? – 현재 체력 절반
    BounceNoDamp,       // 벽반사공: 작용반작용 – 속도 손실 없음
    BounceHitBonus,     // 벽반사공: 예술적 각도 – 반사 횟수 비례 데미지
    GravityMult,        // 중력공: 내게로 와 – 인력 배율
    GravityRepel,       // 중력공: 저리가! – 척력으로 전환
    HeavyMaxHpDamage,   // 무거운공: 압사 – 최대 체력% 추가 데미지
    HeavyCurling,       // 무거운공: 컬링 마스터 – 주변 공 날려보냄
    NormalFixedDamage,  // 평범한공: 애도/기도/회고 – 고정 데미지
    PenFencingMaster,   // 관통공: 펜싱마스터 – 발사 거리 배율
    PenContinuousStab,  // 관통공: 연속찌르기 – 빠른 재충돌 보너스
    SmallDamageMult,    // 작은공: 다윗과 골리앗 – 데미지 배율
    SmallHackSlash,     // 작은공: 핵앤슬래시 – 15배+홀수 1/10
    StraightRelaunch,   // 직선공: 찌찌르기! – 복귀 시 재발사
    StraightKnockback,  // 직선공: 직선넘네 – 넉백 배율
    WallThresholdBlast, // 벽공: 벽력일섬 – 10명 이상 현재 체력 절반
    WallThrowDetach,    // 벽공: 벽치기 – 복귀 시 날려보냄
    WallWideBody,       // 벽공: 판때기 – 가로 크기 4배
    WhirlNoReturn,      // 회오리공: 가출 – 궤도 미복귀
    WhirlCollisionStack,// 회오리공: 몰아치기 – 충돌마다 데미지+1

    UnlockChargeLevel,  // 차지 단계 해금 (+1단계, 차지시간 +0.5초)
}

/// <summary>
/// 증강 데이터 ScriptableObject.
/// Project 창에서 우클릭 → Create → Game → UpgradeData 로 생성 가능합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [Header("기본 정보")]
    public string upgradeName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;

    [Header("등급")]
    public UpgradeRarity rarity = UpgradeRarity.Common;

    [Header("효과")]
    public UpgradeStatType statType;

    [Tooltip("배율 타입: 배율에 더해지는 값 (0.15 = +15%)\nDashCooldown: 음수로 설정 (-0.2 = 쿨다운 20% 감소)\nMaxHp: 증가할 정수 값 (1 = +1 HP)")]
    public float value;

    [Header("선행 조건")]
    [Tooltip("이 업그레이드가 풀에 등장하려면 먼저 획득해야 하는 업그레이드들")]
    public List<UpgradeData> prerequisites;
}
