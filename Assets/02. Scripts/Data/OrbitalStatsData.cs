using UnityEngine;

[CreateAssetMenu(menuName = "Game/OrbitalStats")]
public class OrbitalStatsData : ScriptableObject
{
    [Header("궤도")]
    public float orbitRadius = 2.0f;
    public float orbitAngularSpeed = 180.0f;

    [Header("발사")]
    public float launchSpeed = 14.0f;
    public float launchSpeedOffset = 2.0f;
    public float launchDuration = 0.35f;
    public float launchDurationOffset = 0.05f;
    public float randomAngleOffset = 10.0f;

    [Header("복귀 - 물리")]
    public float returnStrength = 16.0f;
    public float returnDamping = 1.0f;

    [Header("복귀 - 에스컬레이션")]
    public float returnStrengthMax = 150.0f;
    public float returnEscalationTime = 3.0f;

    [Header("복귀 - 궤도 보조")]
    public float orbitAssistStrength = 12.0f;
    public bool useCounterClockwiseAssist = true;

    [Header("복귀 - 속도 제한")]
    public float maxReturnSpeed = 18.0f;

    [Header("복귀 - 궤도 재진입")]
    public float rejoinDistanceToOrbit = 0.2f;
    public float rejoinVelocityLimit = 8.0f;
    public float rejoinBrakeDamping = 8.0f;

    [Header("스냅")]
    public float snapDuration = 0.3f;

    [Header("데미지")]
    public int baseDamage = 2;
    public int maxChargeDamage = 20;
    public float varianceRange = 0.1f;
    public float knockbackForce = 0.4f;

    [Header("특수 동작")]
    public bool noReturn = false; // 궤도로 복귀하지 않음 (가출)
}
