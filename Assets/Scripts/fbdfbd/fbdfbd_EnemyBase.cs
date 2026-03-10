using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class fbdfbd_EnemyBase : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Health")]
    [SerializeField] private int _hp;
    [SerializeField] private int _maxHp;
    [SerializeField] private bool _isDead;

    [Header("Movement")]
    [Min(0f)][SerializeField] private float _moveSpeed = 2f;
    [Min(0f)][SerializeField] private float _stopDistance = 1.2f;
    [Min(0f)][SerializeField] private float _moveLerpSpeed = 8f;

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

    protected Rigidbody2D Rb { get; private set; }
    public Transform Target => _target;
    public bool IsDead => _isDead;

    // Range Enemy에서 사용
    protected Vector2 LastDir { get; private set; } = Vector2.down;

    protected virtual bool CanAttack(float distanceToTarget) => false;
    protected abstract void DoAttack();

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Rb.gravityScale = 0f;
        Rb.freezeRotation = true;

        _noiseSeed = Random.Range(0f, 1000f);
        _personalOffset = Random.insideUnitCircle * _targetOffsetRadius;

        ScheduleNextAttack();

        // Ryeol_GameManager.Instance.RegisterEnemy();
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

        if (distToRealTarget <= _stopDistance)
        {
            _currentVelocity = Vector2.Lerp(_currentVelocity, Vector2.zero, _moveLerpSpeed * Time.fixedDeltaTime);
            Rb.linearVelocity = _currentVelocity;
            return;
        }

        Vector2 desiredMove = CalculateMoveDirection();
        Vector2 targetVelocity = desiredMove * _moveSpeed;

        _currentVelocity = Vector2.Lerp(_currentVelocity, targetVelocity, _moveLerpSpeed * Time.fixedDeltaTime);
        Rb.linearVelocity = _currentVelocity;
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

    protected virtual void OnDeath()
    {
        Rb.linearVelocity = Vector2.zero;

        Ryeol_GameManager.Instance.UnregisterEnemy();

        Debug.Log($"{gameObject.name} 죽었습니다");

        Destroy(gameObject);
        Ryeol_GameManager.Instance.AddScore(100);
    }

    private void ScheduleNextAttack()
    {
        float min = Mathf.Max(0f, _attackIntervalMin);
        float max = Mathf.Max(min, _attackIntervalMax);
        _nextAttackTime = Time.time + Random.Range(min, max);
    }

    public virtual void TakeDamage(int damage)
    {
        if (_isDead)
            return;

        _hp -= damage;
        WS_DamageTextManager.I.Show(damage, transform.position);
        if (_hp <= 0)
        {
            _hp = 0;
            _isDead = true;
            OnDeath();
        }
    }

    /// <summary>
    /// 생성할 때 타겟 주입 필요
    /// </summary>
    public virtual void SetTarget(Transform player)
    {
        _target = player;
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