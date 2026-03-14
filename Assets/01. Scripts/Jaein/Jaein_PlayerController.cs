using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

public class PlayerController : PlayerBase
{
    [Header("Data")]
    [SerializeField] private PlayerStatsData _statsData;

    [Header("References")]
    [SerializeField] private Transform _playerBody;

    [Header("Weapon System")]
    [SerializeField] private BatWeaponManager _weaponSystem;

    public Vector2 FacingDirection => _playerBody != null ? (Vector2)_playerBody.right : Vector2.right;

    [Header("Input")]
    [SerializeField] private KeyCode _dashKey = KeyCode.Space;

    // 런타임 값 (SO에서 초기화)
    private float _moveSpeed = 5f;
    private float _rotationSpeed = 10f;
    private float _dashSpeed = 25f;
    private float _dashDuration = 0.15f;
    private float _dashCooldown = 0.75f;

    [Header("Boundary Settings")]
    [SerializeField] private Vector2 _boundaryCenter = Vector2.zero;
    [SerializeField] private float _boundaryRadius = 24f;
    [SerializeField] private bool _useBoundary = true;
    [SerializeField] private float _boundarySkin = 0.05f;

    [Header("Camera Effect by WooSung")]
    [SerializeField] private ChargeCameraEffect cameraEffect;

    private Vector2 _inputVec;
    private Camera _mainCamera;
    private float _lastDashTime = -100f;
    private bool _isDashing = false;

    protected override void Awake()
    {
        base.Awake();

        _mainCamera = Camera.main;

        if (_weaponSystem == null)
            _weaponSystem = GetComponentInChildren<BatWeaponManager>();

        InitFromStatsData();
    }

    private void InitFromStatsData()
    {
        if (_statsData == null) return;
        _moveSpeed = _statsData.moveSpeed;
        _rotationSpeed = _statsData.rotationSpeed;
        _dashSpeed = _statsData.dashSpeed;
        _dashDuration = _statsData.dashDuration;
        _dashCooldown = _statsData.dashCooldown;
    }

    void Update()
    {
        if (_isDead) return;

        HandleInput();
        HandleDash();

        // 공격 중이거나 대쉬 중일 때는 시점 고정
        if (!_weaponSystem.IsAttacking && !_isDashing)
        {
            RotateTowardsMouse();
        }

        HandleAttack();
    }

    void FixedUpdate()
    {
        if (_isDead || _isDashing) return;

        Move();
        ClampInsideBoundary();
    }

