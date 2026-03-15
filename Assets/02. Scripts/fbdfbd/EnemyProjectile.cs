using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IEnemyProjectile
{
    [SerializeField] private Rigidbody2D _rb;
    public Rigidbody2D Rb => _rb;
    public float Speed => _speed;

    private int _damage;
    private float _speed;
    private float _lifeTime;
    private float _spawnTime;
    private Vector2 _direction;
    private Vector2 _originalDirection;
    private float _originalSpeed;
    private float _deflectTimer;
    private LayerMask _targetMask;
    private GameObject _owner;
    private Color _originalColor;
    private bool _isReflected;

    public bool IsReflected => _isReflected;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        _originalColor = GetComponent<SpriteRenderer>().color;
        _rb.gravityScale = 0f;
    }

    public void Init(int damage, Vector2 direction, float speed, float lifeTime, LayerMask targetMask, GameObject owner)
    {
        this._damage = damage;
        this._direction = direction.normalized;
        this._speed = speed;
        this._lifeTime = lifeTime;
        this._targetMask = targetMask;
        this._owner = owner;
        _spawnTime = Time.time;
        _isReflected = false;
        _deflectTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (_deflectTimer > 0f)
        {
            _deflectTimer -= Time.fixedDeltaTime;
            if (_deflectTimer <= 0f)
            {
                _direction = _originalDirection;
                _speed = _originalSpeed;
            }
        }

        _rb.position += _direction * _speed * Time.fixedDeltaTime;

        if (_lifeTime > 0f && Time.time - _spawnTime >= _lifeTime)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.gameObject == _owner) return;
        if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent(out EntityBase dmg))
            dmg.TakeDamage(_damage);

        ReturnToPool();
    }

    public void Deflect(float duration, float speed)
    {
        _originalDirection = _direction;
        _originalSpeed = _speed;
        _direction = -_direction;
        _speed = speed;
        _deflectTimer = duration;
    }

    public void DisableProjectile()
    {
        ReturnToPool();
    }

    public void ReflectAsBatHit(int overrideDamage, LayerMask enemyMask)
    {
        _direction = -_direction;
        _originalDirection = _direction;
        _deflectTimer = 0f;
        _damage = overrideDamage;
        _owner = null;
        _targetMask = enemyMask;
        _isReflected = true;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
        _lifeTime += 10f;
    }

    private void ReturnToPool()
    {
        if (EnemyProjectilePool.Instance != null)
            EnemyProjectilePool.Instance.Return(this);
        else
            Destroy(gameObject);
    }

    private void OnDisable()
    {
        _deflectTimer = 0f;
        _isReflected = false;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = _originalColor;
    }
}
