using System.Collections;
using UnityEngine;

public class EnemyCharger : EnemyBase
{
    private enum MoveState
    {
        Approach,
        Windup,
        Charge,
        Recover
    }

    [Header("Charger Speeds")]
    [Min(0.1f)][SerializeField] private float _approachSpeedMultiplier = 0.55f;
    [Min(0.1f)][SerializeField] private float _recoverSpeedMultiplier = 0.35f;
    [Min(0.1f)][SerializeField] private float _chargeSpeed = 12f;

    [Header("Charger Distances")]
    [Min(0f)][SerializeField] private float _chargeStartDistance = 4f;

    [Header("Charger Timings")]
    [Min(0f)][SerializeField] private float _windupDuration = 0.25f;
    [Min(0.05f)][SerializeField] private float _chargeDuration = 0.35f;
    [Min(0f)][SerializeField] private float _recoverDuration = 0.3f;
    [Min(0f)][SerializeField] private float _chargeCooldownMin = 1.2f;
    [Min(0f)][SerializeField] private float _chargeCooldownMax = 2f;

    [Header("Wave Scaling (Charger)")]
    [Min(0f)][SerializeField] private float _chargeSpeedScalePerWave = 0.08f;
    [Min(0f)][SerializeField] private float _chargeStartDistanceScalePerWave = 0.06f;
    [Min(0f)][SerializeField] private float _chargeDurationScalePerWave = 0.05f;
    [Min(0f)][SerializeField] private float _windupReducePerWave = 0.05f; // 와인드업 시간 감소 (피하기 어려워짐)
    [Min(0f)][SerializeField] private float _cooldownReducePerWave = 0.05f; // 쿨타임 감소율

    private float _baseChargeSpeed;
    private float _baseChargeStartDistance;
    private float _baseChargeDuration;
    private float _baseWindupDuration;
    private float _baseCooldownMin;
    private float _baseCooldownMax;

