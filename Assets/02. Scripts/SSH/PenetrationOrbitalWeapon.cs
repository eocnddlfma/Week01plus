using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 관통공: 적 접촉 시 가속하며 관통
/// 업그레이드: 펜싱마스터(UpgradeDurationMult), 연속찌르기(ContinuousStab)
/// </summary>
public class PenetrationOrbitalWeapon : OrbitalWeapon
{
    [Header("Penetration Ability")]
    [SerializeField] private float _accelerationAmount = 1.5f;
    [SerializeField] private float _speedBoostDuration = 0.5f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color _boostColor = Color.cyan;

    // ── 업그레이드 스태틱 ──
    public static float UpgradeDurationMult = 1f; // 펜싱마스터: 발사 거리 배율
    public static bool ContinuousStab = false;     // 연속찌르기: 0.5초 내 재충돌 1.2배

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private HashSet<EnemyBase> _hitEnemies = new();
    private float _speedBoostEndTime = 0f;
    private float _baseLaunchDuration;
    private float _lastHitTime = -10f;

    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        _baseLaunchDuration = _launchDuration;
        _launchDuration = _baseLaunchDuration * UpgradeDurationMult;
    }

    /// <summary>
    /// UpgradeManager에서 펜싱마스터 업그레이드 후 기존 인스턴스에 적용
    /// </summary>
    public void RefreshDurationUpgrade()
    {
        _launchDuration = _baseLaunchDuration * UpgradeDurationMult;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_state == BallState.Orbit) return;

        base.OnTriggerEnter2D(other);

        if ((_enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null && !_hitEnemies.Contains(enemy))
            {
                _hitEnemies.Add(enemy);
                _lastHitTime = Time.time;

                enemy.TakeDamage(CalculateDamage(ChargePercent, AttackPower));

                Accelerate();
            }
        }
    }

    protected override int CalculateDamage(float chargePercent, float attackPower = 1f)
    {
        int damage = base.CalculateDamage(chargePercent, attackPower);
        if (ContinuousStab && _lastHitTime > 0f && Time.time - _lastHitTime < 0.5f)
            damage = Mathf.RoundToInt(damage * 1.2f);
        return damage;
    }

    private void Accelerate()
    {
        if (_velocity.sqrMagnitude > 0.01f)
        {
            _velocity = _velocity.normalized * (_velocity.magnitude * _accelerationAmount);
            _speedBoostEndTime = Time.time + _speedBoostDuration;

            if (_spriteRenderer != null)
                _spriteRenderer.color = _boostColor;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (Time.time >= _speedBoostEndTime && _spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        _hitEnemies.Clear();
        _lastHitTime = -10f;
        base.RejoinOrbit(currentPos);
    }
}
