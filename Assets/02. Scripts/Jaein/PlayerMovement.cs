using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerBody;
    [SerializeField] private BatWeaponManager _weaponSystem;
    [SerializeField] private Rigidbody2D _rb;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Boundary Settings")]
    [SerializeField] private Vector2 _boundaryCenter = Vector2.zero;
    [SerializeField] private float _boundaryRadius = 24f;
    [SerializeField] private bool _useBoundary = true;
    [SerializeField] private float _boundarySkin = 0.05f;

    private Camera _mainCamera;
    private bool _isDashing = false;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_playerBody == null)
        {
            var body = FindAnyObjectByType<Transform>();
            if (body != null)
                _playerBody = body;
        }
        if (_weaponSystem == null)
        {
            var weapon = FindAnyObjectByType<BatWeaponManager>();
            if (weapon != null)
                _weaponSystem = weapon;
        }
        _mainCamera = Camera.main;
    }

    public void SetDashingState(bool dashing)
    {
        _isDashing = dashing;
    }

    public void SetStats(float moveSpeed, float rotationSpeed)
    {
        _moveSpeed = moveSpeed;
        _rotationSpeed = rotationSpeed;
    }

    public void Move(Vector2 inputVec)
    {
        if (_rb == null || _isDashing) return;

        float statMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.MoveSpeedMult : 1f;
        float speed = (_weaponSystem.IsCharging ? _moveSpeed * 0.5f : _moveSpeed) * statMult;
        Vector2 desiredVelocity = inputVec * speed;

        if (_useBoundary && desiredVelocity.sqrMagnitude > 0.0001f)
        {
            Vector2 currentPos = _rb.position;
            Vector2 toPlayer = currentPos - _boundaryCenter;
            float maxRadius = Mathf.Max(0f, _boundaryRadius - _boundarySkin);

            if (toPlayer.sqrMagnitude >= maxRadius * maxRadius)
            {
                Vector2 normal = toPlayer.normalized;
                float outwardDot = Vector2.Dot(desiredVelocity, normal);

                if (outwardDot > 0f)
                {
                    desiredVelocity -= normal * outwardDot;
                }
            }
        }

        _rb.linearVelocity = desiredVelocity;
    }

    public void RotateTowardsMouse()
    {
        if (_mainCamera == null || Mouse.current == null || _weaponSystem.IsAttacking || _isDashing)
            return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -_mainCamera.transform.position.z));

        Vector2 direction = new Vector2(mouseWorldPos.x - transform.position.x, mouseWorldPos.y - transform.position.y);

        if (direction.sqrMagnitude > 0.001f)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            _playerBody.rotation = Quaternion.Slerp(_playerBody.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    public void ClampInsideBoundary()
    {
        if (!_useBoundary || _rb == null) return;

        Vector2 currentPos = _rb.position;
        Vector2 clampedPos = GetClampedPosition(currentPos);

        if ((currentPos - clampedPos).sqrMagnitude > 0.000001f)
        {
            Vector2 normal = (currentPos - _boundaryCenter).normalized;
            _rb.position = clampedPos;

            float outwardSpeed = Vector2.Dot(_rb.linearVelocity, normal);
            if (outwardSpeed > 0f)
            {
                _rb.linearVelocity -= normal * outwardSpeed;
            }
        }
    }

    private Vector2 GetClampedPosition(Vector2 targetPosition)
    {
        if (!_useBoundary) return targetPosition;

        Vector2 toTarget = targetPosition - _boundaryCenter;
        float maxRadius = Mathf.Max(0f, _boundaryRadius - _boundarySkin);

        if (toTarget.sqrMagnitude <= maxRadius * maxRadius) return targetPosition;
        return _boundaryCenter + toTarget.normalized * maxRadius;
    }

    public float GetMaxDashDistance(Vector2 startPos, Vector2 dashDir, float desiredDistance)
    {
        if (!_useBoundary) return desiredDistance;

        dashDir = dashDir.normalized;
        Vector2 oc = startPos - _boundaryCenter;
        float radius = Mathf.Max(0f, _boundaryRadius - _boundarySkin);

        float a = Vector2.Dot(dashDir, dashDir);
        float b = 2f * Vector2.Dot(oc, dashDir);
        float c = Vector2.Dot(oc, oc) - radius * radius;

        float discriminant = b * b - 4f * a * c;
        if (discriminant < 0f) return 0f;

        float sqrtD = Mathf.Sqrt(discriminant);
        float t1 = (-b - sqrtD) / (2f * a);
        float t2 = (-b + sqrtD) / (2f * a);
        float maxT = Mathf.Max(t1, t2);

        if (maxT < 0f) return 0f;
        return Mathf.Min(desiredDistance, maxT);
    }
}
