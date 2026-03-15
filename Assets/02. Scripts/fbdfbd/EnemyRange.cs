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

    protected override bool CanAttack(float distanceToTarget) => distanceToTarget <= _attackRange;

    protected override void Awake()
    {
        base.Awake();
        if (_firePoint == null) _firePoint = transform;
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
