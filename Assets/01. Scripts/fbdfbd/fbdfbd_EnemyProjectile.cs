using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IEnemyProjectile
{
    [SerializeField] private Rigidbody2D _rb;

    public Rigidbody2D Rb => _rb;

    private int _damage;
    private float _speed;
    private float _lifeTime;
    private float _spawnTime;
    private Vector2 _direction;
    private LayerMask _targetMask;
    private GameObject _owner;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        _rb.gravityScale = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
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
    }

    private void FixedUpdate()
    {
        _rb.position += _direction * _speed * Time.fixedDeltaTime;

        if (_lifeTime > 0f && Time.time - _spawnTime >= _lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.gameObject == _owner) return;
        if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;


        if (other.TryGetComponent(out EntityBase dmg))
        {
            dmg.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }

    public void ReflectAsBatHit(int overrideDamage, LayerMask enemyMask, Color hitColor)
    {
        _direction = -_direction;
        _damage = overrideDamage;
        _owner = null;
        _targetMask = enemyMask;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = hitColor;
    }
}
