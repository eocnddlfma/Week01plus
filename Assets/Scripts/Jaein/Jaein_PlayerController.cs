using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

public class Jaein_PlayerController : Jaein_PlayerBase
{
    public enum ColliderMode
    {
        Box,
        Polygon
    }

    [Header("Collider Settings")]
    [SerializeField] private ColliderMode _colliderMode = ColliderMode.Box;
    [SerializeField] private BoxCollider2D _weaponBoxCollider;
    [SerializeField] private PolygonCollider2D _weaponPolygonCollider;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private Transform _playerBody; // Body와 OrbitCenter를 별개로 회전시키기 위함

    // TODO: 차지 시 콜라이더의 크기를 변경하는 로직 진행 중
    [SerializeField] private Transform _boxWeaponTransform;
    [SerializeField] private Transform _polygonWeaponTransform;

    public Vector2 FacingDirection => _playerBody != null ? (Vector2)_playerBody.right : Vector2.right;

    [Header("Dash Settings")]
    [SerializeField] private KeyCode _dashKey = KeyCode.Space;
    [SerializeField] private float _dashSpeed = 25f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashCooldown = 0.75f;

    [Header("Combat Settings")]
    [SerializeField] private float _attackCooldown = 0f;
    [SerializeField] private float _attackDuration = 0.25f;

    [Header("Charge Settings")]
    [SerializeField] private float _maxChargeTime = 0.4f;
    [SerializeField] private float _chargeThreshold = 0.2f;
    [SerializeField] private float _minChargeScale = 1.0f;
    [SerializeField] private float _maxChargeScale = 1.25f;

    [Header("Animation")]
    [SerializeField] private Animator _pivotAnimator;
    [SerializeField] private string _attackTriggerName = "Attack";
    [SerializeField] private string _chargeBoolName = "IsCharging";
    [SerializeField] private string _fullChargeBoolName = "IsFullCharged";

    [Header("Boundary Settings")]
    [SerializeField] private Vector2 _boundaryCenter = Vector2.zero;
    [SerializeField] private float _boundaryRadius = 24f;
    [SerializeField] private bool _useBoundary = true;
    [SerializeField] private float _boundarySkin = 0.05f;

    private Vector2 _inputVec;
    private Camera _mainCamera;

    private Vector3 _initialScale;
    private Vector3 _initialBoxScale;
    private Vector3 _initialPolygonScale;

    private Jaein_WeaponChargeInfo _boxChargeInfo;
    private Jaein_WeaponChargeInfo _polygonChargeInfo;

    private Transform CurrentWeaponTransform => _colliderMode == ColliderMode.Box ? _boxWeaponTransform : _polygonWeaponTransform;
    private Vector3 InitialScale => _colliderMode == ColliderMode.Box ? _initialBoxScale : _initialPolygonScale;

    private float _lastDashTime = -100f;
    private bool _isDashing = false;

    private float _lastAttackTime;
    private float _currentChargeTimer = 0f;
    private bool _isAttacking = false;
    private bool _isCharging = false;
    private float _chargePercent;

    [Header("Camera Effect by WooSung")]
    [SerializeField] private WS_ChargeCameraEffect cameraEffect;

    protected override void Awake()
    {
        base.Awake();

        _mainCamera = Camera.main;

        if (_pivotAnimator == null) _pivotAnimator = GetComponentInChildren<Animator>();

        if (_weaponBoxCollider == null) _weaponBoxCollider = GetComponentInChildren<BoxCollider2D>();
        if (_weaponPolygonCollider == null) _weaponPolygonCollider = GetComponentInChildren<PolygonCollider2D>();

        if (_weaponBoxCollider != null)
            _boxChargeInfo = _weaponBoxCollider.GetComponent<Jaein_WeaponChargeInfo>();

        if (_weaponPolygonCollider != null)
            _polygonChargeInfo = _weaponPolygonCollider.GetComponent<Jaein_WeaponChargeInfo>();

        if (_boxWeaponTransform != null) _initialBoxScale = _boxWeaponTransform.localScale;
        if (_polygonWeaponTransform != null) _initialPolygonScale = _polygonWeaponTransform.localScale;

        DisableAllWeaponColliders();
    }

    private void DisableAllWeaponColliders()
    {
        if (_weaponBoxCollider != null) _weaponBoxCollider.enabled = false;
        if (_weaponPolygonCollider != null) _weaponPolygonCollider.enabled = false;
    }

