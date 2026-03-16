using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 관통공: 적 접촉 시 가속하며 관통
/// - 같은 프레임에 여러 적을 동시에 통과하면 데미지 × 적 수, 가속은 1회
/// - 속도에 비례해 데미지 증가
/// - 너무 멀어지면 추가 중력 적용
/// 업그레이드: 펜싱마스터(UpgradeDurationMult), 연속찌르기(ContinuousStab)
/// </summary>
public class PenetrationOrbitalWeapon : OrbitalWeapon
{
    [Header("Penetration Ability")]
    [SerializeField] private float _speedBoostAmount = 5f;  // 적 관통마다 고정 속도 추가량
    [SerializeField] private float _maxSpeed = 40f;         // 속도 상한
    [SerializeField] private float _speedBoostDuration = 0.5f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Speed Damage")]
    [SerializeField] private float _referenceSpeed = 14f; // 이 속도 기준으로 데미지 배율 계산


    [Header("Visual")]
    [SerializeField] private Color _boostColor = Color.cyan;

    // ── 업그레이드 스태틱 ──
    public static float UpgradeDurationMult = 1f;
    public static bool ContinuousStab = false;

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private readonly HashSet<EnemyBase> _hitEnemies = new();
    private float _speedBoostEndTime = 0f;
    private float _baseLaunchDuration;
    private float _lastHitTime = -10f;

    // 프레임 단위 배치 처리
    private readonly List<EnemyBase> _frameHits = new List<EnemyBase>();

    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        _baseLaunchDuration = _launchDuration;
        _launchDuration = _baseLaunchDuration * UpgradeDurationMult;
    }

    public void RefreshDurationUpgrade()
    {
        _launchDuration = _baseLaunchDuration * UpgradeDurationMult;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_state == BallState.Orbit) return;

        if ((_enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;
        if (_hitEnemies.Contains(enemy) || _frameHits.Contains(enemy)) return;

        _frameHits.Add(enemy);
    }

    private void LateUpdate()
    {
        if (_frameHits.Count == 0) return;

        int hitCount = _frameHits.Count;
        // 연속찌르기 체크는 이전 _lastHitTime 기준으로, 데미지 먼저 계산
        int damage = CalculateDamage(ChargePercent, AttackPower) * hitCount;
        _lastHitTime = Time.time;

        foreach (var enemy in _frameHits)
        {
            _hitEnemies.Add(enemy);
            enemy.TakeDamage(damage);
        }

        Accelerate(); // 몇 명이든 가속은 1회
        _frameHits.Clear();
    }

    protected override void Update()
    {
        base.Update();

        // 부스트 색상 복구
        if (Time.time >= _speedBoostEndTime && _spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
    }

    protected override int CalculateDamage(float chargePercent, float attackPower = 1f)
    {
        int baseDamage = base.CalculateDamage(chargePercent, attackPower);

        // 속도 비례 배율 (기준 속도 이하면 1배, 초과할수록 증가)
        float speedMult = Mathf.Max(1f, _velocity.magnitude / _referenceSpeed);
        baseDamage = Mathf.RoundToInt(baseDamage * speedMult);

        // 연속찌르기
        if (ContinuousStab && _lastHitTime > 0f && Time.time - _lastHitTime < 0.5f)
            baseDamage = Mathf.RoundToInt(baseDamage * 1.2f);

        return baseDamage;
    }

    private void Accelerate()
    {
        if (_velocity.sqrMagnitude < 0.01f) return;

        float newSpeed = Mathf.Min(_velocity.magnitude + _speedBoostAmount, _maxSpeed);
        _velocity = _velocity.normalized * newSpeed;
        _speedBoostEndTime = Time.time + _speedBoostDuration;

        if (_spriteRenderer != null)
            _spriteRenderer.color = _boostColor;
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        _hitEnemies.Clear();
        _frameHits.Clear();
        _lastHitTime = -10f;
        base.RejoinOrbit(currentPos);
    }
}
