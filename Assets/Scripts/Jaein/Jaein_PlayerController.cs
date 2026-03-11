using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

public class Jaein_PlayerController : Jaein_PlayerBase
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private Transform _playerBody; // Body와 OrbitCenter를 별개로 회전시키기 위함
    [SerializeField] private Transform _weaponTransform;

    public Vector2 FacingDirection => _playerBody != null ? (Vector2)_playerBody.right : Vector2.right;

    [Header("Dash Settings")]
    [SerializeField] private KeyCode _dashKey = KeyCode.Space;
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashCooldown = 1.0f;

    [Header("Combat Settings")]
    [SerializeField] private float _attackCooldown = 0.75f;
    [SerializeField] private float _attackDuration = 0.65f;

    [Header("Charge Settings")]
    [SerializeField] private float _maxChargeTime = 2.0f;
    [SerializeField] private float _minChargeScale = 1.0f;
    [SerializeField] private float _maxChargeScale = 2.5f;

    [Header("Animation")]
    [SerializeField] private Animator _pivotAnimator;
    [SerializeField] private string _attackTriggerName = "Attack";
    [SerializeField] private string _chargeBoolName = "IsCharging";
    [SerializeField] private string _fullChargeBoolName = "IsFullCharged";

    private Vector2 _inputVec;
    private Camera _mainCamera;
    private Vector3 _initialWeaponScale; // 무기의 초기 로컬 스케일 저장용

    private BoxCollider2D _weaponCollider;
    private Jaein_WeaponChargeInfo _weaponChargeInfo;

    private float _lastDashTime = -100f;
    private bool _isDashing = false;

    private float _lastAttackTime;
    private float _currentChargeTimer = 0f;
    private bool _isAttacking = false;
    private bool _isCharging = false;

    [Header("Camera Effect by WooSung")]
    [SerializeField] private WS_ChargeCameraEffect cameraEffect;

    protected override void Awake()
    {
        base.Awake();

        _mainCamera = Camera.main;

        if (_pivotAnimator == null) _pivotAnimator = GetComponentInChildren<Animator>();

        if (_weaponCollider == null) _weaponCollider = GetComponentInChildren<BoxCollider2D>();

        if (_weaponTransform == null && _weaponCollider != null) _weaponTransform = _weaponCollider.transform;

        if (_weaponTransform != null) _initialWeaponScale = _weaponTransform.localScale;

        if (_weaponCollider != null)
        {
            _weaponCollider.enabled = false;
            _weaponChargeInfo = _weaponCollider.GetComponent<Jaein_WeaponChargeInfo>();
            if (_weaponChargeInfo == null)
            {
                _weaponChargeInfo = _weaponCollider.gameObject.AddComponent<Jaein_WeaponChargeInfo>();
            }
        }
    }

    void Update()
    {
        if (_isDead) return;

        // Debug
        //if (Keyboard.current.tKey.wasPressedThisFrame)
        //{
        //    Debug.Log("[Debug] T Key Pressed: Taking 20 Damage");
        //    TakeDamage(20); // PlayerBase에 구현된 TakeDamage 호출
        //}

        HandleInput();
        HandleDash();

        // 공격 중이거나 대쉬 중일 때는 마우스 방향을 바라보지 않음
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
        if (Input.GetKeyDown(_dashKey) && Time.time >= _lastDashTime + _dashCooldown && !_isDashing)
        {
            Debug.Log("Dash");
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

        // Rb.linearVelocity = Vector2.zero; 

        _isDashing = false;
    }

    private void Move()
    {
        if (Rb != null)
        {
            float speed = _isCharging ? _moveSpeed * 0.5f : _moveSpeed; // Charging 중에는 이동 속도 감소
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

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time >= _lastAttackTime + _attackCooldown)
            {
                _isCharging = true;
                _currentChargeTimer = 0f;

                if (_pivotAnimator != null)
                {
                    _pivotAnimator.SetBool(_chargeBoolName, true);
                    _pivotAnimator.SetBool(_fullChargeBoolName, false);
                }

                cameraEffect.BeginCharge();
            }
        }

        if (_isCharging)
        {
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

    private IEnumerator AttackRoutine(float chargePercent)
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;

        if (_pivotAnimator != null)
        {
            _pivotAnimator.ResetTrigger(_attackTriggerName);
            _pivotAnimator.SetTrigger(_attackTriggerName);
        }

        // 무기와 위성의 충돌 시 WeaponChargeInfo를 통해 chargePercent를 전달

        if (_weaponCollider != null) _weaponCollider.enabled = true;

        yield return new WaitForSeconds(_attackDuration);

        if (_weaponCollider != null) _weaponCollider.enabled = false;

        if (_weaponTransform != null)
        {
            _weaponTransform.localScale = _initialWeaponScale * _minChargeScale;
        }

        // 공격 종료 시 차지 정보 리셋
        if (_weaponChargeInfo != null)
        {
            _weaponChargeInfo.ResetCharge();
        }

        _isAttacking = false;
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