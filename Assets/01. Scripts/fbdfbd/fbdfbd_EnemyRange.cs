using UnityEngine;

public class fbdfbd_EnemyRange : fbdfbd_EnemyBase
{
    [Header("Ranged")]
    [Min(0.1f)][SerializeField] private float _attackRange = 6f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _projectilePrefab;
    [Min(0.1f)][SerializeField] private float _projectileSpeed = 6f;
    [Min(0.1f)][SerializeField] private float _projectileLifeTime = 2f;
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
        if (_projectilePrefab == null) return;

        Vector2 origin = _firePoint != null ? (Vector2)_firePoint.position : Rb.position;
        Vector2 dir = GetTargetDirection(origin);

        GameObject go = Instantiate(_projectilePrefab, origin, Quaternion.identity);
        fbdfbd_EnemyProjectile proj = go.GetComponent<fbdfbd_EnemyProjectile>();
        if (proj != null)
        {
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
