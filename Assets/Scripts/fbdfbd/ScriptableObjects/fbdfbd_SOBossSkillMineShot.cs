using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_MineShot", menuName = "Scriptable Objects/BossSkill/MinsShot")]
public class fbdfbd_SOBossSkillMineShot : fbdfbd_SOBossSkillBase
{
    [Header("Projectile")]
    [SerializeField] private fbdfbd_EnemyBossMineProjectile _mineProjectilePrefab;
    [Min(1)][SerializeField] private int _damage = 1;
    [Min(0.1f)][SerializeField] private float _projectileSpeed = 7f;
    [Range(0f, 1f)][SerializeField] private float _speedRandomPercent = 0.12f;
    [Min(0f)][SerializeField] private float _deceleration = 5f;
    [Min(0f)][SerializeField] private float _stopSpeedThreshold = 0.1f;
    [Min(0f)][SerializeField] private float _blinkDuration = 0.8f;
    [Min(0f)][SerializeField] private float _explodeDelay = 0f;
    [SerializeField] private LayerMask _targetMask;

    [Header("Burst Pattern")]
    [Min(1)][SerializeField] private int _projectilesCount = 20;
    [Min(0f)][SerializeField] private float _projectileInterval = 0.03f;
    [SerializeField] private float _startAngle = 0f;
    [Min(0f)][SerializeField] private float _wobbleAmplitude = 6f;
    [Min(0f)][SerializeField] private float _wobbleFrequency = 0.7f;
    [Min(0f)][SerializeField] private float _randomAngleJitter = 2f;

    public fbdfbd_EnemyBossMineProjectile MineProjectilePrefab => _mineProjectilePrefab;
    public int Damage => _damage;
    public float ProjectileSpeed => _projectileSpeed;
    public float SpeedRandomPercent => _speedRandomPercent;
    public float Deceleration => _deceleration;
    public float StopSpeedThreshold => _stopSpeedThreshold;
    public float BlinkDuration => _blinkDuration;
    public float ExplodeDelay => _explodeDelay;
    public LayerMask TargetMask => _targetMask;
    public int ProjectilesCount => _projectilesCount;
    public float ProjectileInterval => _projectileInterval;
    public float StartAngle => _startAngle;
    public float WobbleAmplitude => _wobbleAmplitude;
    public float WobbleFrequency => _wobbleFrequency;
    public float RandomAngleJitter => _randomAngleJitter;
}
