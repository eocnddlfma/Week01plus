using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int _hp;
    [SerializeField] protected int _maxHp;
    [SerializeField] protected bool _isDead;

    protected Rigidbody2D Rb { get; private set; }

    public int Hp => _hp;
    public int MaxHp => _maxHp;
    public bool IsDead => _isDead;

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Rb.gravityScale = 0;
    }

    public abstract void TakeDamage(int damage, bool isCharge = false);
    protected abstract void OnDeath();
}