    void Update()
    {
        if (_isDead) return;

        HandleInput();
        HandleDash();

        // 공격 중이거나 대쉬 중일 때는 시점 고정
        if (!_isAttacking && !_isDashing)
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
        if (!_isCharging && Input.GetKeyDown(_dashKey) && Time.time >= _lastDashTime + _dashCooldown && !_isDashing)
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

        float speed = _isCharging ? _moveSpeed * 0.5f : _moveSpeed;
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
        if (_isAttacking) return;

        // 마우스 누르는 시점
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time >= _lastAttackTime + _attackCooldown)
            {
                _currentChargeTimer = 0f;
                _isCharging = false; // 초기값은 false
                _chargePercent = 0f;
            }
        }

        // 마우스를 누르고 있는 동안
        if (Mouse.current.leftButton.isPressed)
        {
            _currentChargeTimer += Time.deltaTime;

            // 차지 스레시홀드 초과 시 차지 시작
            if (!_isCharging && _currentChargeTimer > _chargeThreshold)
            {
                _isCharging = true;

                if (_pivotAnimator != null) _pivotAnimator.SetBool(_chargeBoolName, true);
            }

            if (_isCharging)
            {
                float chargeTime = _currentChargeTimer - _chargeThreshold;
                _chargePercent = Mathf.Clamp01(chargeTime / (_maxChargeTime - _chargeThreshold));

                float multiplier = Mathf.Lerp(_minChargeScale, _maxChargeScale, _chargePercent);

                if (_boxWeaponTransform != null)
                {
                    _boxWeaponTransform.localScale = _initialBoxScale * multiplier;
                }

                if (_colliderMode == ColliderMode.Polygon && _polygonWeaponTransform != null)
                {
                    _polygonWeaponTransform.localScale = _initialPolygonScale * multiplier;
                }

                if (_pivotAnimator != null)
                {
                    _pivotAnimator.SetBool(_fullChargeBoolName, _chargePercent >= 1f);
                }

                cameraEffect.BeginCharge();
            }
        }

        // 마우스를 뗐을 때
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // Debug.Log("차지 유무: " + _isCharging + ", 차지 퍼센트: " + _chargePercent);
            ExecuteAttack(_chargePercent);
            cameraEffect.EndCharge(_chargePercent);
            _currentChargeTimer += Time.deltaTime;
        }
    }

    private void ExecuteAttack(float chargePercent)
    {
        // 공격 시점에 모든 차지 상태 해제
        _isCharging = false;

        if (_pivotAnimator != null)
        {
            _pivotAnimator.SetBool(_chargeBoolName, false);
            _pivotAnimator.SetBool(_fullChargeBoolName, false);
            _pivotAnimator.SetTrigger(_attackTriggerName);
        }

        SetChargeToCurrentWeapon(chargePercent);
        StartCoroutine(AttackRoutine(chargePercent));
    }
    private IEnumerator AttackRoutine(float chargePercent)
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;

        if (_pivotAnimator != null)
        {
            _pivotAnimator.ResetTrigger(_attackTriggerName);
            _pivotAnimator.SetTrigger(_attackTriggerName);
        }

        ToggleWeaponCollider(true);

        yield return new WaitForSeconds(_attackDuration);

        ToggleWeaponCollider(false);

        if (_boxWeaponTransform != null) _boxWeaponTransform.localScale = _initialBoxScale;
        if (_polygonWeaponTransform != null) _polygonWeaponTransform.localScale = _initialPolygonScale;

        ResetCurrentWeaponCharge();

        _isAttacking = false;
    }

    // BoxCollider와 PolygonCollider 각각에 차지 퍼센트를 전달하는 메서드
    private void SetChargeToCurrentWeapon(float percent)
    {
        if (_colliderMode == ColliderMode.Box && _boxChargeInfo != null)
            _boxChargeInfo.SetChargePercent(percent);
        else if (_colliderMode == ColliderMode.Polygon && _polygonChargeInfo != null)
            _polygonChargeInfo.SetChargePercent(percent);
    }

    private void ResetCurrentWeaponCharge()
    {
        if (_colliderMode == ColliderMode.Box && _boxChargeInfo != null)
            _boxChargeInfo.ResetCharge();
        else if (_colliderMode == ColliderMode.Polygon && _polygonChargeInfo != null)
            _polygonChargeInfo.ResetCharge();
    }

    // 박스 or 폴리곤 콜라이더 토글
    private void ToggleWeaponCollider(bool isEnable)
    {
        DisableAllWeaponColliders();

        if (!isEnable) return;

        switch (_colliderMode)
        {
            case ColliderMode.Box:
                if (_weaponBoxCollider != null) _weaponBoxCollider.enabled = true;
                break;
            case ColliderMode.Polygon:
                if (_weaponPolygonCollider != null) _weaponPolygonCollider.enabled = true;
                break;
        }
    }

    // Test: 적 투사체와 충돌 시 투사체 제거
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<fbdfbd_EnemyProjectile>(out _))
        {
            Destroy(other.gameObject);
        }
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