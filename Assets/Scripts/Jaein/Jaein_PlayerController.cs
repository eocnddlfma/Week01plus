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

    private Vector2 _inputVec;
    private Camera _mainCamera;

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

        float originalDrag = Rb.linearDamping;
        Rb.linearDamping = 0;
        Rb.linearVelocity = dir.normalized * _dashSpeed;

        yield return new WaitForSeconds(_dashDuration);

        Rb.linearDamping = originalDrag;

        _isDashing = false;
    }

    private void Move()
    {
        if (Rb != null)
        {
            float speed = _isCharging ? _moveSpeed * 0.5f : _moveSpeed; // 차지 중에는 이동 속도 감소
            Rb.linearVelocity = _inputVec * speed;
        }
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

                // 비주얼 처리 파트
                if (CurrentWeaponTransform != null)
                {
                    float multiplier = Mathf.Lerp(_minChargeScale, _maxChargeScale, _chargePercent);
                    CurrentWeaponTransform.localScale = InitialScale * multiplier;
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
            _currentChargeTimer += Time.deltaTime;
            float chargePercent = Mathf.Clamp01(_currentChargeTimer / _maxChargeTime);

            if (_weaponTransform != null)
            {
                float multiplier = Mathf.Lerp(_minChargeScale, _maxChargeScale, chargePercent);
                _weaponTransform.localScale = _initialWeaponScale * multiplier;
            }

            if (_currentChargeTimer >= _maxChargeTime)
            {
                if (_pivotAnimator != null && !_pivotAnimator.GetBool(_fullChargeBoolName))
                {
                    _pivotAnimator.SetBool(_fullChargeBoolName, true);
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isCharging = false;
                cameraEffect.EndCharge(chargePercent);

                if (_pivotAnimator != null)
                {
                    _pivotAnimator.SetBool(_chargeBoolName, false);
                    _pivotAnimator.SetBool(_fullChargeBoolName, false);
                }

                // 무기에 차지 퍼센트 정보 설정 (위성이 충돌 시 이 값을 읽음)
                if (_weaponChargeInfo != null)
                {
                    _weaponChargeInfo.SetChargePercent(chargePercent);
                }

                StartCoroutine(AttackRoutine(chargePercent));
            }
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

        if (CurrentWeaponTransform != null) CurrentWeaponTransform.localScale = InitialScale;

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

    // 공격 시 박스 or 폴리곤 콜라이더 토글
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
}