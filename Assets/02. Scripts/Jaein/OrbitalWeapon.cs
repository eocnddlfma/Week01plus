using UnityEngine;
using System.Collections.Generic;

public class OrbitalWeapon : MonoBehaviour
{
    public enum BallState
    {
        Orbit,
        Launched,
        Returning
    }

    [Header("Data")]
    [SerializeField] protected OrbitalStatsData _statsData;

    [Header("Center Reference")]
    [SerializeField] protected Transform _center;

    [Header("Orbit Settings")]
    [SerializeField] private float _orbitRadius = 2.0f;
    [SerializeField] private float _orbitAngularSpeed = 180.0f;
    [SerializeField] private float _startAngle = 0.0f;

    [Header("Launch Settings")]
    [SerializeField] private float _launchSpeed = 14.0f;
    [SerializeField] private float _launchSpeedOffset = 2.0f;
    [SerializeField] protected float _launchDuration = 0.35f;
    [SerializeField] private float _launchDurationOffset = 0.05f;
    [SerializeField] private float _randomAngleOffset = 10.0f;

    [Header("Return - Physics")]
    [SerializeField] private float _returnStrength = 16.0f;
    [SerializeField] private float _returnDamping = 1.0f;

    [Header("Return - Escalation")]
    [SerializeField] private float _returnStrengthMax = 150.0f;
    [SerializeField] private float _returnEscalationTime = 3.0f;

    [Header("Return - Orbit Assist")]
    [SerializeField] private float _orbitAssistStrength = 12.0f;
    [SerializeField] private bool _useCounterClockwiseAssist = true;

    [Header("Return - Speed Clamp")]
    [SerializeField] private float _maxReturnSpeed = 18.0f;

    [Header("Rejoin Settings")]
    [SerializeField] private float _rejoinDistanceToOrbit = 0.2f;
    [SerializeField] private float _rejoinVelocityLimit = 8.0f;
    [SerializeField] private float _rejoinBrakeDamping = 8.0f;

    [Header("Snap To Orbit")]
    [SerializeField] private float _snapDuration = 0.3f;

    [Header("Damage Settings")]
    [SerializeField] private int _baseDamage = 2;
    [SerializeField] private int _maxChargeDamage = 20;
    [SerializeField] private float _varianceRange = 0.1f;
    [SerializeField] private float _knockbackForce = 4f;

    [Header("Charge Orbit Effect")]
    [SerializeField] private float _chargeBoostRampDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _drawOrbitGizmo = true;
    [SerializeField] private float _stateGizmoRadius = 0.5f;

    private static readonly List<OrbitalWeapon> _allOrbitals = new List<OrbitalWeapon>();

    private PlayerController _playerController;
    private BatWeaponManager _weaponManager;

    protected BallState _state = BallState.Orbit;
    protected BallState State => _state;
    private float _angleDeg;
    protected float _stateTimer;
    protected float _returnTimeElapsed;
    protected Vector2 _velocity;
    private float _snapTimer;
    private float _snapStartRadius;
    private float _chargeBoostRampTimer;
    private BallState _prevState = BallState.Orbit;

    // 발사 시 저장된 차지 퍼센트 (Orbit 복귀 시 초기화)
    private float _chargePercent = 0f;
    private float _attackPower = 1f;

    protected virtual void Awake()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        _center = _playerController.transform;
        if (_playerController == null) print("따라갈 대상 못찾음");

        _weaponManager = FindAnyObjectByType<BatWeaponManager>();
        _allOrbitals.Add(this);

