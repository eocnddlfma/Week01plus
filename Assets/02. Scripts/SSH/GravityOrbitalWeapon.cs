using UnityEngine;

/// <summary>
/// 중력공: 주변 적과 다른 공들을 끌어당김
/// 업그레이드: 내게로 와(UpgradeStrengthMult), 저리가!(Repel)
/// </summary>
public class GravityOrbitalWeapon : OrbitalWeapon
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

    // ── 업그레이드 스태틱 ──
    public static float UpgradeStrengthMult = 1f; // 내게로 와: 2f
    public static bool Repel = false;              // 저리가!: 인력 → 척력

    private readonly Collider2D[] _nearbyColliders = new Collider2D[16];

    private void FixedUpdate()
    {
        if (!ShouldApplyGravity()) return;
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        float distFromPlayer = Vector2.Distance(transform.position, _center.position) / _gravityDivideValue;
        if (distFromPlayer <= 0.001f) return;

        float currentRadius = _gravityRadius * distFromPlayer;
        float currentStrength = _gravityStrength * distFromPlayer * UpgradeStrengthMult;
        float dirMult = Repel ? -1f : 1f;

        int count = Physics2D.OverlapCircle(transform.position, currentRadius, ContactFilter2D.noFilter, _nearbyColliders);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _nearbyColliders[i];

            OrbitalWeapon ball = col.GetComponentInParent<OrbitalWeapon>();
            if (ball != null)
            {
                if (ball == this || !ball.ShouldApplyGravity()) continue;
                Vector2 toBallMe = (Vector2)transform.position - (Vector2)ball.transform.position;
                if (toBallMe.sqrMagnitude <= 0.001f) continue;
                ball.AddVelocity(toBallMe.normalized * currentStrength * dirMult * 0.3f * Time.fixedDeltaTime);
                continue;
            }

            if ((_enemyLayer.value & (1 << col.gameObject.layer)) == 0) continue;

            EnemyBase enemy = col.GetComponentInParent<EnemyBase>();
            if (enemy == null) continue;

            Vector2 toEnemyMe = (Vector2)transform.position - (Vector2)enemy.transform.position;
            if (toEnemyMe.sqrMagnitude <= 0.001f) continue;

            enemy.AddExternalVelocity(toEnemyMe.normalized * currentStrength * dirMult * Time.fixedDeltaTime);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, _gravityRadius);
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, _gravityRadius);
    }
#endif
}
