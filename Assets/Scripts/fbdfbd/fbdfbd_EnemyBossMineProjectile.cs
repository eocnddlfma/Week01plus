using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class fbdfbd_EnemyBossMineProjectile : MonoBehaviour
{
    [Header("Runtime Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _triggerCollider;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Explosion Runtime")]
    [Min(0.01f)][SerializeField] private float _explosionTriggerTime = 0.08f;
    [Min(0f)][SerializeField] private float _fallbackExplosionRadius = 0.8f;
    [Min(0.01f)][SerializeField] private float _blinkMinInterval = 0.06f;
    [Min(0.01f)][SerializeField] private float _blinkMaxInterval = 0.16f;
    [SerializeField] private GameObject _explodeParticlePrefab;

    private int _damage;
    private Vector2 _direction;
    private float _currentSpeed;
    private float _deceleration;
    private float _stopSpeedThreshold;
    private float _blinkDuration;
    private float _explodeDelay;
    private LayerMask _targetMask;
    private GameObject _owner;

    private bool _isInitialized;
    private bool _isStopped;
    private bool _isExploded;

    private Coroutine _stopRoutine;
    private readonly HashSet<int> _damagedTargets = new HashSet<int>();
    private readonly Collider2D[] _overlapResults = new Collider2D[16];

    private void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        if (_triggerCollider == null)
            _triggerCollider = GetComponent<Collider2D>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _rb.gravityScale = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.freezeRotation = true;

        if (_triggerCollider != null)
            _triggerCollider.isTrigger = true;
    }

    public void Init(
        int damage,
        Vector2 direction,
        float speed,
        float deceleration,
        float stopSpeedThreshold,
        float blinkDuration,
        float explodeDelay,
        LayerMask targetMask,
        GameObject owner)
    {
        _damage = Mathf.Max(1, damage);
        _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
        _currentSpeed = Mathf.Max(0f, speed);
        _deceleration = Mathf.Max(0f, deceleration);
        _stopSpeedThreshold = Mathf.Max(0f, stopSpeedThreshold);
        _blinkDuration = Mathf.Max(0f, blinkDuration);
        _explodeDelay = Mathf.Max(0f, explodeDelay);
        _targetMask = targetMask;
        _owner = owner;

        _isInitialized = true;
        _isStopped = false;
        _isExploded = false;
        _damagedTargets.Clear();
    }

    public void WaitBeforeExplosion()
    {
        if (_stopRoutine != null || _isExploded)
            return;

        _stopRoutine = StartCoroutine(StopAndExplodeRoutine());
    }

    private void FixedUpdate()
    {
        if (!_isInitialized || _isStopped || _isExploded)
            return;

        _rb.position += _direction * _currentSpeed * Time.fixedDeltaTime;

        if (_deceleration > 0f)
            _currentSpeed = Mathf.Max(0f, _currentSpeed - (_deceleration * Time.fixedDeltaTime));

        if (_currentSpeed <= _stopSpeedThreshold)
        {
            _currentSpeed = 0f;
            _isStopped = true;
            WaitBeforeExplosion();
        }
    }

    private IEnumerator StopAndExplodeRoutine()
    {
        OnBlinkStartFx();

        if (_blinkDuration > 0f)
            yield return BlinkRoutine(_blinkDuration);

        if (_explodeDelay > 0f)
            yield return new WaitForSeconds(_explodeDelay);

        Explode();
    }

    private IEnumerator BlinkRoutine(float duration)
    {
        if (_spriteRenderer == null)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float blinkT = Mathf.Clamp01(duration <= 0.0001f ? 1f : elapsed / duration);
            float interval = Mathf.Lerp(_blinkMaxInterval, _blinkMinInterval, blinkT);

            _spriteRenderer.enabled = !_spriteRenderer.enabled;
            OnBlinkTickFx();

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        _spriteRenderer.enabled = true;
    }

    private void Explode()
    {
        if (_isExploded)
            return;

        _isExploded = true;
        OnExplodeFx();

        ApplyDamageToCurrentOverlaps();
        StartCoroutine(DestroyAfterExplodeWindow());
    }

    private IEnumerator DestroyAfterExplodeWindow()
    {
        yield return new WaitForSeconds(_explosionTriggerTime);
        Destroy(gameObject);
    }

    private void ApplyDamageToCurrentOverlaps()
    {
        if (_triggerCollider != null)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask = _targetMask;

            int count = _triggerCollider.OverlapCollider(filter, _overlapResults);
            for (int i = 0; i < count; i++)
                TryDamage(_overlapResults[i]);

            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _fallbackExplosionRadius, _targetMask);
        for (int i = 0; i < hits.Length; i++)
            TryDamage(hits[i]);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isExploded)
            return;

        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (other == null)
            return;

        if (_owner != null && other.gameObject == _owner)
            return;

        if ((_targetMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        int id = other.gameObject.GetInstanceID();
        if (_damagedTargets.Contains(id))
            return;

        Jaein_ObjectBase damageable = other.GetComponent<Jaein_ObjectBase>();
        if (damageable == null)
            return;

        _damagedTargets.Add(id);
        damageable.TakeDamage(_damage);
    }

    private void OnBlinkStartFx()
    {
    }

    private void OnBlinkTickFx()
    {
    }

    private void OnExplodeFx()
    {
        if (_explodeParticlePrefab == null)
            return;

        Instantiate(_explodeParticlePrefab, transform.position, Quaternion.identity);
    }
}