        InitFromStatsData();
    }

    protected virtual void OnDestroy()
    {
        _allOrbitals.Remove(this);
    }

    private void InitFromStatsData()
    {
        if (_statsData == null) return;
        _orbitRadius = _statsData.orbitRadius;
        _orbitAngularSpeed = _statsData.orbitAngularSpeed;
        _launchSpeed = _statsData.launchSpeed;
        _launchSpeedOffset = _statsData.launchSpeedOffset;
        _launchDuration = _statsData.launchDuration;
        _launchDurationOffset = _statsData.launchDurationOffset;
        _randomAngleOffset = _statsData.randomAngleOffset;
        _returnStrength = _statsData.returnStrength;
        _returnDamping = _statsData.returnDamping;
        _returnStrengthMax = _statsData.returnStrengthMax;
        _returnEscalationTime = _statsData.returnEscalationTime;
        _orbitAssistStrength = _statsData.orbitAssistStrength;
        _useCounterClockwiseAssist = _statsData.useCounterClockwiseAssist;
        _maxReturnSpeed = _statsData.maxReturnSpeed;
        _rejoinDistanceToOrbit = _statsData.rejoinDistanceToOrbit;
        _rejoinVelocityLimit = _statsData.rejoinVelocityLimit;
        _rejoinBrakeDamping = _statsData.rejoinBrakeDamping;
        _snapDuration = _statsData.snapDuration;
        _baseDamage = _statsData.baseDamage;
        _maxChargeDamage = _statsData.maxChargeDamage;
        _varianceRange = _statsData.varianceRange;
        _knockbackForce = _statsData.knockbackForce;
    }

    protected virtual void Start()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        _center = _playerController.transform;
        if (_playerController == null) print("따라갈 대상 못찾음");
        _angleDeg = _startAngle;
        Vector2 radialDir = GetRadialDirection(_angleDeg);
        transform.position = (Vector2)_center.position + radialDir * _orbitRadius;
        _velocity = Vector2.zero;
    }

    private void Update()
    {
        if (_center == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0.0f) return;
        UpdateState(dt);
    }

    private void UpdateState(float dt)
    {
        if (_state != _prevState)
        {
            //Debug.Log($"[OrbitBall] State: {_prevState} �� {_state}");
            _prevState = _state;
        }

        switch (_state)
        {
            case BallState.Orbit:
                UpdateOrbitState(dt);
                break;

            case BallState.Launched:
                UpdateLaunchedState(dt);
                break;

            case BallState.Returning:
                UpdateReturningState(dt);
                break;
        }
    }

    private void UpdateOrbitState(float dt)
    {
        float chargePercent = (_weaponManager != null && _weaponManager.IsCharging)
            ? _weaponManager.ChargePercent : 0f;

        _chargeBoostRampTimer = Mathf.Min(_chargeBoostRampTimer + dt, _chargeBoostRampDuration);
        float ramp = _chargeBoostRampDuration > 0f
            ? _chargeBoostRampTimer / _chargeBoostRampDuration
            : 1f;

        float speedBoost = _weaponManager != null ? _weaponManager.OrbitalChargeSpeedBoostMax : 2f;
        _angleDeg += _orbitAngularSpeed * (1f + chargePercent * speedBoost * ramp) * dt;

        if (chargePercent > 0f && _allOrbitals.Count > 0 && _allOrbitals[0] != this && _allOrbitals[0]._state == BallState.Orbit)
        {
            float clusterStrength = _weaponManager != null ? _weaponManager.OrbitalChargeClusterStrength : 2f;
            float anchorAngle = _allOrbitals[0]._angleDeg;
            float diff = Mathf.DeltaAngle(_angleDeg, anchorAngle);
            _angleDeg += diff * chargePercent * clusterStrength * dt;
        }

        NormalizeAngle();

        float radius = _orbitRadius;
        if (_snapTimer > 0.0f)
        {
            _snapTimer -= dt;
            float t = Mathf.SmoothStep(0.0f, 1.0f, 1.0f - Mathf.Max(_snapTimer, 0.0f) / _snapDuration);
            radius = Mathf.Lerp(_snapStartRadius, _orbitRadius, t);
        }

        Vector2 radialDir = GetRadialDirection(_angleDeg);
        transform.position = (Vector2)_center.position + radialDir * radius;
    }

    private void UpdateLaunchedState(float dt)
    {
        _stateTimer -= dt;
        transform.position = (Vector2)transform.position + _velocity * dt;

        if (_stateTimer <= 0.0f)
        {
            _returnTimeElapsed = 0.0f;
            _state = BallState.Returning;
        }
    }

    private void UpdateReturningState(float dt)
    {
        Vector2 pos = transform.position;
        if (StepReturnPhysics(ref pos, ref _velocity, ref _returnTimeElapsed, _angleDeg, dt))
            RejoinOrbit(pos);
        transform.position = pos;
    }

    private bool StepReturnPhysics(ref Vector2 pos, ref Vector2 vel, ref float returnTimeElapsed, float fallbackAngleDeg, float dt)
    {
        Vector2 fromCenter = pos - (Vector2)_center.position;
        float distanceToCenter = fromCenter.magnitude;

        if (distanceToCenter <= 0.0001f)
        {
            fromCenter = GetRadialDirection(fallbackAngleDeg) * 0.001f;
            distanceToCenter = fromCenter.magnitude;
        }

        Vector2 radialDir = fromCenter / distanceToCenter;
        Vector2 toCenterDir = -radialDir;
        Vector2 tangentDir = _useCounterClockwiseAssist
            ? new Vector2(-radialDir.y, radialDir.x)
            : new Vector2(radialDir.y, -radialDir.x);

        returnTimeElapsed += dt;
        float distanceToOrbit = Mathf.Abs(distanceToCenter - _orbitRadius);

        float escalationT = Mathf.Clamp01(returnTimeElapsed / _returnEscalationTime);
        float baseEscalation = Mathf.Lerp(0.0f, _returnStrengthMax - _returnStrength, escalationT);
        float escalationStrength = distanceToOrbit > 1.5f ? baseEscalation : baseEscalation * 0.1f;

        Vector2 pullAcceleration = toCenterDir * (_returnStrength + escalationStrength);
        Vector2 orbitAssistAcceleration = tangentDir * _orbitAssistStrength;
        float brakeDamping = distanceToOrbit <= _rejoinDistanceToOrbit ? _rejoinBrakeDamping : 0.0f;
        Vector2 dampingAcceleration = -vel * (_returnDamping + brakeDamping);

        vel += (pullAcceleration + orbitAssistAcceleration + dampingAcceleration) * dt;

        if (vel.magnitude > _maxReturnSpeed)
            vel = vel.normalized * _maxReturnSpeed;

        pos += vel * dt;

        return distanceToOrbit <= _rejoinDistanceToOrbit && vel.magnitude <= _rejoinVelocityLimit * 1.5f;
    }

    public void Launch(float chargePercent= 0.0f)
    {
        if (_center == null) return;
        
        if (chargePercent >= 0.999f)
        {
            var controller = GetComponent<HitStopController>();
            if (controller != null)
                controller.TryPlay();
        }

        Vector2 facingDir = _playerController != null
            ? _playerController.FacingDirection
            : GetRadialDirection(_angleDeg);

        float randomOffset = Random.Range(-_randomAngleOffset, _randomAngleOffset);
        Vector2 launchDir = RotateVector(facingDir, randomOffset).normalized;

        float powerMultiplier = 1.0f + (chargePercent * 0.5f);
        float ballSpeedMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.BallSpeedMult : 1f;
        _velocity = launchDir * ((_launchSpeed * powerMultiplier + Random.Range(-_launchSpeedOffset, _launchSpeedOffset)) * ballSpeedMult);

        float durationMultiplier = 1.0f + (chargePercent * 0.5f);
        _stateTimer = _launchDuration * durationMultiplier + Random.Range(-_launchDurationOffset, _launchDurationOffset);

        _returnTimeElapsed = 0.0f;
        
        _state = BallState.Launched;

        //Debug.Log($"[Orbital] Charge: {chargePercent * 100}%, Speed: {_velocity.magnitude}");
    }

    protected virtual void RejoinOrbit(Vector2 currentPos)
    {
        Vector2 fromCenter = currentPos - (Vector2)_center.position;

        if (fromCenter.sqrMagnitude <= 0.0001f)
        {
            fromCenter = GetRadialDirection(_angleDeg) * _orbitRadius;
        }

        Vector2 radialDir = fromCenter.normalized;
        _angleDeg = Mathf.Atan2(radialDir.y, radialDir.x) * Mathf.Rad2Deg;
        NormalizeAngle();

        _snapStartRadius = fromCenter.magnitude;
        _snapTimer = _snapDuration;
        _velocity = Vector2.zero;
        _chargeBoostRampTimer = 0f;
        _state = BallState.Orbit;

        // Orbit 복귀 시 차지 퍼센트 초기화
        _chargePercent = 0f;
    }

    public void AddVelocity(Vector2 delta)
    {
        if (_state == BallState.Orbit) return;
        _velocity += delta;
    }

    public void ReflectVelocity(Vector2 normal)
    {
        if (_state == BallState.Orbit) return;
        _velocity  = Vector2.Reflect(_velocity, normal.normalized);
        _state     = BallState.Launched;
        _stateTimer = _launchDuration;
    }

    public Vector2[] SimulateTrajectory(int maxSteps, float simDt)
    {
        if (_center == null) return System.Array.Empty<Vector2>();

        var positions = new System.Collections.Generic.List<Vector2>(maxSteps + 1);

        Vector2 simPos = transform.position;
        Vector2 simVel;
        float simStateTimer;
        float simReturnTimeElapsed;
        float simAngleDeg = _angleDeg;
        bool isLaunched;

        switch (_state)
        {
            case BallState.Orbit:
                Vector2 facingDir = _playerController != null
                    ? _playerController.FacingDirection
                    : GetRadialDirection(_angleDeg);
                simVel = facingDir * _launchSpeed;
                simStateTimer = _launchDuration;
                simReturnTimeElapsed = 0f;
                isLaunched = true;
                break;
            case BallState.Launched:
                simVel = _velocity;
                simStateTimer = _stateTimer;
                simReturnTimeElapsed = _returnTimeElapsed;
                isLaunched = true;
                break;
            case BallState.Returning:
                simVel = _velocity;
                simStateTimer = 0f;
                simReturnTimeElapsed = _returnTimeElapsed;
                isLaunched = false;
                break;
            default:
                return System.Array.Empty<Vector2>();
        }

        positions.Add(simPos);

        for (int i = 0; i < maxSteps; i++)
        {
            if (isLaunched)
            {
                simStateTimer -= simDt;
                simPos += simVel * simDt;
                positions.Add(simPos);

                if (simStateTimer <= 0f)
                {
                    simReturnTimeElapsed = 0f;
                    isLaunched = false;
                }
            }
            else
            {
                bool rejoined = StepReturnPhysics(ref simPos, ref simVel, ref simReturnTimeElapsed, simAngleDeg, simDt);
                positions.Add(simPos);
                if (rejoined) break;
            }
        }

        return positions.ToArray();
    }

    private Vector2 GetRadialDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private Vector2 RotateVector(Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    private void NormalizeAngle()
    {
        while (_angleDeg >= 360.0f) _angleDeg -= 360.0f;
        while (_angleDeg < 0.0f) _angleDeg += 360.0f;
    }


    /// <summary>
    /// 무기 충돌로부터 발사 요청 (BatWeaponManager에서 호출)
    /// </summary>
    public void TriggerLaunchFromWeapon(float chargePercent, float attackPower = 1f)
    {
        _chargePercent = chargePercent;
        _attackPower = attackPower;
        Launch(_chargePercent);
    }

    /// <summary>
    /// 무기 이펙트 재생 (BatWeaponManager에서 호출)
    /// </summary>
    public void PlayWeaponEffect(Transform weaponTransform, float chargePercent)
    {
        EffectParticle effect = weaponTransform.GetComponentInChildren<EffectParticle>();
        if (effect != null)
            effect.Play(chargePercent);
    }

    /// <summary>
    /// 그래비티 적용 여부 반환 (Orbit 상태가 아닐 때 적용)
    /// </summary>
    public bool ShouldApplyGravity()
    {
        return _state != BallState.Orbit;
    }

    /// <summary>
    /// 궤적 색상 타입 반환 (Returning 또는 Launch)
    /// </summary>
    public bool IsReturning()
    {
        return _state == BallState.Returning;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // 적과 충돌 시 저장된 차지 퍼센트로 데미지 계산
        var enemy = other.gameObject.GetComponent<EnemyBase>();

        if (enemy != null)
        {
            if (_state == BallState.Orbit) return;

            int finalDamage = CalculateDamage(_chargePercent, _attackPower);
            bool isFullCharge = _chargePercent >= 0.999f;
            _chargePercent = 0f;
            _attackPower = 1f;

            enemy.TakeDamage(finalDamage, isFullCharge);
            ApplyKnockback(enemy);

            return;
        }
    }

    private void ApplyKnockback(EnemyBase enemy)
    {
        if (_knockbackForce <= 0f || _center == null) return;

        Vector2 ballDir = _velocity.normalized;
        if (ballDir.sqrMagnitude < 0.001f) return;

        Vector2 toPlayer = ((Vector2)_center.position - (Vector2)transform.position).normalized;
        float dot = Vector2.Dot(ballDir, toPlayer);

        Vector2 knockbackDir;
        if (dot > 0f)
        {
            // 플레이어 쪽으로 향하는 중 → 수직 방향으로 보정
            Vector2 sideways = ballDir - dot * toPlayer;
            if (sideways.sqrMagnitude > 0.001f)
                knockbackDir = Vector2.Lerp(ballDir, sideways.normalized, dot).normalized;
            else
                knockbackDir = Vector2.Perpendicular(toPlayer).normalized;
        }
        else
        {
            // 플레이어 반대 방향으로 향하는 중 → 진행 방향 그대로
            knockbackDir = ballDir;
        }

        float kbMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.KnockbackMult : 1f;
        float force = _velocity.magnitude * _knockbackForce * kbMult;

        float t = Mathf.Clamp01(force / 15f);
        float duration = (1f - (1f - t) * (1f - t) * (1f - t)) * 0.25f;

        enemy.AddExternalVelocity(knockbackDir * force, duration);
    }

    private int CalculateDamage(float chargePercent, float attackPower = 1f)
    {
        float ballDmgMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.BallDamageMult : 1f;
        float rawDamage = Mathf.Lerp(_baseDamage, _maxChargeDamage, chargePercent) * attackPower * ballDmgMult;
        float variance = rawDamage * _varianceRange;

        float finalDamage = Random.Range(rawDamage - variance, rawDamage + variance);

        return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_drawOrbitGizmo) return;

        // 궤도 그리기
        if (_center != null)
        {
            Gizmos.color = Color.red;
            const int segmentCount = 96;
            Vector3 prev = _center.position + Vector3.right * _orbitRadius;

            for (int i = 1; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                float angle = t * Mathf.PI * 2.0f;
                Vector3 next = _center.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f) * _orbitRadius;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        // 상태에 따라 기즈모 색상 변경 및 원 그리기
        switch (_state)
        {
            case BallState.Orbit:
                Gizmos.color = Color.red; // 궤도 상태: 빨간색
                break;
            case BallState.Launched:
                Gizmos.color = Color.yellow; // 발사 상태: 노란색
                break;
            case BallState.Returning:
                Gizmos.color = Color.green; // 복귀 상태: 녹색
                break;
        }

        // 공 외부에 원을 그립니다.
        Gizmos.DrawWireSphere(transform.position, _stateGizmoRadius);
    }
#endif
}