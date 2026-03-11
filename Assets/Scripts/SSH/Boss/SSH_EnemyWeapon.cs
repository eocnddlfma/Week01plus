using UnityEngine;

public class SSH_EnemyWeapon : MonoBehaviour
{
    [SerializeField] private int       _damage;
    [SerializeField] private float     _speed;
    [SerializeField] private float     _lifeTime = 4f;
    [SerializeField] private int       _usage    = 1;
    [SerializeField] private LayerMask _targetMask;

    private Rigidbody2D _rb;
    private float       _spawnTime;
    private GameObject  _owner;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        _rb.gravityScale = 0f;
        _rb.bodyType     = RigidbodyType2D.Kinematic;
    }

    public void Init(int damage, float speed, LayerMask targetMask, GameObject owner)
    {
        _damage     = damage;
        _speed      = speed;
        _targetMask = targetMask;
        _owner      = owner;
        _spawnTime  = Time.time;
    }

    private void FixedUpdate()
    {
        _rb.position += (Vector2)(-transform.up) * _speed * Time.fixedDeltaTime;

        if (Time.time - _spawnTime >= _lifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.gameObject == _owner) return;
        if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;

        other.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);

        _usage--;
        if (_usage <= 0) Destroy(gameObject);
    }
}
