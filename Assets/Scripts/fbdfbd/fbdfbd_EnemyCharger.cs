using UnityEngine;

public class fbdfbd_EnemyCharger : fbdfbd_EnemyBase
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

    private MoveState _state = MoveState.Approach;
    private float _stateEndTime;
    private float _nextChargeTime;
    private Vector2 _chargeDir = Vector2.down;

    protected override bool CanAttack(float distanceToTarget) => false;
    protected override void DoAttack() { }

    protected override void Awake()
    {
        base.Awake();
        ScheduleNextCharge();
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
        _state = MoveState.Windup;
        _stateEndTime = Time.time + _windupDuration;
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
}
