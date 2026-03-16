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

    [Header("Bat Visual")]
    [SerializeField] private Transform _miniBatPivot;
    [SerializeField] private Transform _endpointTransform;
    [SerializeField] private float _swingAngle = 180f;
    [SerializeField] private float _swingDuration = 0.25f;
    [SerializeField] private AnimationCurve _swingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Bat Reference")]
    [SerializeField] private BatWeaponManager _batManager;

    protected override bool EnableSpin => false;

    // ── 업그레이드 스태틱 ──
    public static float UpgradeSwingRadiusMult = 1f; // 홈런: 2f
    public static bool HalfHpOnHit = false;           // 빠따로 맞아볼래?

    private Vector3 _initialMiniBatScale;
    private bool _isSwinging = false;

    protected override void Start()
    {
        base.Start();
        if (_batManager == null)
            _batManager = FindAnyObjectByType<BatWeaponManager>();

        if (_batManager != null)
        {
            _batManager.OnSwingStarted += PerformSwingAttack;
            Debug.Log($"[BatBall] BatWeaponManager 연결 성공: {_batManager.gameObject.name}");
        }
        else
        {
            Debug.LogError("[BatBall] BatWeaponManager를 찾지 못했습니다! 이벤트 구독 실패.");
        }

        if (_miniBatPivot != null)
        {
            _initialMiniBatScale = _miniBatPivot.localScale;
            ApplySwingRadiusVisual();

            if (_endpointTransform == null)
                _endpointTransform = _miniBatPivot.Find("EndPoint");
        }

        if (_endpointTransform == null)
            _endpointTransform = transform.Find("EndPoint");

        Debug.Log($"[BatBall] EndPoint: {(_endpointTransform != null ? _endpointTransform.gameObject.name : "없음!")}");

    }

    protected override void OnDestroy()
    {
        if (_batManager != null)
            _batManager.OnSwingStarted -= PerformSwingAttack;
        base.OnDestroy();
    }

    public void ApplySwingRadiusVisual()
    {
        if (_miniBatPivot == null) return;
        _miniBatPivot.localScale = new Vector3(
            _initialMiniBatScale.x,
            _initialMiniBatScale.y * UpgradeSwingRadiusMult,
            _initialMiniBatScale.z);
    }

    protected override void Update()
    {
        base.Update();

        // 스윙 중이 아닐 때 미니 배트를 플레이어 바라보는 방향으로 추적
        if (!_isSwinging && _miniBatPivot != null && PlayerController != null)
        {
            Vector2 facing = PlayerController.FacingDirection;
            if (facing.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
                _miniBatPivot.eulerAngles = new Vector3(0f, 0f, angle);
            }
        }
    }

    private void PerformSwingAttack(float chargePercent, int batDamage, float knockbackForce)
    {
        Debug.Log($"[BatBall] PerformSwingAttack 호출 - charge:{chargePercent:F2} dmg:{batDamage} kb:{knockbackForce:F1}");
        StartCoroutine(SwingVisual(chargePercent, batDamage, knockbackForce));
    }

    private void ApplySwingHits(int batDamage, float knockbackForce, bool isFullCharge)
    {
        Vector2 hitCenter = _endpointTransform != null
            ? (Vector2)_endpointTransform.position
            : (Vector2)transform.position;
        float radius = _endpointTransform != null ? _swingRadius * UpgradeSwingRadiusMult : _swingRadius * UpgradeSwingRadiusMult;
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, radius);

        Debug.Log($"[BatBall] ApplySwingHits - 중심:{hitCenter} 반경:{radius:F2} 감지된 콜라이더:{hits.Length}개");

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            Debug.Log($"[BatBall] 콜라이더 감지: {hit.gameObject.name} (layer:{LayerMask.LayerToName(hit.gameObject.layer)})");
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy == null)
            {
                Debug.Log($"[BatBall]  └ EnemyBase 없음, 스킵");
                continue;
            }

            int damage = HalfHpOnHit && !enemy.CompareTag("Boss")
                ? Mathf.Max(1, enemy.Hp / 2)
                : batDamage;

            Debug.Log($"[BatBall]  └ 적 히트: {enemy.name} 데미지:{damage}");
            enemy.TakeDamage(damage, isFullCharge);

            Vector2 knockbackDir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
            float t = Mathf.Clamp01(knockbackForce / 15f);
            float duration = (1f - (1f - t) * (1f - t) * (1f - t)) * 0.15f;
            enemy.AddExternalVelocity(knockbackDir * (knockbackForce * 2f), duration);
        }
    }

    private IEnumerator SwingVisual(float chargePercent, int batDamage, float knockbackForce)
    {
        _isSwinging = true;
        bool hitProcessed = false;
        bool isFullCharge = chargePercent >= 0.999f;

        float facingAngle = _miniBatPivot.eulerAngles.z;
        float startZ = facingAngle - _swingAngle * 0.5f;
        float endZ   = facingAngle + _swingAngle * 0.5f;

        float elapsed = 0f;
        float duration = _swingDuration * (1f - chargePercent * 0.3f);

        while (elapsed < duration)
        {
            float t = _swingCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            _miniBatPivot.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startZ, endZ, t));

            // 스윙 중간 지점에서 히트 판정
            if (!hitProcessed && t >= 0.5f)
            {
                ApplySwingHits(batDamage, knockbackForce, isFullCharge);
                hitProcessed = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        _miniBatPivot.eulerAngles = new Vector3(0f, 0f, endZ);

        _isSwinging = false;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }
}
