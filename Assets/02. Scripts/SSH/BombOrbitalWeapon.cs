using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// 폭탄공: 적 접촉 시 범위 폭발
/// 업그레이드: 폭탄 받아라(UpgradeExplosionMult), 터져버렷(AutoExplode)
/// </summary>
public class BombOrbitalWeapon : OrbitalWeapon
{
    [Header("Bomb Ability")]
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private float _explosionForce = 8f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color _explosionColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private float _explosionDuration = 0.3f;

    // ── 업그레이드 스태틱 ──
    public static float UpgradeExplosionMult = 1f; // 폭탄 받아라: 2f
    public static bool AutoExplode = false;         // 터져버렷

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private bool _hasExploded = false;
    private Coroutine _autoExplodeRoutine;
    private ParticleSystem _explosionEffect;

    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        _explosionEffect = GetComponentInChildren<ParticleSystem>(includeInactive: true);
    }

    protected override void Update()
    {
        base.Update();
        if (AutoExplode && State == BallState.Launched && _autoExplodeRoutine == null)
            _autoExplodeRoutine = StartCoroutine(AutoExplodeRoutine());
    }

    private IEnumerator AutoExplodeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (!AutoExplode || State != BallState.Launched)
                break;
            _hasExploded = false;
            Explode(transform.position);
            _hasExploded = false; // 자연 트리거도 허용
        }
        _autoExplodeRoutine = null;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_state == BallState.Orbit) return;

        base.OnTriggerEnter2D(other);

        if (!_hasExploded && (_enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            _hasExploded = true;
            Explode(other.transform.position);
        }
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        _hasExploded = false;
        if (_autoExplodeRoutine != null)
        {
            StopCoroutine(_autoExplodeRoutine);
            _autoExplodeRoutine = null;
        }
        base.RejoinOrbit(currentPos);
    }

    private void Explode(Vector3 explosionCenter)
    {
        float effectiveRadius = _explosionRadius * UpgradeExplosionMult;
        int effectiveDamage = Mathf.RoundToInt(CalculateDamage(ChargePercent, AttackPower) * UpgradeExplosionMult);

        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionCenter, effectiveRadius);
        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(effectiveDamage);
                Vector2 direction = ((Vector2)enemy.transform.position - (Vector2)explosionCenter).normalized;
                enemy.AddExternalVelocity(direction * _explosionForce, 0.3f);
            }
        }

        if (_explosionEffect != null)
        {
            _explosionEffect.transform.position = explosionCenter;
            _explosionEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _explosionEffect.Play();
        }

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

        Debug.Log($"[폭탄공] 폭발! 반경:{effectiveRadius:F1} 데미지:{effectiveDamage}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius * UpgradeExplosionMult);
    }
#endif
}
