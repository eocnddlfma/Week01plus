using UnityEngine;

public class Jaein_OrbitalWeapon : MonoBehaviour
{
    private enum BallState
    {
        Orbit,
        Launched,
        Returning
    }

    [Header("Center Reference")]
    [SerializeField] private Transform _center;

    [Header("Orbit Settings")]
    [SerializeField] private float _orbitRadius = 2.0f;
    [SerializeField] private float _orbitAngularSpeed = 180.0f;
    [SerializeField] private float _startAngle = 0.0f;

    [Header("Launch Settings")]
    [SerializeField] private KeyCode _testHitKey = KeyCode.Space;
    [SerializeField] private float _launchSpeed = 14.0f;
    [SerializeField] private float _launchDuration = 0.35f;
    [SerializeField] private float _randomAngleOffset = 10.0f;

    [Header("Return - Physics")]
    [SerializeField] private float _returnStrength = 16.0f;
    [SerializeField] private float _returnDamping = 1.0f;
    [SerializeField] private float _orbitAssistStrength = 12.0f;
    [SerializeField] private bool _useCounterClockwiseAssist = true;
    [SerializeField] private float _maxReturnSpeed = 18.0f;

    [Header("Rejoin Settings")]
    [SerializeField] private float _rejoinDistanceToOrbit = 0.2f;
    [SerializeField] private float _rejoinVelocityLimit = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool _drawOrbitGizmo = true;

    private BallState _state = BallState.Orbit;
    private float _angleDeg;
    private float _stateTimer;
    private Vector2 _velocity;

    private void Start()
    {
        _angleDeg = _startAngle;
        SnapToOrbit();
    }

    private void Update()
    {
        if (_center == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0.0f) return;

        HandleInput();
        UpdateState(dt);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(_testHitKey))
        {
            Launch();
        }
    }

    private void UpdateState(float dt)
    {
        switch (_state)
        {
            case BallState.Orbit:
                UpdateOrbitState(dt);
                break;

            case BallState.Launched:
                UpdateLaunchedState(dt);
                break;

            case BallState.Returning:
                UpdateReturningState(dt);
                break;
        }
    }

    private void UpdateOrbitState(float dt)
    {
        _angleDeg += _orbitAngularSpeed * dt;
        NormalizeAngle();

        Vector2 radialDir = GetRadialDirection(_angleDeg);
        transform.position = (Vector2)_center.position + radialDir * _orbitRadius;
    }

    private void UpdateLaunchedState(float dt)
    {
        _stateTimer -= dt;
        transform.position = (Vector2)transform.position + _velocity * dt;

        if (_stateTimer <= 0.0f)
        {
            _state = BallState.Returning;
        }
    }

    private void UpdateReturningState(float dt)
    {
        Vector2 currentPos = transform.position;
        Vector2 fromCenter = currentPos - (Vector2)_center.position;
        float distanceToCenter = fromCenter.magnitude;

        if (distanceToCenter <= 0.0001f)
        {
            fromCenter = GetRadialDirection(_angleDeg) * 0.001f;
            distanceToCenter = fromCenter.magnitude;
        }

        Vector2 radialDir = fromCenter / distanceToCenter;
        Vector2 toCenterDir = -radialDir;

        Vector2 tangentDir = _useCounterClockwiseAssist
            ? new Vector2(-radialDir.y, radialDir.x)
            : new Vector2(radialDir.y, -radialDir.x);

        Vector2 pullAcceleration = toCenterDir * _returnStrength;
        Vector2 orbitAssistAcceleration = tangentDir * _orbitAssistStrength;
        Vector2 dampingAcceleration = -_velocity * _returnDamping;

        Vector2 totalAcceleration = pullAcceleration + orbitAssistAcceleration + dampingAcceleration;

        _velocity += totalAcceleration * dt;

        if (_velocity.magnitude > _maxReturnSpeed)
        {
            _velocity = _velocity.normalized * _maxReturnSpeed;
        }

        currentPos += _velocity * dt;
        transform.position = currentPos;

        float distanceToOrbit = Mathf.Abs(distanceToCenter - _orbitRadius);

        if (distanceToOrbit <= _rejoinDistanceToOrbit)
        {
            if (_velocity.magnitude <= _rejoinVelocityLimit * 1.5f)
            {
                RejoinOrbit(currentPos);
            }
        }
    }

    public void Launch()
    {
        if (_center == null) return;

        Vector2 currentPos = transform.position;
        Vector2 radialDir = ((Vector2)currentPos - (Vector2)_center.position).normalized;

        if (radialDir.sqrMagnitude <= 0.0001f)
        {
            radialDir = GetRadialDirection(_angleDeg);
        }

        Vector2 tangentDir = new Vector2(-radialDir.y, radialDir.x);

        float randomOffset = Random.Range(-_randomAngleOffset, _randomAngleOffset);
        Vector2 launchDir = RotateVector(tangentDir, randomOffset).normalized;

        _velocity = launchDir * _launchSpeed;
        _stateTimer = _launchDuration;
        _state = BallState.Launched;
    }

    private void RejoinOrbit(Vector2 currentPos)
    {
        Vector2 fromCenter = currentPos - (Vector2)_center.position;

        if (fromCenter.sqrMagnitude <= 0.0001f)
        {
            fromCenter = GetRadialDirection(_angleDeg) * _orbitRadius;
        }

        Vector2 radialDir = fromCenter.normalized;
        _angleDeg = Mathf.Atan2(radialDir.y, radialDir.x) * Mathf.Rad2Deg;
        NormalizeAngle();

        _velocity = Vector2.zero;
        _state = BallState.Orbit;

        SnapToOrbit();
    }

    private void SnapToOrbit()
    {
        Vector2 radialDir = GetRadialDirection(_angleDeg);
        transform.position = (Vector2)_center.position + radialDir * _orbitRadius;
    }

    private Vector2 GetRadialDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private Vector2 RotateVector(Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    private void NormalizeAngle()
    {
        while (_angleDeg >= 360.0f) _angleDeg -= 360.0f;
        while (_angleDeg < 0.0f) _angleDeg += 360.0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            if (_state != BallState.Launched)
            {
                Debug.Log("Hit! State: " + _state);
                Launch();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_drawOrbitGizmo || _center == null) return;

        Gizmos.color = Color.red;
        const int segmentCount = 96;
        Vector3 prev = _center.position + Vector3.right * _orbitRadius;

        for (int i = 1; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            float angle = t * Mathf.PI * 2.0f;
            Vector3 next = _center.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f) * _orbitRadius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}