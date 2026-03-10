using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

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

        if (_pivotAnimator == null)
            _pivotAnimator = GetComponentInChildren<Animator>();

        if (_weaponCollider == null)
            _weaponCollider = GetComponentInChildren<BoxCollider2D>();

        if (_weaponTransform == null && _weaponCollider != null) _weaponTransform = _weaponCollider.transform; 

        _weaponCollider.enabled = false;
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

        // 1. 차지 시작
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= _lastAttackTime + _attackCooldown)
        {
            _isCharging = true;
            _currentChargeTimer = 0f;
            if (_pivotAnimator != null) _pivotAnimator.SetBool(_chargeBoolName, true);
        }

        // 2. 차지 중
        if (_isCharging)
        {
            _currentChargeTimer += Time.deltaTime;
            float chargePercent = Mathf.Clamp01(_currentChargeTimer / _maxChargeTime);

            // 무기 크기 실시간 조절
            if (_weaponTransform != null)
            {
                float currentScale = Mathf.Lerp(_minChargeScale, _maxChargeScale, chargePercent);
                _weaponTransform.localScale = new Vector3(currentScale, currentScale, 1f);
            }

            // 풀 차지 애니메이션 전환
            if (_currentChargeTimer >= _maxChargeTime)
            {
                if (_pivotAnimator != null) _pivotAnimator.SetBool(_fullChargeBoolName, true);
            }

            // 3. 차지 해제 (공격 발사)
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isCharging = false;
                if (_pivotAnimator != null)
                {
                    _pivotAnimator.SetBool(_chargeBoolName, false);
                    _pivotAnimator.SetBool(_fullChargeBoolName, false);
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

        // 위성 타격 파워 전달 (위성 스크립트가 있다면)
        var orbital = GetComponentInChildren<Jaein_OrbitalWeapon>();
        if (orbital != null) orbital.Launch(); // TODO: Launch() 메서드에 chargePercent 전달하여 공격력 조절

        if (_weaponCollider != null) _weaponCollider.enabled = true;

        yield return new WaitForSeconds(_attackDuration);

        if (_weaponCollider != null) _weaponCollider.enabled = false;

        // 크기 원복
        if (_weaponTransform != null) _weaponTransform.localScale = Vector3.one * _minChargeScale;

        _isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: DeathSequence 호출 및 적 충돌 로직 구현
        // if (other.CompareTag("Enemy")) { ... }
    }

    // TODO: IEnumerator DeathSequence() 구현 필요
}