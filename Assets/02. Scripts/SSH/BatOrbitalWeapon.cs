using UnityEngine;
using System.Collections;

/// <summary>
/// 빠따공: 배트 휘두를 때 자신의 위치에서 스윙 공격
/// 업그레이드: 홈런(UpgradeSwingRadiusMult), 빠따로 맞아볼래?(HalfHpOnHit)
/// </summary>
public class BatOrbitalWeapon : OrbitalWeapon
{
    [Header("Bat Swing Attack")]
    [SerializeField] private float _swingRadius = 1.5f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Bat Visual")]
    [SerializeField] private Transform _miniBatPivot;
    [SerializeField] private float _swingAngle = 180f;
    [SerializeField] private float _swingDuration = 0.25f;
    [SerializeField] private AnimationCurve _swingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Bat Reference")]
    [SerializeField] private BatWeaponManager _batManager;

    // ── 업그레이드 스태틱 ──
    public static float UpgradeSwingRadiusMult = 1f; // 홈런: 2f
    public static bool HalfHpOnHit = false;           // 빠따로 맞아볼래?

    private bool _wasAttacking = false;
    private Vector3 _initialMiniBatScale;

    protected override void Start()
    {
        base.Start();
        if (_batManager == null)
            _batManager = FindAnyObjectByType<BatWeaponManager>();

        if (_miniBatPivot != null)
        {
            _initialMiniBatScale = _miniBatPivot.localScale;
            ApplySwingRadiusVisual();
        }
    }

    public void ApplySwingRadiusVisual()
    {
        if (_miniBatPivot == null) return;
        _miniBatPivot.localScale = new Vector3(
            _initialMiniBatScale.x,
            _initialMiniBatScale.y * UpgradeSwingRadiusMult,
            _initialMiniBatScale.z);
    }

    private void Update()
    {
        if (_batManager == null) return;

        bool isAttacking = _batManager.IsAttacking;
        if (isAttacking && !_wasAttacking)
            PerformSwingAttack(_batManager.ChargePercent);
        _wasAttacking = isAttacking;
    }

    private void PerformSwingAttack(float chargePercent)
    {
        if (_miniBatPivot != null)
            StartCoroutine(SwingVisual(chargePercent));

        float radius = _swingRadius * UpgradeSwingRadiusMult;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, _enemyLayer);
        bool isFullCharge = chargePercent >= 0.999f;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (!hit.TryGetComponent(out EnemyBase enemy)) continue;

            int damage;
            if (HalfHpOnHit && !enemy.CompareTag("Boss"))
                damage = Mathf.Max(1, enemy.Hp / 2);
            else
                damage = CalculateDamage(chargePercent);

            enemy.TakeDamage(damage, isFullCharge);
        }
    }

    private IEnumerator SwingVisual(float chargePercent)
    {
        float facingAngle = Mathf.Atan2(
            _batManager.transform.up.y,
            _batManager.transform.up.x) * Mathf.Rad2Deg;

        float startZ = facingAngle - _swingAngle * 0.5f;
        float endZ   = facingAngle + _swingAngle * 0.5f;

        float elapsed = 0f;
        float duration = _swingDuration * (1f - chargePercent * 0.3f);

        while (elapsed < duration)
        {
            float t = _swingCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            _miniBatPivot.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startZ, endZ, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        _miniBatPivot.eulerAngles = new Vector3(0f, 0f, endZ);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }
}
