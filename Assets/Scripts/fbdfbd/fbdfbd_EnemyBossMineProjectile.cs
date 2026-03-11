using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class fbdfbd_EnemyBossMineProjectile : MonoBehaviour
{
    [Header("Runtime Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _explodeParticle;

    [Header("Explosion Runtime")]
    [Min(0f)][SerializeField] private float _explosionRadius = 0.8f;
    [Min(0.01f)][SerializeField] private float _blinkMinInterval = 0.06f;
    [Min(0.01f)][SerializeField] private float _blinkMaxInterval = 0.16f;

    private int _damage;
    private Vector2 _direction;
    private float _currentSpeed;
    private float _deceleration;
    private float _stopSpeedThreshold;
    private float _blinkDuration;
    private float _explodeDelay;
    private LayerMask _targetMask;
    private GameObject _owner;
    private Transform _target;

    private bool _isInitialized;
    private bool _isStopped;
    private bool _isExploded;
    private Coroutine _stopRoutine;

    private void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_explodeParticle == null)
            _explodeParticle = GetComponentInChildren<ParticleSystem>(true);

        _rb.gravityScale = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.freezeRotation = true;

        if (_explodeParticle != null)
            _explodeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
        GameObject owner,
        Transform target)
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
        _target = target;

        _isInitialized = true;
        _isStopped = false;
        _isExploded = false;
        _stopRoutine = null;

        if (_spriteRenderer != null)
            _spriteRenderer.enabled = true;

        if (_explodeParticle != null)
            _explodeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        ApplyDamageToInjectedTarget();
        float fxLifeTime = OnExplodeFx();
        StartCoroutine(DestroyAfterExplodeWindow(fxLifeTime));
    }

    private void ApplyDamageToInjectedTarget()
    {
        if (_target == null)
            return;

        if (_owner != null && _target.gameObject == _owner)
            return;

        if ((_targetMask.value & (1 << _target.gameObject.layer)) == 0)
            return;

        float radius = Mathf.Max(0f, _explosionRadius);
        Vector2 delta = (Vector2)_target.position - _rb.position;
        if (delta.sqrMagnitude > radius * radius)
            return;

        Debug.Log("Mine explosion hit target in range.");

        Jaein_ObjectBase damageable = _target.GetComponent<Jaein_ObjectBase>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
        }
    }

    private IEnumerator DestroyAfterExplodeWindow(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    private void OnBlinkStartFx()
    {
    }

    private void OnBlinkTickFx()
    {
    }

    private float OnExplodeFx()
    {
        if (_explodeParticle == null)
            return 0f;

        if (!_explodeParticle.gameObject.activeSelf)
            _explodeParticle.gameObject.SetActive(true);

        _explodeParticle.Play(true);
        return EstimateFxLifeTime(_explodeParticle);
    }

    private float EstimateFxLifeTime(ParticleSystem rootParticle)
    {
        if (rootParticle == null)
            return 0f;

        ParticleSystem[] systems = rootParticle.GetComponentsInChildren<ParticleSystem>(true);
        if (systems == null || systems.Length == 0)
            return 0f;

        float maxLife = 0f;

        for (int i = 0; i < systems.Length; i++)
        {
            ParticleSystem.MainModule main = systems[i].main;
            float startDelay = GetCurveMax(main.startDelay);
            float duration = Mathf.Max(0f, main.duration);
            float startLife = GetCurveMax(main.startLifetime);
            float candidate = startDelay + duration + startLife;

            if (candidate > maxLife)
                maxLife = candidate;
        }

        return maxLife + 0.1f;
    }

    private static float GetCurveMax(ParticleSystem.MinMaxCurve curve)
    {
        float value = curve.constantMax;

        if (value <= 0f)
            value = curve.constant;

        if (value <= 0f)
            value = curve.curveMultiplier;

        return Mathf.Max(0f, value);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 center = Application.isPlaying && _rb != null
            ? (Vector3)_rb.position
            : transform.position;

        float radius = Mathf.Max(0f, _explosionRadius);
        Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.35f);
        Gizmos.DrawSphere(center, radius);
        Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.95f);
        Gizmos.DrawWireSphere(center, radius);
    }
#endif

}
