using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 관통공: 적 접촉 시 가속하며 관통
/// </summary>
public class PenetrationOrbitalWeapon : OrbitalWeapon
{
    [Header("Penetration Ability")]
    [SerializeField] private float _accelerationAmount = 1.5f;
    [SerializeField] private float _speedBoostDuration = 0.5f;
    [SerializeField] private int _penetrationDamage = 2;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color _boostColor = Color.cyan;

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private HashSet<EnemyBase> _hitEnemies = new();
    private float _speedBoostEndTime = 0f;

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

        // 적 관통 및 가속
        if ((_enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null && !_hitEnemies.Contains(enemy))
            {
                _hitEnemies.Add(enemy);
                enemy.TakeDamage(_penetrationDamage);
                Accelerate();
            }
        }
    }

    private void Accelerate()
    {
        if (_velocity.sqrMagnitude > 0.01f)
        {
            Vector2 currentDir = _velocity.normalized;
            float newSpeed = _velocity.magnitude * _accelerationAmount;
            _velocity = currentDir * newSpeed;

            _speedBoostEndTime = Time.time + _speedBoostDuration;

            if (_spriteRenderer != null)
                _spriteRenderer.color = _boostColor;

            Debug.Log($"[관통공] 관통! 가속도: {_accelerationAmount}배");
        }
    }

    private void Update()
    {
        // 가속 종료
        if (Time.time >= _speedBoostEndTime && _spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        _hitEnemies.Clear();
        base.RejoinOrbit(currentPos);
    }
}
