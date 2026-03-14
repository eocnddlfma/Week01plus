using UnityEngine;

/// <summary>
/// 플레이어 스탯 수정자 (싱글톤).
/// 증강 시스템에서 이 값을 변경하면 기존 시스템들이 자동으로 반영합니다.
/// 플레이어 루트 오브젝트에 부착하거나, AugmentManager가 자동으로 추가합니다.
/// </summary>
public class PlayerStatModifier : MonoBehaviour
{
    public static PlayerStatModifier Instance { get; private set; }

    [Header("배율 (기본값 1.0, 증강이 누적됩니다)")]
    public float MoveSpeedMult = 1f;
    public float BatDamageMult = 1f;
    public float BallDamageMult = 1f;
    public float KnockbackMult = 1f;
    public float ChargeSpeedMult = 1f;
    public float BallSpeedMult = 1f;
    public float AttackRangeMult = 1f;
    public float DashCooldownMult = 1f;  // 낮을수록 빠름
    public float InvincibilityMult = 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ResetAll()
    {
        MoveSpeedMult = 1f;
        BatDamageMult = 1f;
        BallDamageMult = 1f;
        KnockbackMult = 1f;
        ChargeSpeedMult = 1f;
        BallSpeedMult = 1f;
        AttackRangeMult = 1f;
        DashCooldownMult = 1f;
        InvincibilityMult = 1f;
    }
}
