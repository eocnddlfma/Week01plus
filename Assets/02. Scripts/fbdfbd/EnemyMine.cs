using UnityEngine;

public class EnemyMine : EnemyBase
{
    [Header("Mine Shot")]
    [Min(0.1f)][SerializeField] private float _attackRange = 6f;
    [SerializeField] private Transform _shotPoint;
    [Min(1)][SerializeField] private int _damage = 1;
    [Min(0.1f)][SerializeField] private float _projectileSpeed = 7f;
    [Min(0f)][SerializeField] private float _deceleration = 5f;
    [Min(0f)][SerializeField] private float _stopSpeedThreshold = 0.1f;
    [Min(0f)][SerializeField] private float _blinkDuration = 0.8f;
    [Min(0f)][SerializeField] private float _explodeDelay = 0f;
    [SerializeField] private LayerMask _targetMask;

    protected override bool CanAttack(float distanceToTarget) => distanceToTarget <= _attackRange;

    protected override void Awake()
    {
        base.Awake();

        if (_shotPoint == null)
            _shotPoint = transform;
    }

    protected override void DoAttack()
    {
        // Phase 7: 풀에서 투사체 획득
        if (EnemyBossMineProjectilePool.Instance == null)
            return;

        Vector2 origin = _shotPoint != null ? (Vector2)_shotPoint.position : Rb.position;
        Vector2 direction = GetTargetDirection(origin);

        EnemyBossMineProjectile mine = EnemyBossMineProjectilePool.Instance.Get();
        if (mine != null)
        {
            mine.transform.position = origin;
            mine.transform.rotation = Quaternion.identity;
            mine.Init(
                _damage,
                direction,
                _projectileSpeed,
                _deceleration,
                _stopSpeedThreshold,
                _blinkDuration,
                _explodeDelay,
                _targetMask,
                gameObject,
                Target);
        }
    }

    private Vector2 GetTargetDirection(Vector2 origin)
    {
        if (Target != null)
        {
            Vector2 toTarget = (Vector2)Target.position - origin;
            if (toTarget.sqrMagnitude > 0.0001f)
                return toTarget.normalized;
        }

        return LastDir.sqrMagnitude > 0.001f ? LastDir.normalized : Vector2.down;
    }
}
