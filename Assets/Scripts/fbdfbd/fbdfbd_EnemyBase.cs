using Unity.VisualScripting;
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

    [Header("Attack Timing")]
    [Min(0f)][SerializeField] private float _attackIntervalMin = 1.0f;
    [Min(0f)][SerializeField] private float _attackIntervalMax = 2.0f;

    private float _nextAttackTime;

    protected Rigidbody2D Rb { get; private set; }
    public Transform Target => _target;
    public bool IsDead => _isDead;

    //Range Enemy에서 사용(내부 계산 오류시 사용, 타겟 방향 계산 실패시 마지막 방향 유지)
    protected Vector2 LastDir { get; private set; } = Vector2.down;

    protected virtual bool CanAttack(float distanceToTarget) => false;
    protected abstract void DoAttack();

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Rb.gravityScale = 0;
        ScheduleNextAttack();

        Ryeol_GameManager.Instance.RegisterEnemy();
    }

    protected virtual void Update()
    {
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
        if (_target == null) return;

        Vector2 toTarget = (Vector2)_target.position - Rb.position;
        float dist = toTarget.magnitude;

        if (dist > _stopDistance)
        {
            Vector2 vel = toTarget.normalized * _moveSpeed;
            Rb.linearVelocity = vel;
        }
        else
        {
            Rb.linearVelocity = Vector2.zero;
        }
    }

    protected virtual void OnDeath()
    {
        Ryeol_GameManager.Instance.UnregisterEnemy();

        Debug.Log($"{gameObject.name} 죽었습니다");

        //김우성 추가: dev
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
        if (_isDead) return;
        _hp -= damage;
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
}
