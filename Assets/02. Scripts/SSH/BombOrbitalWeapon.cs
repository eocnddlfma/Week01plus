using UnityEngine;
using DG.Tweening;

/// <summary>
/// 폭탄공: 적 접촉 시 범위 폭발
/// </summary>
public class BombOrbitalWeapon : OrbitalWeapon
{
    [Header("Bomb Ability")]
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private int _explosionDamage = 3;
    [SerializeField] private float _explosionForce = 8f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color _explosionColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private float _explosionDuration = 0.3f;

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private bool _hasExploded = false;

    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_state == BallState.Orbit) return;

        base.OnTriggerEnter2D(other);

        // 적 접촉 시 폭발
        if (!_hasExploded && (_enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            _hasExploded = true;
            Explode(other.transform.position);
        }
    }

    private void Explode(Vector3 explosionCenter)
    {
        // 범위 내 모든 적에게 피해
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionCenter, _explosionRadius);
        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(_explosionDamage);

                // 폭발 방향으로 밀어냄
                Vector2 direction = ((Vector2)enemy.transform.position - (Vector2)explosionCenter).normalized;
                enemy.AddExternalVelocity(direction * _explosionForce, 0.3f);
            }
        }

        // 시각 효과
        if (_spriteRenderer != null)
        {
            _spriteRenderer.DOColor(_explosionColor, _explosionDuration * 0.5f)
                .OnComplete(() =>
                {
                    if (_spriteRenderer != null)
                        _spriteRenderer.DOColor(_originalColor, _explosionDuration * 0.5f);
                });
        }

        transform.DOScale(1.3f, _explosionDuration * 0.5f)
            .OnComplete(() =>
            {
                if (transform != null)
                    transform.DOScale(1f, _explosionDuration * 0.5f);
            });

        Debug.Log($"[폭탄공] 폭발!");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
#endif
}