    [Header("Windup Visual")]
    [SerializeField] private SpriteRenderer[] _windupRenderers;
    [SerializeField] private EnemyHitFlash _hitFlash;
    [SerializeField] private Color _windupColor = Color.yellow;
    [SerializeField, Range(0f, 1f)] private float _windupBlend = 1f;
    [SerializeField] private AnimationCurve _windupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Min(0f)] private float _windupReturnDuration = 0.1f;

    private MoveState _state = MoveState.Approach;
    private float _stateEndTime;
    private float _nextChargeTime;
    private Vector2 _chargeDir = Vector2.down;

    private Color[] _windupBaseColors;
    private Coroutine _windupRestoreRoutine;
    private float _windupStartTime;

    protected override bool CanAttack(float distanceToTarget) => false;
    protected override void DoAttack() { }

    protected override void Awake()
    {
        base.Awake();
        if (_hitFlash == null) _hitFlash = GetComponent<EnemyHitFlash>();
        _baseChargeSpeed = _chargeSpeed;
        _baseChargeStartDistance = _chargeStartDistance;
        _baseChargeDuration = _chargeDuration;
        _baseWindupDuration = _windupDuration;
        _baseCooldownMin = _chargeCooldownMin;
        _baseCooldownMax = _chargeCooldownMax;
        InitWindupVisual();
        ScheduleNextCharge();
    }

    protected override void ApplyWaveScaling(int waveIndex)
    {
        base.ApplyWaveScaling(waveIndex);
        _chargeSpeed = _baseChargeSpeed * (1f + waveIndex * _chargeSpeedScalePerWave);
        _chargeStartDistance = _baseChargeStartDistance * (1f + waveIndex * _chargeStartDistanceScalePerWave);
        _chargeDuration = _baseChargeDuration * (1f + waveIndex * _chargeDurationScalePerWave);
        _windupDuration = _baseWindupDuration / (1f + waveIndex * _windupReducePerWave);
        float cooldownMult = 1f / (1f + waveIndex * _cooldownReducePerWave);
        _chargeCooldownMin = _baseCooldownMin * cooldownMult;
        _chargeCooldownMax = _baseCooldownMax * cooldownMult;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_windupRestoreRoutine != null)
        {
            StopCoroutine(_windupRestoreRoutine);
            _windupRestoreRoutine = null;
        }

        RestoreWindupVisualImmediate();
    }

    protected override void FixedUpdate()
    {
        if (_isDead)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        if (Target == null)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsKnockedBack)
        {
            base.FixedUpdate();
            return;
        }

        switch (_state)
        {
            case MoveState.Approach:
                UpdateApproach();
                break;

            case MoveState.Windup:
                UpdateWindup();
                break;

            case MoveState.Charge:
                UpdateCharge();
                break;

            case MoveState.Recover:
                UpdateRecover();
                break;
        }
    }

    private void UpdateApproach()
    {
        base.FixedUpdate();
        Rb.linearVelocity *= Mathf.Clamp01(_approachSpeedMultiplier);

        if (Time.time < _nextChargeTime)
            return;

        float distanceToTarget = Vector2.Distance(Rb.position, Target.position);
        if (distanceToTarget <= _chargeStartDistance)
        {
            EnterWindup();
        }
    }

    private void UpdateWindup()
    {
        Rb.linearVelocity = Vector2.zero;
        UpdateWindupVisual();

        if (Time.time >= _stateEndTime)
        {
            EnterCharge();
        }
    }

    private void UpdateCharge()
    {
        Rb.linearVelocity = _chargeDir * _chargeSpeed;

        if (Time.time >= _stateEndTime)
        {
            EnterRecover();
        }
    }

    private void UpdateRecover()
    {
        base.FixedUpdate();
        Rb.linearVelocity *= Mathf.Clamp01(_recoverSpeedMultiplier);

        if (Time.time >= _stateEndTime)
        {
            _state = MoveState.Approach;
        }
    }

    private void EnterWindup()
    {
        if (_windupRestoreRoutine != null)
        {
            StopCoroutine(_windupRestoreRoutine);
            _windupRestoreRoutine = null;
        }

        _state = MoveState.Windup;
        _stateEndTime = Time.time + _windupDuration;
        _windupStartTime = Time.time;
        Rb.linearVelocity = Vector2.zero;
    }

    private void EnterCharge()
    {
        Vector2 toTarget = (Vector2)Target.position - Rb.position;
        if (toTarget.sqrMagnitude > 0.0001f)
            _chargeDir = toTarget.normalized;
        else
            _chargeDir = LastDir.sqrMagnitude > 0.0001f ? LastDir.normalized : Vector2.down;

        _state = MoveState.Charge;
        _stateEndTime = Time.time + _chargeDuration;
        BeginRestoreWindupVisual();
    }

    private void EnterRecover()
    {
        _state = MoveState.Recover;
        _stateEndTime = Time.time + _recoverDuration;
        ScheduleNextCharge();
    }

    private void ScheduleNextCharge()
    {
        float min = Mathf.Max(0f, _chargeCooldownMin);
        float max = Mathf.Max(min, _chargeCooldownMax);
        _nextChargeTime = Time.time + Random.Range(min, max);
    }

    private void InitWindupVisual()
    {
        if (_windupRenderers == null || _windupRenderers.Length == 0)
            _windupRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        _windupBaseColors = new Color[_windupRenderers.Length];
        for (int i = 0; i < _windupRenderers.Length; i++)
            _windupBaseColors[i] = _windupRenderers[i].color;

        RestoreWindupVisualImmediate();
    }

    private void UpdateWindupVisual()
    {
        if (_hitFlash != null && _hitFlash.IsFlashing)
            return;

        if (_windupRenderers == null || _windupRenderers.Length == 0)
            return;

        float t = _windupDuration <= 0f
            ? 1f
            : Mathf.Clamp01((Time.time - _windupStartTime) / _windupDuration);

        float curveValue = _windupCurve != null ? _windupCurve.Evaluate(t) : t;
        float weight = Mathf.Clamp01(curveValue) * Mathf.Clamp01(_windupBlend);

        for (int i = 0; i < _windupRenderers.Length; i++)
            _windupRenderers[i].color = Color.Lerp(_windupBaseColors[i], _windupColor, weight);
    }

    private void BeginRestoreWindupVisual()
    {
        if (_windupRestoreRoutine != null)
            StopCoroutine(_windupRestoreRoutine);

        if (_windupReturnDuration <= 0f)
        {
            RestoreWindupVisualImmediate();
            _windupRestoreRoutine = null;
            return;
        }

        _windupRestoreRoutine = StartCoroutine(RestoreWindupVisualRoutine());
    }

    private IEnumerator RestoreWindupVisualRoutine()
    {
        int count = _windupRenderers != null ? _windupRenderers.Length : 0;
        if (count == 0)
        {
            _windupRestoreRoutine = null;
            yield break;
        }

        Color[] startColors = new Color[count];
        for (int i = 0; i < count; i++)
            startColors[i] = _windupRenderers[i].color;

        float elapsed = 0f;
        while (elapsed < _windupReturnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _windupReturnDuration);

            for (int i = 0; i < count; i++)
                _windupRenderers[i].color = Color.Lerp(startColors[i], _windupBaseColors[i], t);

            yield return null;
        }

        RestoreWindupVisualImmediate();
        _windupRestoreRoutine = null;
    }

    private void RestoreWindupVisualImmediate()
    {
        if (_windupRenderers == null || _windupBaseColors == null)
            return;

        int count = Mathf.Min(_windupRenderers.Length, _windupBaseColors.Length);
        for (int i = 0; i < count; i++)
            _windupRenderers[i].color = _windupBaseColors[i];
    }
}
