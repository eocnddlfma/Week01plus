using UnityEngine;
using System.Collections;

/// <summary>
/// 빠따공: 평소는 일반 공, 배트 휘두를 때 자신의 위치에서 또 다른 배트를 휘둘러서 공격
/// </summary>
public class BatOrbitalWeapon : OrbitalWeapon
{
    [Header("Bat Swing Attack")]
    [SerializeField] private float _swingRadius = 1.5f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Bat Visual")]
    [SerializeField] private Transform _miniBatPivot;   // 미니 배트 pivot (공 자식 오브젝트)
    [SerializeField] private float _swingAngle = 180f;  // 스윙 각도
    [SerializeField] private float _swingDuration = 0.25f;
    [SerializeField] private AnimationCurve _swingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Bat Reference")]
    [SerializeField] private BatWeaponManager _batManager;

    private bool _wasAttacking = false;

    protected override void Start()
    {
        base.Start();

        if (_batManager == null)
            _batManager = FindAnyObjectByType<BatWeaponManager>();
    }

    private void Update()
    {
        if (_batManager == null) return;

        bool isAttacking = _batManager.IsAttacking;

        // 스윙 시작 순간(엣지)만 감지
        if (isAttacking && !_wasAttacking)
        {
            PerformSwingAttack(_batManager.ChargePercent);
        }

        _wasAttacking = isAttacking;
    }

    private void PerformSwingAttack(float chargePercent)
    {
        // 시각적 스윙
        if (_miniBatPivot != null)
            StartCoroutine(SwingVisual(chargePercent));

        // 데미지
        Vector2 pivotPos = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(pivotPos, _swingRadius, _enemyLayer);

        bool isFullCharge = chargePercent >= 0.999f;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            if (!hit.TryGetComponent(out EnemyBase enemy)) continue;

            int damage = CalculateDamage(chargePercent);
            enemy.TakeDamage(damage, isFullCharge);
        }
    }

    private IEnumerator SwingVisual(float chargePercent)
    {
        // 플레이어 facing 방향 기준으로 시작 각도 결정
        float facingAngle = Mathf.Atan2(
            _batManager.transform.up.y,
            _batManager.transform.up.x) * Mathf.Rad2Deg;

        float startZ = facingAngle - _swingAngle * 0.5f;
        float endZ   = facingAngle + _swingAngle * 0.5f;

        float elapsed = 0f;
        float duration = _swingDuration * (1f - chargePercent * 0.3f); // 풀차지일수록 빠르게

        while (elapsed < duration)
        {
            float t = _swingCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            float currentZ = Mathf.Lerp(startZ, endZ, t);
            _miniBatPivot.eulerAngles = new Vector3(0f, 0f, currentZ);

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
