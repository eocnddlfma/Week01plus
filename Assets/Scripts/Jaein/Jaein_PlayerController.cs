using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

public class Jaein_PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private Transform _playerBody; // Body와 OrbitCenter를 별개로 회전시키기 위함
    [SerializeField] private Transform _weaponTransform;

    [Header("Combat Settings")]
    [SerializeField] private float _attackCooldown = 0.75f;
    [SerializeField] private float _attackDuration = 0.65f;

    [Header("Charge Settings")]
    [SerializeField] private float _maxChargeTime = 2.0f;
    [SerializeField] private float _minChargeScale = 1.0f;
    [SerializeField] private float _maxChargeScale = 2.5f;   
    [SerializeField] private string _chargeAnimBool = "IsCharging"; 

    [Header("Animation")]
    [SerializeField] private Animator _pivotAnimator; 
    [SerializeField] private string _attackTriggerName = "Attack";
    [SerializeField] private string _chargeBoolName = "IsCharging";      
    [SerializeField] private string _fullChargeBoolName = "IsFullCharged"; 

    private Rigidbody2D _rigidBody;
    private Vector2 _inputVec;
    private Camera _mainCamera;
    private Vector3 _initialWeaponScale; // 무기의 초기 로컬 스케일 저장용

    private BoxCollider2D _weaponCollider;
    private float _lastAttackTime;
    private float _currentChargeTimer = 0f;
    private bool _isAttacking = false;
    private bool _isCharging = false;
    private bool _isAlive = true;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;

        if (_pivotAnimator == null) _pivotAnimator = GetComponentInChildren<Animator>();

        if (_weaponCollider == null) _weaponCollider = GetComponentInChildren<BoxCollider2D>();

        if (_weaponTransform == null && _weaponCollider != null) _weaponTransform = _weaponCollider.transform;

        if (_weaponTransform != null) _initialWeaponScale = _weaponTransform.localScale; // 무기의 초기 스케일

        if (_weaponCollider != null) _weaponCollider.enabled = false;

        _isAlive = true;
    }

    void Update()
    {
        if (!_isAlive) return;

        HandleInput();

        // 공격 중이 아닐 때만 마우스 방향을 바라봄
        if (!_isAttacking)
        {
            RotateTowardsMouse();
        }

        HandleAttack();
    }

    void FixedUpdate()
    {
        if (!_isAlive) return;

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

    private void Move()
    {
        if (_rigidBody != null)
        {
            float speed = _isCharging ? _moveSpeed * 0.5f : _moveSpeed; // Charging 중에는 이동 속도 감소
            //_rigidBody.linearVelocity = _inputVec * _moveSpeed;
            _rigidBody.linearVelocity = _inputVec * speed;
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

        // 1. 차지 시작 판정 (마우스를 누르는 순간)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 쿨타임 체크
            if (Time.time >= _lastAttackTime + _attackCooldown)
            {
                _isCharging = true;
                _currentChargeTimer = 0f;

                // 애니메이션 파라미터 즉시 설정
                if (_pivotAnimator != null)
                {
                    _pivotAnimator.SetBool(_chargeBoolName, true);
                    _pivotAnimator.SetBool(_fullChargeBoolName, false);
                }
            }
        }

        // 2. 차지 중 로직 (마우스를 누르고 있는 동안 매 프레임 실행)
        if (_isCharging)
        {
            // 타이머 증가
            _currentChargeTimer += Time.deltaTime;
            float chargePercent = Mathf.Clamp01(_currentChargeTimer / _maxChargeTime);

            // [핵심] 무기 크기 실시간 조절과 애니메이션 동기화
            if (_weaponTransform != null)
            {
                float multiplier = Mathf.Lerp(_minChargeScale, _maxChargeScale, chargePercent);
                _weaponTransform.localScale = _initialWeaponScale * multiplier;
            }

            // 풀 차지 애니메이션 전환 체크
            if (_currentChargeTimer >= _maxChargeTime)
            {
                if (_pivotAnimator != null && !_pivotAnimator.GetBool(_fullChargeBoolName))
                {
                    _pivotAnimator.SetBool(_fullChargeBoolName, true);
                }
            }

            // 3. 차지 해제 및 공격 (마우스를 떼는 순간)
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isCharging = false;

                if (_pivotAnimator != null)
                {
                    _pivotAnimator.SetBool(_chargeBoolName, false);
                    _pivotAnimator.SetBool(_fullChargeBoolName, false);
                }

                // 현재까지 쌓인 chargePercent를 넘기며 공격 실행
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

        // TODO: Launch() 메서드에 chargePercent 전달하여 공격력 조절
        var orbital = GetComponentInChildren<Jaein_OrbitalWeapon>();
        if (orbital != null) orbital.Launch(); 

        if (_weaponCollider != null) _weaponCollider.enabled = true;

        yield return new WaitForSeconds(_attackDuration);

        if (_weaponCollider != null) _weaponCollider.enabled = false;

        if (_weaponTransform != null)
        {
            _weaponTransform.localScale = _initialWeaponScale * _minChargeScale;
        }

        _isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: DeathSequence 호출 및 적 충돌 로직 구현
        // if (other.CompareTag("Enemy")) { ... }
    }

    // TODO: IEnumerator DeathSequence() 구현 필요
}