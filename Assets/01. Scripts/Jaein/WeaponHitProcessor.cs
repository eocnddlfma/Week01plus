using UnityEngine;
using System.Collections.Generic;

public class WeaponHitProcessor : MonoBehaviour
{
    [System.Serializable]
    private class HitTarget
    {
        public enum TargetType { Ball, Enemy }
        public TargetType targetType;
        public OrbitalWeapon ball;
        public EnemyBase enemy;
        public float angleToHit;
        public float distanceToPivot;
        public Vector2 targetPosition;
    }

    [Header("Hit Settings")]
    [SerializeField] private LayerMask _enemyReflectLayerMask;
    [SerializeField] private Transform _weaponTransform;
    [SerializeField] private Transform _endpointTransform;

    private float _currentDamageAmount = 1;
    private float _currentKnockbackForce = 3f;
    private float _currentChargePercentForAttack = 0f;
    private bool _isAttacking = false;

    private HashSet<OrbitalWeapon> _hitBallsThisAttack = new HashSet<OrbitalWeapon>();
    private HashSet<EnemyBase> _hitEnemiesThisAttack = new HashSet<EnemyBase>();
    private Vector2 PlayerPosition => (Vector2)transform.root.position;

    private HitStopController _hitStopController;

    private void Awake()
    {
        if (_weaponTransform == null) _weaponTransform = transform;
        if (_endpointTransform == null) _endpointTransform = transform.Find("EndPoint");
        _hitStopController = GetComponent<HitStopController>();
    }

    public void SetAttackState(bool attacking, float chargePercent, int damageAmount, float knockbackForce)
    {
        _isAttacking = attacking;
        _currentChargePercentForAttack = chargePercent;
        _currentDamageAmount = damageAmount;
        _currentKnockbackForce = knockbackForce;

        if (!attacking)
        {
            _hitEnemiesThisAttack.Clear();
            _hitBallsThisAttack.Clear();
        }
    }

    public void OnWeaponTriggerEnter2D(Collider2D collision)
    {
        OrbitalWeapon orbitalWeapon = collision.GetComponent<OrbitalWeapon>();
        if (orbitalWeapon != null)
        {
            orbitalWeapon.TriggerLaunchFromWeapon(_currentChargePercentForAttack);
            orbitalWeapon.PlayWeaponEffect(_weaponTransform, _currentChargePercentForAttack);
            if (_isAttacking && !_hitBallsThisAttack.Contains(orbitalWeapon))
                _hitBallsThisAttack.Add(orbitalWeapon);
            return;
        }

        if (!_isAttacking) return;

        int chargeLevel = Mathf.FloorToInt(_currentChargePercentForAttack * 4f);
        chargeLevel = Mathf.Clamp(chargeLevel, 0, 3);

        IEnemyProjectile projectile = collision.GetComponent<IEnemyProjectile>();
        if (projectile != null)
        {
            HandleEnemyProjectile(projectile, chargeLevel);
            return;
        }

        EnemyBase enemy = collision.GetComponent<EnemyBase>();
        if (enemy != null && !_hitEnemiesThisAttack.Contains(enemy))
        {
            ApplyEnemyHit(enemy);
        }
    }

    private void HandleEnemyProjectile(IEnemyProjectile projectile, int chargeLevel)
    {
        switch (chargeLevel)
        {
            case 0:
                break;
            case 1:
                PushEnemyProjectile(projectile);
                break;
            case 2:
                Destroy((projectile as Component).gameObject);
                break;
            case 3:
                ReflectEnemyProjectile(projectile);
                break;
        }
    }

    private void PushEnemyProjectile(IEnemyProjectile projectile)
    {
        Rigidbody2D rb = projectile.Rb;
        if (rb == null) return;

        Vector2 direction = (PlayerPosition - rb.position).normalized;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;

        float speed = rb.linearVelocity.magnitude;
        rb.linearVelocity = direction * speed * _currentKnockbackForce;
    }

    private void ReflectEnemyProjectile(IEnemyProjectile projectile)
    {
        EffectParticle effect = _weaponTransform.GetComponentInChildren<EffectParticle>();
        if (effect != null) effect.Play(_currentChargePercentForAttack);

        projectile.ReflectAsBatHit((int)_currentDamageAmount, _enemyReflectLayerMask, Color.white);
    }

    private void ApplyEnemyHit(EnemyBase enemy)
    {
        if (_currentDamageAmount > 0)
        {
            enemy.TakeDamage((int)_currentDamageAmount);
        }

        if (_hitStopController != null)
            _hitStopController.TryPlayWithCharge(_currentChargePercentForAttack);

        _hitEnemiesThisAttack.Add(enemy);

        Vector2 knockbackDir = ((Vector2)enemy.transform.position - PlayerPosition).normalized;
        enemy.AddExternalVelocity(knockbackDir * _currentKnockbackForce);
    }

    public bool IsAttacking => _isAttacking;
}
