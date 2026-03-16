using UnityEngine;

/// <summary>
/// 360도 회전 링샷 스킬 데이터.
/// Execute가 반복될수록 링이 RotationPerExecute만큼 회전합니다.
/// </summary>
[CreateAssetMenu(fileName = "BossSkill_RotatingShot", menuName = "Scriptable Objects/BossSkill/RotatingShot")]
public class SOBoss2SkillRotatingShot : fbdfbd_SOBossSkillBase
{
    [Header("Projectile")]
    [Min(1)][SerializeField] private int _damage = 1;
    [Min(0.1f)][SerializeField] private float _projectileSpeed = 5f;
    [Min(0.1f)][SerializeField] private float _projectileLifeTime = 3f;
    [SerializeField] private LayerMask _targetMask;

    [Header("Ring Pattern")]
    [Min(1)][SerializeField] private int _projectilesPerRing = 12;
    [SerializeField] private float _initialAngleOffset = 0f;

    [Header("Rotation")]
    [SerializeField] private float _rotationPerExecute = 15f;   // Execute마다 회전 각도
    [SerializeField] private float _waveInterval = 0f;          // 파도 간격 (0이면 즉시)

    public int Damage => _damage;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileLifeTime => _projectileLifeTime;
    public LayerMask TargetMask => _targetMask;
    public int ProjectilesPerRing => _projectilesPerRing;
    public float InitialAngleOffset => _initialAngleOffset;
    public float RotationPerExecute => _rotationPerExecute;
    public float WaveInterval => _waveInterval;
}
