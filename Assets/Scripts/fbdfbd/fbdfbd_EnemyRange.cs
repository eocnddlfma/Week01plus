using UnityEngine;

public class fbdfbd_EnemyRange : fbdfbd_EnemyBase
{
    [Header("Ranged")]
    [Min(0.1f)][SerializeField] private float attackRange = 6f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [Min(0.1f)][SerializeField] private float projectileSpeed = 6f;
    [Min(0.1f)][SerializeField] private float projectileLifeTime = 2f;
    [Min(1)][SerializeField] private int damage = 1;
    [SerializeField] private LayerMask targetMask;

    protected override bool CanAttack(float distanceToTarget) => distanceToTarget <= attackRange;

    protected override void Awake()
    {
        base.Awake();
        if (firePoint == null) firePoint = transform;
    }

    protected override void DoAttack()
    {
        if (projectilePrefab == null) return;

        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : Rb.position;
        Vector2 dir = GetTargetDirection(origin);

        GameObject go = Instantiate(projectilePrefab, origin, Quaternion.identity);
        fbdfbd_EnemyProjectile proj = go.GetComponent<fbdfbd_EnemyProjectile>();
        if (proj != null)
        {
            proj.Init(damage, dir, projectileSpeed, projectileLifeTime, targetMask, gameObject);
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