    private void HandleInput()
    {
        // New Input System
        float h = 0;
        float v = 0;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) v = 1;
            if (keyboard.sKey.isPressed) v = -1;
            if (keyboard.aKey.isPressed) h = -1;
            if (keyboard.dKey.isPressed) h = 1;
        }

        _inputVec = new Vector2(h, v).normalized;
    }

    private void HandleDash()
    {
        float dashCD = _dashCooldown * (PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.DashCooldownMult : 1f);
        if (!_weaponSystem.IsCharging && Input.GetKeyDown(_dashKey) && Time.time >= _lastDashTime + dashCD && !_isDashing)
        {
            // 입력 방향이 없으면 바라보는 방향으로, 있으면 입력 방향으로 대쉬
            Vector2 dashDir = _inputVec.sqrMagnitude > 0.001f ? _inputVec : (Vector2)_playerBody.right;
            StartCoroutine(DashRoutine(dashDir));
        }
    }

    private IEnumerator DashRoutine(Vector2 dir)
    {
        _isDashing = true;
        _lastDashTime = Time.time;

        StartCoroutine(InvincibleRoutine(false));

        dir = dir.normalized;

        float originalDrag = Rb.linearDamping;
        Rb.linearDamping = 0f;

        float desiredDashDistance = _dashSpeed * _dashDuration;
        float allowedDashDistance = GetMaxDashDistanceInBoundary(Rb.position, dir, desiredDashDistance);

        if (allowedDashDistance <= 0.001f)
        {
            Rb.linearVelocity = Vector2.zero;
            Rb.linearDamping = originalDrag;
            _isDashing = false;
            yield break;
        }

        float actualDashDuration = allowedDashDistance / _dashSpeed;
        Rb.linearVelocity = dir * _dashSpeed;

        yield return new WaitForSeconds(actualDashDuration);

        Rb.linearVelocity = Vector2.zero;
        Rb.linearDamping = originalDrag;

        ClampInsideBoundary();

        _isDashing = false;
    }

    private void Move()
    {
        if (Rb == null)
            return;

        float statMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.MoveSpeedMult : 1f;
        float speed = (_weaponSystem.IsCharging ? _moveSpeed * 0.5f : _moveSpeed) * statMult;
        Vector2 desiredVelocity = _inputVec * speed;

        if (_useBoundary && desiredVelocity.sqrMagnitude > 0.0001f)
        {
            Vector2 currentPos = Rb.position;
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

        Rb.linearVelocity = desiredVelocity;
    }

    private void RotateTowardsMouse()
    {
        if (_mainCamera == null || Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -_mainCamera.transform.position.z));

        Vector2 direction = new Vector2(mouseWorldPos.x - transform.position.x, mouseWorldPos.y - transform.position.y);

        if (direction.sqrMagnitude > 0.001f)
        {
            // 1. 목표 각도 계산
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            // 2. Slerp(구면 선형 보간)를 사용하여 현재 회전에서 목표 회전까지 부드럽게 이동
            _playerBody.rotation = Quaternion.Slerp(_playerBody.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleAttack()
    {
        if (_weaponSystem == null) return;

        bool pressed = Mouse.current.leftButton.wasPressedThisFrame;
        bool held = Mouse.current.leftButton.isPressed;
        bool released = Mouse.current.leftButton.wasReleasedThisFrame;

        _weaponSystem.HandleAttackInput(pressed, held, released);

        // 카메라 이펙트
        if (released)
        {
            cameraEffect.EndCharge(_weaponSystem.ChargePercent);
        }
        else if (held && _weaponSystem.IsCharging)
        {
            cameraEffect.BeginCharge();
        }
    }

    // 투사체 처리 (무기와의 충돌은 BatWeaponManager에서 처리)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IEnemyProjectile>(out _))
            Destroy(other.gameObject);
    }

    private Vector2 GetClampedPosition(Vector2 targetPosition)
    {
        if (!_useBoundary)
            return targetPosition;

        Vector2 toTarget = targetPosition - _boundaryCenter;
        float maxRadius = Mathf.Max(0f, _boundaryRadius - _boundarySkin);

        if (toTarget.sqrMagnitude <= maxRadius * maxRadius)
            return targetPosition;

        return _boundaryCenter + toTarget.normalized * maxRadius;
    }

    private void ClampInsideBoundary()
    {
        if (!_useBoundary || Rb == null)
            return;

        Vector2 currentPos = Rb.position;
        Vector2 clampedPos = GetClampedPosition(currentPos);

        if ((currentPos - clampedPos).sqrMagnitude > 0.000001f)
        {
            Vector2 normal = (currentPos - _boundaryCenter).normalized;

            Rb.position = clampedPos;

            float outwardSpeed = Vector2.Dot(Rb.linearVelocity, normal);
            if (outwardSpeed > 0f)
            {
                Rb.linearVelocity -= normal * outwardSpeed;
            }
        }
    }

    private float GetMaxDashDistanceInBoundary(Vector2 startPos, Vector2 dashDir, float desiredDistance)
    {
        if (!_useBoundary)
            return desiredDistance;

        dashDir = dashDir.normalized;
        Vector2 oc = startPos - _boundaryCenter;
        float radius = Mathf.Max(0f, _boundaryRadius - _boundarySkin);

        float a = Vector2.Dot(dashDir, dashDir);
        float b = 2f * Vector2.Dot(oc, dashDir);
        float c = Vector2.Dot(oc, oc) - radius * radius;

        float discriminant = b * b - 4f * a * c;

        if (discriminant < 0f)
            return 0f;

        float sqrtD = Mathf.Sqrt(discriminant);
        float t1 = (-b - sqrtD) / (2f * a);
        float t2 = (-b + sqrtD) / (2f * a);

        float maxT = Mathf.Max(t1, t2);

        if (maxT < 0f)
            return 0f;

        return Mathf.Min(desiredDistance, maxT);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(_boundaryCenter.x, _boundaryCenter.y, 0f), _boundaryRadius);
    }
}