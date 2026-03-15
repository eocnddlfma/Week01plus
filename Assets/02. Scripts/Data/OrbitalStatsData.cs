using UnityEngine;

[CreateAssetMenu(menuName = "Game/OrbitalStats")]
public class OrbitalStatsData : ScriptableObject
{
    [Header("궤도")]
    public float orbitRadius = 2.0f;
    public float orbitAngularSpeed = 180.0f;

    [Header("발사")]
    public float launchSpeed = 14.0f;
    public float launchDuration = 0.35f;
    public float randomAngleOffset = 10.0f;

    [Header("복귀")]
    public float returnStrength = 16.0f;
    public float returnStrengthMax = 150.0f;
    public float maxReturnSpeed = 18.0f;

    [Header("데미지")]
    public int baseDamage = 2;
    public int maxChargeDamage = 20;
    public float varianceRange = 0.1f;
    public float knockbackForce = 0.4f;
}
