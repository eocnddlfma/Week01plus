using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponSwingSystem : MonoBehaviour
{
    [System.Serializable]
    private class HitTarget
    {
        public enum TargetType { Ball, Enemy, Projectile }
        public TargetType targetType;
        public OrbitalWeapon ball;
        public EnemyBase enemy;
        public IEnemyProjectile projectile;
        public float angleToHit;
        public float distanceToPivot;
        public Vector2 targetPosition;
    }

    [Header("Weapon References")]
    [SerializeField] private Transform _weaponTransform;
    [SerializeField] private Transform _endpointTransform;

    [Header("Swing Settings")]
    [SerializeField] private AnimationCurve _rotationEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float _hitProcessPercent = 0.7f;
    [SerializeField] private float _quickTurnBackAngle = 1.5f;

    private float _currentRotationAngle = 0f;
    private Vector3 _initialWeaponPosition;
    private Quaternion _initialPivotRotation;
    private Vector3 _initialPivotScale;
    private Transform _pivotParent;

    private List<HitTarget> _hitTargetsThisAttack = new List<HitTarget>();
    private WeaponChargeSystem.ChargeLevel[] _chargeLevels;
    private HitStopController _hitStopController;
    private WeaponHitProcessor _hitProcessor;
    private float _swingChargePercent;
    private float _swingAttackPower;

    public event System.Action OnSwingComplete;

    private void Awake()
    {
        if (_weaponTransform == null) _weaponTransform = transform;
        if (_endpointTransform == null) _endpointTransform = transform.Find("EndPoint");

        _initialWeaponPosition = _weaponTransform.localPosition;

        Transform pivot = transform.parent;
        if (pivot != null)
        {
            _initialPivotRotation = pivot.localRotation;
            _initialPivotScale = pivot.localScale;
            _pivotParent = pivot.parent;
        }

        _hitStopController = GetComponent<HitStopController>();
        _hitProcessor = GetComponent<WeaponHitProcessor>();
    }

    public void ExecuteSwing(float chargePercent, WeaponChargeSystem.ChargeLevel[] chargeLevels)
    {
        if (_chargeLevels == null)
            _chargeLevels = chargeLevels;

        StartCoroutine(PerformSwing(chargePercent));
    }

    private IEnumerator PerformSwing(float chargePercent)
    {
        chargePercent = Mathf.Clamp01(chargePercent);

        int maxChargeLevel = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.MaxChargeLevel : 4;
        int maxIdx = Mathf.Clamp(maxChargeLevel - 1, 0, 3);

        WeaponChargeSystem.ChargeLevel minLevel = _chargeLevels[0];
        WeaponChargeSystem.ChargeLevel maxLevel = _chargeLevels[maxIdx];

        float dynamicRotationAngle = Mathf.Lerp(minLevel.rotationAngle, maxLevel.rotationAngle, chargePercent);
        float dynamicRotationDuration = Mathf.Lerp(minLevel.rotationDuration, maxLevel.rotationDuration, chargePercent);
        float batDmgMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.BatDamageMult : 1f;
        float kbMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.KnockbackMult : 1f;
        int dynamicDamageAmount = Mathf.RoundToInt(Mathf.Lerp(minLevel.damageAmount, maxLevel.damageAmount, chargePercent) * batDmgMult);
        float dynamicKnockbackForce = Mathf.Lerp(minLevel.knockbackForce, maxLevel.knockbackForce, chargePercent) * kbMult;
        int chargeLevel = Mathf.Min(maxIdx, Mathf.FloorToInt(chargePercent * 4f));
        float dynamicAttackPower = _chargeLevels[chargeLevel].attackPower;

        _swingChargePercent = chargePercent;
        _swingAttackPower = dynamicAttackPower;

        _hitProcessor.SetAttackState(true, chargePercent, dynamicDamageAmount, dynamicKnockbackForce, dynamicAttackPower);

        Transform pivot = transform.parent;
        if (pivot == null)
        {
            _hitProcessor.SetAttackState(false, 0, 0, 0);
            yield break;
        }

        float rangeMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.AttackRangeMult : 1f;
        float hitRadius = Vector2.Distance(pivot.position, _endpointTransform.position) * rangeMult;

        float facingZ = _pivotParent != null ? _pivotParent.eulerAngles.z : 0f;
        float centerZ = facingZ + _initialPivotRotation.eulerAngles.z + 90f;
        float startEulerZ = centerZ - dynamicRotationAngle * 0.5f;
        float targetEulerZ = centerZ + dynamicRotationAngle * 0.5f;

        CollectHitTargets(pivot, hitRadius, startEulerZ, targetEulerZ);

        if (_hitTargetsThisAttack.Count > 0)
        {
            yield return StartCoroutine(SwingWithHits(pivot, startEulerZ, targetEulerZ, dynamicRotationDuration, chargePercent, minLevel));
        }
        else
        {
            yield return StartCoroutine(SwingWithoutHits(pivot, startEulerZ, targetEulerZ, dynamicRotationDuration));
        }

        _hitProcessor.SetAttackState(false, 0, 0, 0);
        OnSwingComplete?.Invoke();
    }

    private IEnumerator SwingWithHits(Transform pivot, float startEulerZ, float targetEulerZ, float rotationDuration, float chargePercent, WeaponChargeSystem.ChargeLevel minLevel)
    {
        HitTarget firstTarget = _hitTargetsThisAttack[0];
        float firstTargetAbsAngle = ((startEulerZ + firstTarget.angleToHit) % 360f) - _quickTurnBackAngle;

        float quickRotationDuration = rotationDuration * 0.2f;
        float angleToFirst = (firstTargetAbsAngle - startEulerZ + 360f) % 360f;
        float elapsedQuick = 0f;

        while (elapsedQuick < quickRotationDuration && pivot != null)
        {
            float progress = Mathf.Clamp01(elapsedQuick / quickRotationDuration);
            float easedProgress = _rotationEasingCurve.Evaluate(progress);
            float currentZ = (startEulerZ + angleToFirst * easedProgress) % 360f;
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, currentZ);
            _currentRotationAngle = currentZ;
            elapsedQuick += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        float hitStopDurationMult = Mathf.Lerp(minLevel.hitStopDurationMult, _chargeLevels[3].hitStopDurationMult, chargePercent);
        if (_hitStopController != null)
            _hitStopController.TryPlayWithChargeAndHitCount(chargePercent, 1, hitStopDurationMult);

        float remainingDuration = rotationDuration - quickRotationDuration;
        float angleFromFirstToTarget = (targetEulerZ - firstTargetAbsAngle + 360f) % 360f;
        float elapsedRemain = 0f;
        bool hitProcessed = false;

        while (elapsedRemain < remainingDuration && pivot != null)
        {
            float progress = Mathf.Clamp01(elapsedRemain / remainingDuration);
            float easedProgress = _rotationEasingCurve.Evaluate(progress);
            float currentZ = (firstTargetAbsAngle + angleFromFirstToTarget * easedProgress) % 360f;
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, currentZ);
            _currentRotationAngle = currentZ;

            if (!hitProcessed && progress >= _hitProcessPercent)
            {
                for (int i = 0; i < _hitTargetsThisAttack.Count; i++)
                {
                    ProcessHitTarget(_hitTargetsThisAttack[i]);
                }
                hitProcessed = true;
            }

            elapsedRemain += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (pivot != null)
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, targetEulerZ);
    }

    private IEnumerator SwingWithoutHits(Transform pivot, float startEulerZ, float targetEulerZ, float rotationDuration)
    {
        float elapsedTime = 0f;
        float totalAngleOnly = (targetEulerZ - startEulerZ + 360f) % 360f;
        while (elapsedTime < rotationDuration && pivot != null)
        {
            float progress = Mathf.Clamp01(elapsedTime / rotationDuration);
            float easedProgress = _rotationEasingCurve.Evaluate(progress);
            float currentZ = (startEulerZ + totalAngleOnly * easedProgress) % 360f;
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, currentZ);
            _currentRotationAngle = currentZ;
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        if (pivot != null)
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, targetEulerZ);
    }

    private void ProcessHitTarget(HitTarget target)
    {
        if (target.targetType == HitTarget.TargetType.Ball)
        {
            target.ball.TriggerLaunchFromWeapon(_swingChargePercent, _swingAttackPower);
            target.ball.PlayWeaponEffect(_weaponTransform, _swingChargePercent);
        }
        else if (target.targetType == HitTarget.TargetType.Enemy)
        {
            _hitProcessor.ProcessEnemyHit(target.enemy);
        }
        else if (target.targetType == HitTarget.TargetType.Projectile)
        {
            _hitProcessor.ProcessProjectileHit(target.projectile);
        }
    }

    private void CollectHitTargets(Transform pivot, float radius, float startAngle, float endAngle)
    {
        _hitTargetsThisAttack.Clear();

        Collider2D[] hits = Physics2D.OverlapCircleAll(pivot.position, radius);

        startAngle = NormalizeAngle(startAngle);
        endAngle = NormalizeAngle(endAngle);

        float ToRelativeAngle(float angle)
        {
            float rel = angle - startAngle;
            if (rel < 0) rel += 360f;
            return rel;
        }

        HashSet<OrbitalWeapon> ballsAdded = new HashSet<OrbitalWeapon>();
        HashSet<EnemyBase> enemiesAdded = new HashSet<EnemyBase>();
        HashSet<IEnemyProjectile> projectilesAdded = new HashSet<IEnemyProjectile>();

        foreach (Collider2D hit in hits)
        {
            OrbitalWeapon orbitalWeapon = hit.GetComponent<OrbitalWeapon>();
            if (orbitalWeapon != null && !ballsAdded.Contains(orbitalWeapon))
            {
                Vector2 dir = ((Vector2)orbitalWeapon.transform.position - (Vector2)pivot.position);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle = NormalizeAngle(angle);
                float relAngle = ToRelativeAngle(angle);
                float distance = dir.magnitude;
                _hitTargetsThisAttack.Add(new HitTarget
                {
                    targetType = HitTarget.TargetType.Ball,
                    ball = orbitalWeapon,
                    angleToHit = relAngle,
                    distanceToPivot = distance,
                    targetPosition = orbitalWeapon.transform.position
                });
                ballsAdded.Add(orbitalWeapon);
            }

            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && !enemiesAdded.Contains(enemy))
            {
                Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)pivot.position);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle = NormalizeAngle(angle);
                float relAngle = ToRelativeAngle(angle);
                float distance = dir.magnitude;
                if (IsAngleInRange(angle, startAngle, endAngle))
                {
                    _hitTargetsThisAttack.Add(new HitTarget
                    {
                        targetType = HitTarget.TargetType.Enemy,
                        enemy = enemy,
                        angleToHit = relAngle,
                        distanceToPivot = distance,
                        targetPosition = enemy.transform.position
                    });
                    enemiesAdded.Add(enemy);
                }
            }

            IEnemyProjectile projectile = hit.GetComponent<IEnemyProjectile>();
            if (projectile != null && !projectile.IsReflected && !projectilesAdded.Contains(projectile))
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)pivot.position);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle = NormalizeAngle(angle);
                float relAngle = ToRelativeAngle(angle);
                float distance = dir.magnitude;
                if (IsAngleInRange(angle, startAngle, endAngle))
                {
                    _hitTargetsThisAttack.Add(new HitTarget
                    {
                        targetType = HitTarget.TargetType.Projectile,
                        projectile = projectile,
                        angleToHit = relAngle,
                        distanceToPivot = distance,
                        targetPosition = hit.transform.position
                    });
                    projectilesAdded.Add(projectile);
                }
            }
        }

        _hitTargetsThisAttack.Sort((a, b) => {
            int angleComp = a.angleToHit.CompareTo(b.angleToHit);
            if (angleComp != 0) return angleComp;
            return a.distanceToPivot.CompareTo(b.distanceToPivot);
        });
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

    private bool IsAngleInRange(float angle, float startAngle, float endAngle)
    {
        if (endAngle >= startAngle)
        {
            return angle >= startAngle && angle <= endAngle;
        }
        else
        {
            return angle >= startAngle || angle <= endAngle;
        }
    }
}
