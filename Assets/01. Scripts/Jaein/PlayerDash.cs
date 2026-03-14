using UnityEngine;
using System.Collections;

public class PlayerDash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerBody;
    [SerializeField] private BatWeaponManager _weaponSystem;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerMovement _movement;

    [Header("Dash Settings")]
    [SerializeField] private KeyCode _dashKey = KeyCode.Space;
    [SerializeField] private float _dashSpeed = 25f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashCooldown = 0.75f;

    private float _lastDashTime = -100f;
    private bool _isDashing = false;

    public bool IsDashing => _isDashing;

    public event System.Action<bool> OnDashStateChanged;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_movement == null) _movement = GetComponent<PlayerMovement>();
    }

    public void SetStats(float dashSpeed, float dashDuration, float dashCooldown)
    {
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
        _dashCooldown = dashCooldown;
    }

    public void HandleDash(Vector2 inputVec)
    {
        float dashCD = _dashCooldown * (PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.DashCooldownMult : 1f);
        if (!_weaponSystem.IsCharging && Input.GetKeyDown(_dashKey) && Time.time >= _lastDashTime + dashCD && !_isDashing)
        {
            Vector2 dashDir = inputVec.sqrMagnitude > 0.001f ? inputVec : (Vector2)_playerBody.right;
            StartCoroutine(DashRoutine(dashDir));
        }
    }

    private IEnumerator DashRoutine(Vector2 dir)
    {
        _isDashing = true;
        _movement.SetDashingState(true);
        _lastDashTime = Time.time;

        dir = dir.normalized;

        float originalDrag = _rb.linearDamping;
        _rb.linearDamping = 0f;

        float desiredDashDistance = _dashSpeed * _dashDuration;
        float allowedDashDistance = _movement.GetMaxDashDistance(_rb.position, dir, desiredDashDistance);

        if (allowedDashDistance <= 0.001f)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.linearDamping = originalDrag;
            _isDashing = false;
            _movement.SetDashingState(false);
            OnDashStateChanged?.Invoke(false);
            yield break;
        }

        float actualDashDuration = allowedDashDistance / _dashSpeed;
        _rb.linearVelocity = dir * _dashSpeed;

        yield return new WaitForSeconds(actualDashDuration);

        _rb.linearVelocity = Vector2.zero;
        _rb.linearDamping = originalDrag;

        _movement.ClampInsideBoundary();

        _isDashing = false;
        _movement.SetDashingState(false);
        OnDashStateChanged?.Invoke(false);
    }

}
