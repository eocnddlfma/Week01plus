using UnityEngine;

public class SSH_GravityOrbitalWeapon : Jaein_OrbitalWeapon
{
    [Header("Gravity Ability")]
    [Tooltip("중력 범위")]
    [SerializeField] private float _gravityRadius = 5f;
    [Tooltip("중력 세기")]
    [SerializeField] private float _gravityStrength = 25f;
    [Tooltip("중력 거리 배율")]
    [SerializeField] private float _gravityDivideValue = 2f;
    [Tooltip("중력을 적용할 적 레이어")]
    [SerializeField] private LayerMask _enemyLayer;

    private readonly Collider2D[] _nearbyColliders = new Collider2D[16];

    private void FixedUpdate()
    {
        if (_state == BallState.Orbit) return;
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        float distFromPlayer = Vector2.Distance(transform.position, _center.position)/_gravityDivideValue;

        if (distFromPlayer <= 0.001f) return;

        float currentRadius = _gravityRadius * distFromPlayer;
        float currentStrength = _gravityStrength * distFromPlayer;

        int count = Physics2D.OverlapCircle(transform.position, currentRadius, ContactFilter2D.noFilter, _nearbyColliders);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _nearbyColliders[i];

            // 공 체크 (Orbit 상태 제외)
            Jaein_OrbitalWeapon ball = col.GetComponentInParent<Jaein_OrbitalWeapon>();
            if (ball != null)
            {
                if (ball == this || ball.State == BallState.Orbit) continue;

                Vector2 toBallMe = (Vector2)transform.position - (Vector2)ball.transform.position;
                if (toBallMe.sqrMagnitude <= 0.001f) continue;

                ball.AddVelocity(toBallMe.normalized * currentStrength * 0.1f * Time.fixedDeltaTime);
                continue;
            }

            // 적 체크
            if ((_enemyLayer.value & (1 << col.gameObject.layer)) == 0) continue;

            fbdfbd_EnemyBase enemy = col.GetComponentInParent<fbdfbd_EnemyBase>();
            if (enemy == null) continue;

            Vector2 toEnemyMe = (Vector2)transform.position - (Vector2)enemy.transform.position;
            if (toEnemyMe.sqrMagnitude <= 0.001f) continue;

            enemy.AddExternalVelocity(toEnemyMe.normalized * currentStrength * Time.fixedDeltaTime);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, _gravityRadius);
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, _gravityRadius);

        if (_center != null)
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(_center.position, _gravityRadius);
        }
    }
#endif
}

