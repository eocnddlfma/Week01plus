using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : EntityBase
{
    [Header("Data")]
    [SerializeField] protected EnemyStatsData _statsData;

    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Movement")]
    [Min(0f)][SerializeField] private float _moveSpeed = 2f;
    [Min(0f)][SerializeField] private float _stopDistance = 1.2f;
    [Min(0f)][SerializeField] private float _moveLerpSpeed = 8f;
    private float _distanceTolerance;

    [Header("Group Movement")]
    [SerializeField] private LayerMask _enemyLayerMask;
    [Min(0f)][SerializeField] private float _separationRadius = 0.7f;
    [Min(0f)][SerializeField] private float _separationWeight = 1.0f;
    [Min(0f)][SerializeField] private float _targetOffsetRadius = 0.5f;
    [Min(0f)][SerializeField] private float _noiseWeight = 0.08f;
    [Min(0f)][SerializeField] private float _noiseSpeed = 1.2f;

    [Header("Attack Timing")]
    [Min(0f)][SerializeField] private float _attackIntervalMin = 1.0f;
    [Min(0f)][SerializeField] private float _attackIntervalMax = 2.0f;

    private float _nextAttackTime;
    private Vector2 _currentVelocity;
    private Vector2 _personalOffset;
    private float _noiseSeed;
    private bool _isEnemyCountRegistered;

    public Transform Target => _target;
    public event Action<int> OnDamaged;

    private Vector2 _externalVelocity;
    public void AddExternalVelocity(Vector2 vel) => _externalVelocity += vel;

    // Range Enemy에서 사용
    protected Vector2 LastDir { get; private set; } = Vector2.down;
    protected virtual bool ShouldTrackEnemyCount => true;

    protected void InitDistanceTolerance()
    {
        _distanceTolerance = Random.Range(0.1f, 0.7f);
    }
    protected virtual bool CanAttack(float distanceToTarget) => false;
    protected abstract void DoAttack();

    protected override void Awake()
    {
        base.Awake();

        Rb.freezeRotation = true;

        InitFromStatsData();

        _noiseSeed = Random.Range(0f, 1000f);
        _personalOffset = Random.insideUnitCircle * _targetOffsetRadius;

        ScheduleNextAttack();

        if (ShouldTrackEnemyCount && GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemy();
            _isEnemyCountRegistered = true;
        }
    }

    private void InitFromStatsData()
    {
        if (_statsData == null) return;
        _moveSpeed = _statsData.moveSpeed;
        _stopDistance = _statsData.stopDistance;
        // hp는 EntityBase에서 설정하므로 생략 (필요시 추가)
    }

    protected virtual void Update()
    {
        if (_isDead)
            return;

        if (_target == null)
        {
            Debug.Log("target이 없습니다.");
            return;
        }

        Vector2 toTarget = (Vector2)_target.position - Rb.position;
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            LastDir = toTarget.normalized;
        }

        float dist = toTarget.magnitude;
        if (CanAttack(dist) && Time.time >= _nextAttackTime)
        {
            DoAttack();
            ScheduleNextAttack();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (_isDead)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        if (_target == null)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 realTargetPos = (Vector2)_target.position;
        Vector2 toRealTarget = realTargetPos - Rb.position;
        float distToRealTarget = toRealTarget.magnitude;

        Vector2 desiredMove = Vector2.zero;

        if (distToRealTarget > _stopDistance + _distanceTolerance)
        {
            desiredMove = CalculateMoveDirection();
        }
        else if (distToRealTarget < _stopDistance - _distanceTolerance)
        {
            desiredMove = -toRealTarget.normalized;
        }

        Vector2 targetVelocity = desiredMove * _moveSpeed;
        _currentVelocity = Vector2.Lerp(
            _currentVelocity,
            targetVelocity,
            _moveLerpSpeed * Time.fixedDeltaTime);

        Rb.linearVelocity = _currentVelocity + _externalVelocity;
        _externalVelocity = Vector2.zero;
    }

    private Vector2 CalculateMoveDirection()
    {
        Vector2 dynamicTargetPos = GetDynamicTargetPosition();
        Vector2 toTarget = dynamicTargetPos - Rb.position;

        Vector2 moveDir = Vector2.zero;

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            moveDir += toTarget.normalized;
        }

        moveDir += CalculateSeparation();
        moveDir += CalculateNoise();

        if (moveDir.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        return moveDir.normalized;
    }

    private Vector2 GetDynamicTargetPosition()
    {
        if (_target == null)
            return Rb.position;

        float t = Time.time * 0.75f + _noiseSeed;

        Vector2 driftingOffset = new Vector2(
            Mathf.PerlinNoise(_noiseSeed, t) - 0.5f,
            Mathf.PerlinNoise(t, _noiseSeed) - 0.5f
        ) * 2f * _targetOffsetRadius;

        return (Vector2)_target.position + (_personalOffset * 0.5f) + (driftingOffset * 0.5f);
    }

    private Vector2 CalculateSeparation()
    {
        if (_separationRadius <= 0f || _separationWeight <= 0f)
            return Vector2.zero;

        Collider2D[] hits = Physics2D.OverlapCircleAll(Rb.position, _separationRadius, _enemyLayerMask);

        if (hits == null || hits.Length == 0)
            return Vector2.zero;

        Vector2 push = Vector2.zero;
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null || hit.attachedRigidbody == null)
                continue;

            if (hit.attachedRigidbody == Rb)
                continue;

            Vector2 diff = Rb.position - hit.attachedRigidbody.position;
            float dist = diff.magnitude;

            if (dist <= 0.0001f || dist > _separationRadius)
                continue;

            float weight = 1f - (dist / _separationRadius);
            push += diff.normalized * weight;
            count++;
        }

        if (count <= 0)
            return Vector2.zero;

        push /= count;
        return push * _separationWeight;
    }

    private Vector2 CalculateNoise()
    {
        if (_noiseWeight <= 0f)
            return Vector2.zero;

        float t = Time.time * _noiseSpeed;

        float nx = Mathf.PerlinNoise(_noiseSeed, t) - 0.5f;
        float ny = Mathf.PerlinNoise(t, _noiseSeed) - 0.5f;

        return new Vector2(nx, ny) * _noiseWeight;
    }

    protected override void OnDeath()
    {
        Rb.linearVelocity = Vector2.zero;

        if (_isEnemyCountRegistered && GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterEnemy();
            _isEnemyCountRegistered = false;
        }

        GameManager.Instance.AddScore(100);
        ReturnToPool();
    }

    // Phase 7: 에너미 풀링
    protected virtual void OnDisable()
    {
        ResetState();
    }

    protected virtual void ResetState()
    {
        // 기본 상태 리셋
        _isDead = false;
        _hp = 1;
        _currentVelocity = Vector2.zero;
        _externalVelocity = Vector2.zero;
        Rb.linearVelocity = Vector2.zero;
        _target = null;
        _isEnemyCountRegistered = false;

        // 트랜스폼 리셋
        if (Rb != null)
            Rb.position = Vector2.zero;
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
    }

    protected void ScheduleNextAttack()
    {
        float min = Mathf.Max(0f, _attackIntervalMin);
        float max = Mathf.Max(min, _attackIntervalMax);
        _nextAttackTime = Time.time + Random.Range(min, max);
    }

    protected bool CanAttackToTarget()
    {
        if (_isDead || _target == null)
            return false;

        float distanceToTarget = Vector2.Distance(Rb.position, _target.position);
        return CanAttack(distanceToTarget);
    }

    public override void TakeDamage(int damage, bool isCharge = false)
    {
        if (_isDead)
            return;

        _hp -= damage;
        DamageTextManager.I.Show(damage, transform.position, isCharge);
        Debug.Log(name + "의 현재 적 체력: " + _hp + " (데미지=" + damage + ")\n" + new System.Diagnostics.StackTrace(1, false).ToString());
        if (_hp <= 0)
        {
            _hp = 0;
            _isDead = true;
            OnDeath();
            return;
        }

        OnDamaged?.Invoke(damage);
    }

    /// <summary>
    /// 생성할 때 타겟 주입 필요
    /// </summary>
    public virtual void SetTarget(Transform player)
    {
        _target = player;
    }

    public void UnregisterFromEnemyCount()
    {
        if (_isEnemyCountRegistered && GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterEnemy(countAsKill: false);
            _isEnemyCountRegistered = false;
        }
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stopDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);
    }
#endif
}
