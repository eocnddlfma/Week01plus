using UnityEngine;

public class EnemyRange : EnemyBase
{
    [Header("Ranged")]
    [Min(0.1f)][SerializeField] private float _attackRange = 6f;
    [SerializeField] private Transform _firePoint;
    [Min(0.1f)][SerializeField] private float _projectileSpeed = 6f;
    [Min(0.1f)][SerializeField] private float _projectileLifeTime = 12f;
    [Min(1)][SerializeField] private int _damage = 1;
    [SerializeField] private LayerMask _targetMask;

    [Header("Wave Scaling (Ranged)")]
    [Min(0f)][SerializeField] private float _damageScalePerWave = 0.1f;
    [Min(0f)][SerializeField] private float _attackRangeScalePerWave = 0.08f;
    [Min(0f)][SerializeField] private float _projectileSpeedScalePerWave = 0.05f;
    [Min(0f)][SerializeField] private float _projectileLifeTimeScalePerWave = 0.05f;

    private int _baseDamage;
    private float _baseAttackRange;
    private float _baseProjectileSpeed;
    private float _baseProjectileLifeTime;

    protected override bool CanAttack(float distanceToTarget) => distanceToTarget <= _attackRange;

    protected override void Awake()
    {
        base.Awake();
        if (_firePoint == null) _firePoint = transform;
        _baseDamage = _damage;
        _baseAttackRange = _attackRange;
        _baseProjectileSpeed = _projectileSpeed;
        _baseProjectileLifeTime = _projectileLifeTime;
    }

    protected override void ApplyWaveScaling(int waveIndex)
    {
        base.ApplyWaveScaling(waveIndex);
        _damage = Mathf.Max(1, Mathf.RoundToInt(_baseDamage * (1f + waveIndex * _damageScalePerWave)));
        _attackRange = _baseAttackRange * (1f + waveIndex * _attackRangeScalePerWave);
        _projectileSpeed = _baseProjectileSpeed * (1f + waveIndex * _projectileSpeedScalePerWave);
        _projectileLifeTime = _baseProjectileLifeTime * (1f + waveIndex * _projectileLifeTimeScalePerWave);
    }

    protected override void DoAttack()
    {
        // Phase 7: 풀에서 투사체 획득
        if (EnemyProjectilePool.Instance == null) return;

        Vector2 origin = _firePoint != null ? (Vector2)_firePoint.position : Rb.position;
        Vector2 dir = GetTargetDirection(origin);

        EnemyProjectile proj = EnemyProjectilePool.Instance.Get();
        if (proj != null)
        {
            proj.transform.position = origin;
            proj.transform.rotation = Quaternion.identity;
            proj.Init(_damage, dir, _projectileSpeed, _projectileLifeTime, _targetMask, gameObject);
        }
    }

    private Vector2 GetTargetDirection(Vector2 origin)
    {
        if (Target != null)
        {
            Vector2 toTarget = (Vector2)Target.position - origin;
            if (toTarget.sqrMagnitude > 0.0001f) return toTarget.normalized;
        }

        return LastDir.sqrMagnitude > 0.001f ? LastDir.normalized : Vector2.down;
    }
}
