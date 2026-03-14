using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 배트 무기를 관리하는 시스템
/// - 4단계 차지 레벨
/// - 물리 기반 회전
/// - 충돌 처리
/// </summary>
public class Jaein_BatWeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class ChargeLevel
    {
        public string levelName;
        [Range(0f, 360f)] public float rotationAngle = 180f;
        public float attackPower = 1f;
        public float rotationDuration = 0.3f; // 초(seconds) 단위
        public Color weaponColor = Color.white;
        public float weaponSizeMultiplier = 1f;
        public bool knockbackEnemies = true;
        public float knockbackForce = 5f;
        public int damageAmount = 1;
        public float hitStopDurationMult = 1f; // 히트스탑 듀레이션 멀티플라이어
    }

    [System.Serializable]
    private class HitTarget
    {
        public enum TargetType { Ball, Enemy }

        public TargetType targetType;
        public Jaein_OrbitalWeapon ball;
        public fbdfbd_EnemyBase enemy;
        public float angleToHit; // 시계방향 각도(0~360)
        public float distanceToPivot; // 피봇~대상 거리
        public Vector2 targetPosition;
    }

    [Header("Charge Levels")]
    [SerializeField] private ChargeLevel[] _chargeLevels = new ChargeLevel[4];

    [Header("Weapon References")]
    [SerializeField] private Transform _weaponTransform;
    [SerializeField] private SpriteRenderer _weaponSpriteRenderer;
    [SerializeField] private Transform _endpointTransform; // 무기의 끝 위치
    [SerializeField] private float _hitRadius = 2f; // 타격 범위 반지름

    [Header("Attack Settings")]
    [SerializeField] private LayerMask _enemyReflectLayerMask; // Level3 반사 탄환이 맞힐 적 레이어
    [SerializeField] private float _chargeCooldown = 0.3f;
    [SerializeField] private float _maxChargeTime = 1.2f;
    [SerializeField] private float _chargeThreshold = 0.2f;
    [SerializeField] private AnimationCurve _rotationEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float _targetRotationOffsetAngle = 5f; // 타격 대상 각도에서 얼마나 덜 회전할지
    [SerializeField] private float _hitProcessPercent = 0.7f; // 타격 처리 타이밍 (0~1, 기본 70%)
    [SerializeField] private float _quickTurnBackAngle = 1.5f; // 빠른 회전 시 뒤로 물러날 각도

    private float _currentChargeTimer = 0f;
    private int _currentChargeLevel = 0;
    private bool _isCharging = false;
    private bool _isAttacking = false;
    private float _lastAttackTime = -100f;
    private float _chargePercent = 0f;
    private float _currentChargePercentForAttack = 0f;
    private int _currentDamageAmount = 1;
    private float _currentKnockbackForce = 3f;

    private float _currentRotationAngle = 0f; // 현재 회전 각도 추적
    private HashSet<Jaein_OrbitalWeapon> _hitBallsThisAttack = new HashSet<Jaein_OrbitalWeapon>(); // 이번 공격에 타격한 공 추적

    private Vector3 _initialWeaponScale;
    private Vector3 _initialWeaponPosition; // Weapon의 초기 로컬 위치
    private Vector3 _initialPivotScale;
    private Color _initialWeaponColor;
    private Quaternion _initialPivotRotation;
    private Rigidbody2D _pivotRigidbody;
    private Transform _pivotParent; // Pivot의 부모 (플레이어 몸통, 마우스 방향으로 회전)
    private Vector2 PlayerPosition => (Vector2)transform.root.position;

    private List<HitTarget> _hitTargetsThisAttack = new List<HitTarget>(); // 이번 공격의 타격 대상 목록 (공, 적)
    private HashSet<fbdfbd_EnemyBase> _hitEnemiesThisAttack = new HashSet<fbdfbd_EnemyBase>(); // 이번 공격에 타격한 적 추적

    private void Awake()
    {
        // 자동 참조
        if (_weaponTransform == null) _weaponTransform = transform;
        if (_weaponSpriteRenderer == null) _weaponSpriteRenderer = GetComponent<SpriteRenderer>();
        if (_endpointTransform == null) _endpointTransform = transform.Find("EndPoint"); // 자식 오브젝트에서 Endpoint 찾기

        _initialWeaponScale = _weaponTransform.localScale;
        if (_weaponSpriteRenderer != null)
            _initialWeaponColor = _weaponSpriteRenderer.color;
        
        _initialWeaponPosition = _weaponTransform.localPosition; // Weapon 초기 로컬 위치 저장

        // Pivot에서 Rigidbody 찾기 및 초기 회전값, 스케일 저장
        Transform pivot = transform.parent;
        if (pivot != null)
        {
            _pivotRigidbody = pivot.GetComponent<Rigidbody2D>();
            _initialPivotRotation = pivot.localRotation;
            _initialPivotScale = pivot.localScale;
            _pivotParent = pivot.parent; // PlayerBody
        }


        SetupLevels();
    }

    private void SetupLevels()
    {
        // 이미 Inspector에서 설정한 경우 건너뛰기
        if (_chargeLevels != null && _chargeLevels.Length == 4 && _chargeLevels[0] != null)
            return;

        if (_chargeLevels == null || _chargeLevels.Length == 0)
            _chargeLevels = new ChargeLevel[4];

        // Level 0: 기본 휘두르기 (차지 없이 바로 공격)
        _chargeLevels[0] = new ChargeLevel
        {
            levelName = "Basic Swing",
            rotationAngle = 180f,
            attackPower = 1f,
            rotationDuration = 0.3f,
            weaponColor = Color.white,
            weaponSizeMultiplier = 1f,
            knockbackForce = 3f,
            damageAmount = 1,
            hitStopDurationMult = 1f
        };

        // Level 1: 세게 휘두르기 (탄환 밀처짐)
        _chargeLevels[1] = new ChargeLevel
        {
            levelName = "Strong Swing",
            rotationAngle = 210f,
            attackPower = 1.5f,
            rotationDuration = 0.2f,
            weaponColor = new Color(1f, 0.8f, 0.5f),
            weaponSizeMultiplier = 1.1f,
            knockbackForce = 5f,
            damageAmount = 2,
            hitStopDurationMult = 1.2f
        };

        // Level 2: 개쎄게 휘두르기 (탄환 지워짐)
        _chargeLevels[2] = new ChargeLevel
        {
            levelName = "Powerful Swing",
            rotationAngle = 250f,
            attackPower = 2f,
            rotationDuration = 0.15f,
            weaponColor = new Color(1f, 0.5f, 0.2f),
            weaponSizeMultiplier = 1.2f,
            knockbackForce = 7f,
            damageAmount = 3,
            hitStopDurationMult = 1.5f
        };

        // Level 3: 존나쌔게 휘두르기 (탄환 반사)
        _chargeLevels[3] = new ChargeLevel
        {
            levelName = "Maximum Swing",
            rotationAngle = 300f,
            attackPower = 3f,
            rotationDuration = 0.1f,
            weaponColor = Color.red,
            weaponSizeMultiplier = 1.3f,
            knockbackForce = 10f,
            damageAmount = 4,
            hitStopDurationMult = 2f
        };
    }

    public void HandleAttackInput(bool pressed, bool held, bool released)
    {
        if (pressed && !_isAttacking && Time.time >= _lastAttackTime + _chargeCooldown)
        {
            _currentChargeTimer = 0f;
            _isCharging = false;
            _currentChargeLevel = 0;
            Debug.Log("[Weapon] 마우스 누름");
        }

        if (held && !_isAttacking)
        {
            float chargeMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.ChargeSpeedMult : 1f;
            _currentChargeTimer += Time.deltaTime * chargeMult;
            if (!_isCharging && _currentChargeTimer > _chargeThreshold)
                _isCharging = true;

            if (_isCharging)
                UpdateChargeVisuals();
        }

        // 마우스를 떼면 차지 상태와 무관하게 공격
        if (released && !_isAttacking)
        {
            StartAttack(_chargePercent); // 차지 퍼센트로 공격 (연속 증가)
            _isCharging = false;
        }
    }

    private void UpdateChargeVisuals()
    {
        float chargeTime = _currentChargeTimer - _chargeThreshold;
        _chargePercent = Mathf.Clamp01(chargeTime / (_maxChargeTime - _chargeThreshold));
        _currentChargeLevel = Mathf.Min(3, Mathf.FloorToInt(_chargePercent * 4f));

        // Pivot 크기 연속 변화
        float dynamicSizeMultiplier = Mathf.Lerp(_chargeLevels[0].weaponSizeMultiplier, _chargeLevels[3].weaponSizeMultiplier, _chargePercent);
        Transform pivot = transform.parent;
        if (pivot != null)
            pivot.localScale = _initialPivotScale * dynamicSizeMultiplier;

        // 색상 변화 (흰색 -> 빨강)
        if (_weaponSpriteRenderer != null)
            _weaponSpriteRenderer.color = Color.Lerp(_chargeLevels[0].weaponColor, _chargeLevels[3].weaponColor, _chargePercent);

        // 회전 변화: 차지에 따라 weaponTransform의 localRotation을 조정 (오른쪽(0도) 기준에서 (angle/2-90)만큼 아래로 이동)
        float minAngle = _chargeLevels[0].rotationAngle;
        float maxAngle = _chargeLevels[3].rotationAngle;
        float dynamicAngle = Mathf.Lerp(minAngle, maxAngle, _chargePercent);
        // 차지 중에는 항상 오른쪽(0도) 기준에서 (angle/2-90)만큼 아래로 이동
        // 스윙 시작 위치(pull-back): 초기 localZ(-90°) = angle=180일 때 그대로, 차지할수록 더 뒤로
        pivot.localRotation = Quaternion.Euler(0f, 0f, -dynamicAngle * 0.5f);
    }

    private void StartAttack(float chargePercent)
    {
        chargePercent = Mathf.Clamp01(chargePercent); // 0~1로 정규화
        StartCoroutine(ExecuteAttack(chargePercent));
    }

    private IEnumerator ExecuteAttack(float chargePercent)
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;
        _currentChargePercentForAttack = chargePercent;

        ChargeLevel minLevel = _chargeLevels[0];
        ChargeLevel maxLevel = _chargeLevels[3];

        float dynamicRotationAngle = Mathf.Lerp(minLevel.rotationAngle, maxLevel.rotationAngle, chargePercent);
        float dynamicRotationDuration = Mathf.Lerp(minLevel.rotationDuration, maxLevel.rotationDuration, chargePercent);
        float batDmgMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.BatDamageMult : 1f;
        float kbMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.KnockbackMult : 1f;
        int dynamicDamageAmount = Mathf.RoundToInt(Mathf.Lerp(minLevel.damageAmount, maxLevel.damageAmount, chargePercent) * batDmgMult);
        float dynamicKnockbackForce = Mathf.Lerp(minLevel.knockbackForce, maxLevel.knockbackForce, chargePercent) * kbMult;

        _currentDamageAmount = dynamicDamageAmount;
        _currentKnockbackForce = dynamicKnockbackForce;

        _hitEnemiesThisAttack.Clear();
        _hitBallsThisAttack.Clear();

        Transform pivot = transform.parent;
        if (pivot == null)
        {
            _isAttacking = false;
            yield break;
        }

        float rangeMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.AttackRangeMult : 1f;
        float hitRadius = Vector2.Distance(pivot.position, _endpointTransform.position) * rangeMult;

        // 스윙 중심은 항상 플레이어 몸통(playerBody) 방향 기준
        float facingZ = _pivotParent != null ? _pivotParent.eulerAngles.z : 0f;
        float centerZ = facingZ + _initialPivotRotation.eulerAngles.z + 90f;
        float startEulerZ = centerZ - dynamicRotationAngle * 0.5f;
        float targetEulerZ = centerZ + dynamicRotationAngle * 0.5f;

        CollectHitTargets(pivot, hitRadius, startEulerZ, targetEulerZ);

        float rotationDuration = dynamicRotationDuration;

        if (_hitTargetsThisAttack.Count > 0)
        {
            // 첫 타격 대상 각도 계산 (1~2도 덜 가기)
            HitTarget firstTarget = _hitTargetsThisAttack[0];
            float firstTargetAbsAngle = ((startEulerZ + firstTarget.angleToHit) % 360f) - _quickTurnBackAngle;

            // 먼저 첫 타격 대상으로 빠르게 회전 (20% 시간)
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

            // 히트스탑 발동 (차지 레벨에 따른 듀레이션 멀티플라이어 적용)
            float hitStopDurationMult = Mathf.Lerp(_chargeLevels[0].hitStopDurationMult, _chargeLevels[3].hitStopDurationMult, _currentChargePercentForAttack);
            WS_HitStopController hitStop = GetComponent<WS_HitStopController>();
            if (hitStop != null)
                hitStop.TryPlayWithChargeAndHitCount(_currentChargePercentForAttack, 1, hitStopDurationMult);

            // 원래 스윙 경로로 계속 회전 (남은 시간)
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

                // 지정된 퍼센트에 도달하면 타격 처리
                if (!hitProcessed && progress >= _hitProcessPercent)
                {
                    // 모든 타격 대상을 한꺼번에 처리
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
        else
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

        ResetToNormal();
        _isAttacking = false;
    }

    private void ResetToNormal()
    {
        // Pivot 스케일 복구
        Transform pivot = transform.parent;
        if (pivot != null)
        {
            pivot.localScale = _initialPivotScale;
            pivot.localRotation = _initialPivotRotation;
        }

        if (_weaponSpriteRenderer != null)
            _weaponSpriteRenderer.color = _initialWeaponColor;

        _currentChargeTimer = 0f;
        _isCharging = false;
    }

    /// <summary>
    /// Pivot의 CollisionHandler에서 호출하는 공용 메서드
    /// </summary>
    public void OnWeaponTriggerEnter2D(Collider2D collision)
    {
        if (!_isAttacking) return;

        // 공(OrbitalWeapon) 타격 처리
        Jaein_OrbitalWeapon orbitalWeapon = collision.GetComponent<Jaein_OrbitalWeapon>();
        if (orbitalWeapon != null)
        {
            if (!_hitBallsThisAttack.Contains(orbitalWeapon))
            {
                orbitalWeapon.TriggerLaunchFromWeapon(_currentChargePercentForAttack);
                orbitalWeapon.PlayWeaponEffect(_weaponTransform, _currentChargePercentForAttack);
                _hitBallsThisAttack.Add(orbitalWeapon);
            }
            return;
        }

        // 차지 단계 계산 (0~3)
        int chargeLevel = Mathf.FloorToInt(_currentChargePercentForAttack * 4f);
        chargeLevel = Mathf.Clamp(chargeLevel, 0, 3);

        // 프로젝타일 처리
        IEnemyProjectile projectile = collision.GetComponent<IEnemyProjectile>();
        if (projectile != null)
        {
            HandleEnemyProjectile(projectile, chargeLevel);
            return;
        }
        // 적 타격 처리
        fbdfbd_EnemyBase enemy = collision.GetComponent<fbdfbd_EnemyBase>();
        if (enemy != null && !_hitEnemiesThisAttack.Contains(enemy))
        {
            ApplyEnemyHit(enemy);
        }
    }

    public void OnWeaponTriggerStay2D(Collider2D collision)
    {
        // Unused callback
    }

    public void OnWeaponTriggerExit2D(Collider2D collision)
    {
        // Unused callback
    }

    /// <summary>
    /// 적 탄환 처리 (차지 단계별)
    /// - Level 0: 아무것도 안함
    /// - Level 1: 방향 유지, 밀처냄 (순간이동)
    /// - Level 2: 제거
    /// - Level 3: 제거
    /// </summary>
    private void HandleEnemyProjectile(IEnemyProjectile projectile, int chargeLevel)
    {
        switch (chargeLevel)
        {
            case 0:
                // 아무것도 안함
                Debug.Log($"[Weapon] 탄환 Level 0: 무시");
                break;

            case 1:
                // 방향 유지, 밀처냄 (순간이동)
                Debug.Log($"[Weapon] 탄환 Level 1: 밀처냄");
                PushEnemyProjectile(projectile);
                break;

            case 2:
                // 제거
                Debug.Log($"[Weapon] 탄환 Level 2: 제거");
                Destroy((projectile as Component).gameObject);
                break;

            case 3:
                // 반사
                Debug.Log($"[Weapon] 탄환 Level 3: 반사");
                ReflectEnemyProjectile(projectile);
                break;
        }
    }

    /// <summary>
    /// 적 탄환 밀처내기 (방향 유지, 순간이동)
    /// </summary>
    private void PushEnemyProjectile(IEnemyProjectile projectile)
    {
        Rigidbody2D rb = projectile.Rb;
        if (rb == null) return;

        // 현재 방향 유지
        Vector2 direction = (PlayerPosition - rb.position).normalized;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;

        // 밀처내기 (현재 속력 유지하며 방향 변경)
        float speed = rb.linearVelocity.magnitude;
        rb.linearVelocity = direction * speed * _currentKnockbackForce;

        Debug.Log($"[Weapon] 탄환 밀처냄: 방향={direction}, 속력={speed * _currentKnockbackForce}");
    }

    /// <summary>
    /// 적 탄환 반사 (방향 반전, 배트 데미지 적용, 색상/이펙트)
    /// </summary>
    private void ReflectEnemyProjectile(IEnemyProjectile projectile)
    {
        // 이펙트 재생
        WS_EffectParticle effect = _weaponTransform.GetComponentInChildren<WS_EffectParticle>();
        if (effect != null) effect.Play(_currentChargePercentForAttack);

        projectile.ReflectAsBatHit(_currentDamageAmount, _enemyReflectLayerMask, Color.white);
    }

    /// <summary>
    /// 적 타격 처리 (데미지, 슬로우, 넉백)
    /// </summary>
    private void ApplyEnemyHit(fbdfbd_EnemyBase enemy)
    {
        Debug.Log($"[Weapon] 적 타격: {enemy.name}, 데미지: {_currentDamageAmount}");
        
        if (_currentDamageAmount > 0)
        {
            enemy.TakeDamage(_currentDamageAmount);
            Debug.Log($"[Weapon] 데미지 적용됨: {enemy.name} <- {_currentDamageAmount}");
        }

        // 슬로우 효과 적용 (차지량에 따라)
        WS_HitStopController hitStop = GetComponent<WS_HitStopController>();
        if (hitStop != null)
            hitStop.TryPlayWithCharge(_currentChargePercentForAttack);
        
        _hitEnemiesThisAttack.Add(enemy);

        // 밀쳐내기
        int chargeLevel = Mathf.Clamp(Mathf.FloorToInt(_currentChargePercentForAttack * 4f), 0, 3);
        if (_chargeLevels[chargeLevel].knockbackEnemies)
        {
            Vector2 knockbackDir = ((Vector2)enemy.transform.position - PlayerPosition).normalized;
            enemy.AddExternalVelocity(knockbackDir * _currentKnockbackForce);
            Debug.Log($"[Weapon] 넉백 적용: {enemy.name}, 강도: {_currentKnockbackForce}");
        }
    }


    /// <summary>
    /// 원형 범위 내 모든 타격 대상(공, 적) 수집 - 회전 범위 내에서만 각도 순서로 정렬
    /// </summary>
    private void CollectHitTargets(Transform pivot, float radius, float startAngle, float endAngle)
    {
        _hitTargetsThisAttack.Clear();

        Collider2D[] hits = Physics2D.OverlapCircleAll(pivot.position, radius);
        Debug.Log($"[Weapon] 범위 내 colliders: {hits.Length}개, 반지름: {radius}, 회전범위: {startAngle}~{endAngle}");

        // 0~360 범위로 정규화
        startAngle = NormalizeAngle(startAngle);
        endAngle = NormalizeAngle(endAngle);

        // 각도를 startAngle 기준 상대값(0~360)으로 변환하는 함수
        float ToRelativeAngle(float angle)
        {
            float rel = angle - startAngle;
            if (rel < 0) rel += 360f;
            return rel;
        }

        HashSet<Jaein_OrbitalWeapon> ballsAdded = new HashSet<Jaein_OrbitalWeapon>();
        HashSet<fbdfbd_EnemyBase> enemiesAdded = new HashSet<fbdfbd_EnemyBase>();

        foreach (Collider2D hit in hits)
        {
            // 공 감지 - 각도 범위 상관없이 반지름 내에 있으면 항상 수집
            Jaein_OrbitalWeapon orbitalWeapon = hit.GetComponent<Jaein_OrbitalWeapon>();
            if (orbitalWeapon != null && orbitalWeapon.State == Jaein_OrbitalWeapon.BallState.Orbit && !ballsAdded.Contains(orbitalWeapon))
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
                Debug.Log($"[Weapon] 공 추가: {orbitalWeapon.name}, 각도: {angle} (rel:{relAngle}), 거리: {distance}");
            }

            // 적 감지
            fbdfbd_EnemyBase enemy = hit.GetComponent<fbdfbd_EnemyBase>();
            if (enemy != null && !enemiesAdded.Contains(enemy))
            {
                if (enemy == null) continue;
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
                    Debug.Log($"[Weapon] 적 추가: {enemy.name}, 각도: {angle} (rel:{relAngle}), 거리: {distance}");
                }
            }
        }

        // 반시계방향(작은 각도→큰 각도), 같은 각도면 가까운 것 우선 정렬
        _hitTargetsThisAttack.Sort((a, b) => {
            int angleComp = a.angleToHit.CompareTo(b.angleToHit);
            if (angleComp != 0) return angleComp;
            return a.distanceToPivot.CompareTo(b.distanceToPivot);
        });
        Debug.Log($"[Weapon] 타격 대상: {_hitTargetsThisAttack.Count}개 (반시계방향, 가까운 것 우선) 정렬 완료");
    }

    /// <summary>
    /// 각도를 0~360 범위로 정규화
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

    /// <summary>
    /// 각 타격 후 현재 각도 ~ 최종 각도 범위에서 추가 타격 대상 검색
    /// </summary>
    private void AddAdditionalHitTargets(Transform pivot, float radius, float currentAngle, float endAngle)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(pivot.position, radius);
        Debug.Log($"[Weapon] 추가 타격 검색: 범위 {currentAngle}~{endAngle}");

        // 0~360 범위로 정규화
        currentAngle = NormalizeAngle(currentAngle);
        endAngle = NormalizeAngle(endAngle);

        // 각도를 currentAngle 기준 상대값(0~360)으로 변환하는 함수
        float ToRelativeAngle(float angle)
        {
            float rel = angle - currentAngle;
            if (rel < 0) rel += 360f;
            return rel;
        }

        int addedCount = 0;
        HashSet<Jaein_OrbitalWeapon> ballsAdded = new HashSet<Jaein_OrbitalWeapon>(_hitTargetsThisAttack.FindAll(t => t.targetType == HitTarget.TargetType.Ball).ConvertAll(t => t.ball));
        HashSet<fbdfbd_EnemyBase> enemiesAdded = new HashSet<fbdfbd_EnemyBase>(_hitTargetsThisAttack.FindAll(t => t.targetType == HitTarget.TargetType.Enemy).ConvertAll(t => t.enemy));

        foreach (Collider2D hit in hits)
        {
            // 공 감지
            Jaein_OrbitalWeapon orbitalWeapon = hit.GetComponent<Jaein_OrbitalWeapon>();
            if (orbitalWeapon != null && orbitalWeapon.State == Jaein_OrbitalWeapon.BallState.Orbit && !ballsAdded.Contains(orbitalWeapon))
            {
                Vector2 dir = ((Vector2)orbitalWeapon.transform.position - (Vector2)pivot.position);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle = NormalizeAngle(angle);
                float relAngle = ToRelativeAngle(angle);
                float distance = dir.magnitude;
                if (IsAngleInRange(angle, currentAngle, endAngle))
                {
                    _hitTargetsThisAttack.Add(new HitTarget
                    {
                        targetType = HitTarget.TargetType.Ball,
                        ball = orbitalWeapon,
                        angleToHit = relAngle,
                        distanceToPivot = distance,
                        targetPosition = orbitalWeapon.transform.position
                    });
                    ballsAdded.Add(orbitalWeapon);
                    addedCount++;
                    Debug.Log($"[Weapon] 추가 공: {orbitalWeapon.name}, 각도: {angle} (rel:{relAngle}), 거리: {distance}");
                }
            }

            // 적 감지
            fbdfbd_EnemyBase enemy = hit.GetComponent<fbdfbd_EnemyBase>();
            if (enemy != null && !enemiesAdded.Contains(enemy))
            {
                if (enemy == null) continue;
                Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)pivot.position);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle = NormalizeAngle(angle);
                float relAngle = ToRelativeAngle(angle);
                float distance = dir.magnitude;
                if (IsAngleInRange(angle, currentAngle, endAngle))
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
                    addedCount++;
                    Debug.Log($"[Weapon] 추가 적: {enemy.name}, 각도: {angle} (rel:{relAngle}), 거리: {distance}");
                }
            }
        }

        // 반시계방향(작은 각도→큰 각도), 같은 각도면 가까운 것 우선 정렬
        if (addedCount > 0)
        {
            _hitTargetsThisAttack.Sort((a, b) => {
                int angleComp = a.angleToHit.CompareTo(b.angleToHit);
                if (angleComp != 0) return angleComp;
                return a.distanceToPivot.CompareTo(b.distanceToPivot);
            });
            Debug.Log($"[Weapon] 추가 타격 {addedCount}개 발견, 총 {_hitTargetsThisAttack.Count}개 (반시계방향, 가까운 것 우선)");
        }
    }

    /// <summary>
    /// 각도가 회전 범위 내에 있는지 확인 (반시계방향)
    /// </summary>
    private bool IsAngleInRange(float angle, float startAngle, float endAngle)
    {
        if (endAngle >= startAngle)
        {
            // 범위가 360을 넘지 않는 경우: startAngle ~ endAngle
            return angle >= startAngle && angle <= endAngle;
        }
        else
        {
            // 범위가 360을 넘는 경우: startAngle ~ 360 + 0 ~ endAngle
            return angle >= startAngle || angle <= endAngle;
        }
    }

    /// <summary>
    /// Weapon을 대상 위치로 이동
    /// </summary>
    private void MoveWeaponToTarget(Vector2 targetWorldPos)
    {
        Transform pivot = transform.parent;
        if (pivot == null) return;

        // Weapon을 Pivot 기준 로컬 좌표로 변환
        Vector2 pivotToTarget = targetWorldPos - (Vector2)pivot.position;
        _weaponTransform.localPosition = new Vector3(pivotToTarget.x, pivotToTarget.y, _initialWeaponPosition.z);
    }

    /// <summary>
    /// Weapon을 초기 위치로 복구
    /// </summary>
    private void ResetWeaponPosition()
    {
        _weaponTransform.localPosition = _initialWeaponPosition;
    }

    /// <summary>
    /// 타격 대상 이미 처리했는지 확인
    /// </summary>
    private bool HasHitTarget(HitTarget target)
    {
        if (target.targetType == HitTarget.TargetType.Ball)
            return _hitBallsThisAttack.Contains(target.ball);
        else
            return _hitEnemiesThisAttack.Contains(target.enemy);
    }

    /// <summary>
    /// 타격 대상 처리 (공 발사 또는 적 데미지)
    /// </summary>
    private void ProcessHitTarget(HitTarget target)
    {
        if (target.targetType == HitTarget.TargetType.Ball)
        {
            // 공이 이미 이번 공격에서 타격되었더라도, 여러 번 타격 가능하게 중복 체크 제거
            Debug.Log($"[Weapon] 공 타격: {target.ball.name}, 차지: {_currentChargePercentForAttack:P0}");
            target.ball.TriggerLaunchFromWeapon(_currentChargePercentForAttack);
            target.ball.PlayWeaponEffect(_weaponTransform, _currentChargePercentForAttack);
            // _hitBallsThisAttack.Add(target.ball); // 중복 방지 제거 (여러 번 타격 허용)
        }
        else
        {
            // 죽은 적(파괴된 오브젝트) 패스
            if (target.enemy == null) return;
            if (_hitEnemiesThisAttack.Contains(target.enemy)) return; // 적은 중복 타격 방지
            Debug.Log($"[Weapon] 적 타격 처리: {target.enemy.name}");
            ApplyEnemyHit(target.enemy);
            _hitEnemiesThisAttack.Add(target.enemy);
        }
    }

    // --- 분리된 타격 애니메이션/처리 루틴 ---
    private IEnumerator RotateAndHitTargets(Transform pivot, float startEulerZ, float targetEulerZ, float rotationDuration, float hitRadius)
    {
        float totalHitTime = rotationDuration * 0.7f;
        float timePerHit = totalHitTime / _hitTargetsThisAttack.Count;
        float currentRotation = startEulerZ;
        for (int i = 0; i < _hitTargetsThisAttack.Count; i++)
        {
            HitTarget target = _hitTargetsThisAttack[i];
            float absTargetAngle = (startEulerZ + target.angleToHit) % 360f;
            float nextRotationTarget = absTargetAngle - _targetRotationOffsetAngle;

            // 부드럽게 회전
            float rotationElapsed = 0f;
            while (rotationElapsed < timePerHit * 0.5f && pivot != null)
            {
                float progress = Mathf.Clamp01(rotationElapsed / (timePerHit * 0.5f));
                float easedProgress = _rotationEasingCurve.Evaluate(progress);
                currentRotation = Mathf.Lerp(currentRotation, nextRotationTarget, easedProgress);
                pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, currentRotation);
                rotationElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, nextRotationTarget);
            currentRotation = nextRotationTarget;
            yield return new WaitForFixedUpdate();
            if (i == 0)
            {
                WS_HitStopController hitStop = GetComponent<WS_HitStopController>();
                if (hitStop != null)
                    hitStop.TryPlayWithChargeAndHitCount(_currentChargePercentForAttack, 1);
            }
            ProcessHitTarget(target);
            yield return new WaitForSeconds(timePerHit * 0.5f);
        }
        if (pivot != null)
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, targetEulerZ);
    }

    private IEnumerator RotateWithoutHit(Transform pivot, float startEulerZ, float targetEulerZ, float rotationDuration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < rotationDuration && pivot != null)
        {
            float progress = Mathf.Clamp01(elapsedTime / rotationDuration);
            float easedProgress = _rotationEasingCurve.Evaluate(progress);
            float currentZ = Mathf.Lerp(startEulerZ, targetEulerZ, easedProgress);
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, currentZ);
            _currentRotationAngle = currentZ;
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        if (pivot != null)
            pivot.eulerAngles = new Vector3(pivot.eulerAngles.x, pivot.eulerAngles.y, targetEulerZ);
    }
    

    public bool IsAttacking => _isAttacking;
    public bool IsCharging => _isCharging;
    public float ChargePercent => _chargePercent;
    public int CurrentChargeLevel => _currentChargeLevel;
}
