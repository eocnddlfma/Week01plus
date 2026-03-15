using UnityEngine;

public class BossEnemyProjectile : MonoBehaviour, IEnemyProjectile
{

    [SerializeField]private int _damage;
    [SerializeField] private float _speed;
    [SerializeField] private float LifeTime = 4f;
    [SerializeField] private int usage = 1;

    private Rigidbody2D _rb;
    private float _spawnTime;
    [SerializeField] private LayerMask _targetMask;
    private GameObject _owner;
    private bool _isReflected;

    public Rigidbody2D Rb => _rb;
    public float Speed => _speed;
    public bool IsReflected => _isReflected;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        _rb.gravityScale = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Init(int damage, float speed, LayerMask targetMask, GameObject owner)
    {
        this._damage = damage;
        this._speed = speed;
        this._targetMask = targetMask;
        this._owner = owner;
        _spawnTime = Time.time;
    }

    private void FixedUpdate()
    {
        _rb.position += (Vector2)(-transform.up) * _speed * Time.fixedDeltaTime;

        if (Time.time - _spawnTime >= LifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.gameObject == _owner) return;
        if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent(out EntityBase target))
        {
            target.TakeDamage(_damage);
            Debug.Log($"Boss 투사체 타격, 데미지: {_damage}");
        }

        usage--;
        if (usage <= 0) Destroy(gameObject);
    }

    public void ReflectAsBatHit(int overrideDamage, LayerMask enemyMask)
    {
        transform.Rotate(0f, 0f, 180f);
        _damage = overrideDamage;
        _owner = null;
        _targetMask = enemyMask;
        _isReflected = true;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
    }
}
