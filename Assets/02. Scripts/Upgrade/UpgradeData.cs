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
    MaxHp           // 최대 체력 (정수 증가)
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
