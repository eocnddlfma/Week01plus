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

    [Header("Knockback")]
    [SerializeField] private float _knockbackSpeedMult = 2f;
    [SerializeField] private float _knockbackMaxForce = 15f;
    [SerializeField] private float _knockbackMaxDuration = 0.15f;

    private float _currentDamageAmount = 1;
    private float _currentKnockbackForce = 3f;
    private float _currentChargePercentForAttack = 0f;
    private float _currentAttackPower = 1f;
    private bool _isAttacking = false;

    private HashSet<EnemyBase> _hitEnemiesThisAttack = new HashSet<EnemyBase>();
    private Vector2 PlayerPosition => (Vector2)transform.root.position;

    private HitStopController _hitStopController;

    private void Awake()
    {
        if (_weaponTransform == null) _weaponTransform = transform;
        if (_endpointTransform == null) _endpointTransform = transform.Find("EndPoint");
        _hitStopController = GetComponent<HitStopController>();
    }

    public void SetAttackState(bool attacking, float chargePercent, int damageAmount, float knockbackForce, float attackPower = 1f)
    {
        _isAttacking = attacking;
        _currentChargePercentForAttack = chargePercent;
        _currentDamageAmount = damageAmount;
        _currentKnockbackForce = knockbackForce;
        _currentAttackPower = attackPower;

        if (!attacking)
        {
            _hitEnemiesThisAttack.Clear();
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
                projectile.DisableProjectile();
                break;
            case 3:
                ReflectEnemyProjectile(projectile);
                break;
        }
    }

    private void PushEnemyProjectile(IEnemyProjectile projectile)
    {
        projectile.Deflect();
    }

    private void ReflectEnemyProjectile(IEnemyProjectile projectile)
    {
        EffectParticle effect = _weaponTransform.GetComponentInChildren<EffectParticle>();
        if (effect != null) effect.Play(_currentChargePercentForAttack);

        projectile.ReflectAsBatHit((int)_currentDamageAmount, _enemyReflectLayerMask);
    }

    // SwingSystem의 OverlapCircleAll에서 수집한 적 총알에게 호출
    public void ProcessProjectileHit(IEnemyProjectile projectile)
    {
        int chargeLevel = Mathf.FloorToInt(_currentChargePercentForAttack * 4f);
        chargeLevel = Mathf.Clamp(chargeLevel, 0, 3);
        HandleEnemyProjectile(projectile, chargeLevel);
    }

    // SwingSystem의 OverlapCircleAll에서 수집한 적에게 호출
    public void ProcessEnemyHit(EnemyBase enemy)
    {
        if (enemy == null || _hitEnemiesThisAttack.Contains(enemy)) return;
        ApplyEnemyHit(enemy);
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
        float t = Mathf.Clamp01(_currentKnockbackForce / _knockbackMaxForce);
        float duration = (1f - (1f - t) * (1f - t) * (1f - t)) * _knockbackMaxDuration;
        enemy.AddExternalVelocity(knockbackDir * (_currentKnockbackForce * _knockbackSpeedMult), duration);
    }

    public bool IsAttacking => _isAttacking;
}
