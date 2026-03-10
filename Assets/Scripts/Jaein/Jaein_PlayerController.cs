using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Jaein_PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Combat Settings")]
    [SerializeField] private float _attackCooldown = 0.75f;
    [SerializeField] private float _attackDuration = 0.65f;

    [Header("Animation")]
    [SerializeField] private Animator _pivotAnimator; // Inspector에서 Pivot 오브젝트를 드래그 앤 드롭
    [SerializeField] private string _attackTriggerName = "Attack";

    private Rigidbody2D _rigidBody;
    private Vector2 _inputVec;
    private Camera _mainCamera;

    private float _lastAttackTime;
    private bool _isAttacking = false;
    private bool _isAlive = true;

    // TODO: 애니메이터 및 무기 콜라이더 관련 필드 추가 필요

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;

        if (_pivotAnimator == null)
            _pivotAnimator = GetComponentInChildren<Animator>();

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
        // New Input System: Keyboard 직접 참조 방식
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
            _rigidBody.linearVelocity = _inputVec * _moveSpeed;
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleAttack()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!_isAttacking && Time.time >= _lastAttackTime + _attackCooldown)
            {
                // TODO: 공격 코루틴 구현
                _lastAttackTime = Time.time;
                Debug.Log("Attack!");
                StartCoroutine(AttackRoutine());
            }
        }
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;

        // 애니메이터 트리거 작동
        if (_pivotAnimator != null)
        {
            // 연타 시 트리거가 쌓이지 않도록 리셋 후 세팅
            _pivotAnimator.ResetTrigger(_attackTriggerName);
            _pivotAnimator.SetTrigger(_attackTriggerName);
        }

        // TODO: 무기 콜라이더 활성화 (필요 시)
        // _weaponCollider.enabled = true;

        // 애니메이션이 휘둘러지는 시간 동안 대기
        yield return new WaitForSeconds(_attackDuration);

        // TODO: 무기 콜라이더 비활성화
        // _weaponCollider.enabled = false;

        _isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: DeathSequence 호출 및 적 충돌 로직 구현
        // if (other.CompareTag("Enemy")) { ... }
    }

    // TODO: IEnumerator DeathSequence() 구현 필요
}