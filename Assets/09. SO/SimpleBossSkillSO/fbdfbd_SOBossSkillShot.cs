using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_Shot", menuName = "Scriptable Objects/BossSkill/Shot")]
public class fbdfbd_SOBossSkillShot : fbdfbd_SOBossSkillBase
{
    [Header("Projectile")]
    [SerializeField] private fbdfbd_EnemyProjectile _projectilePrefab;
    [Min(1)][SerializeField] private int _damage = 1;
    [Min(0.1f)][SerializeField] private float _projectileSpeed = 7f;
    [Min(0.1f)][SerializeField] private float _projectileLifeTime = 2f;
    [SerializeField] private LayerMask _targetMask;

    [Header("Burst Pattern")]
    [Min(1)][SerializeField] private int _projectilesPerExecute = 1;
    [Min(0f)][SerializeField] private float _fanAngle = 16f;
    [Min(0f)][SerializeField] private float _waveInterval = 0f;
    [SerializeField] private float[] _angleOffsets = new float[] { 0f, 0f, 0f };

    public fbdfbd_EnemyProjectile ProjectilePrefab => _projectilePrefab;
    public int Damage => _damage;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileLifeTime => _projectileLifeTime;
    public LayerMask TargetMask => _targetMask;
    public int ProjectilesPerExecute => _projectilesPerExecute;
    public float FanAngle => _fanAngle;
    public float WaveInterval => _waveInterval;
    public float[] AngleOffsets => _angleOffsets;
}
