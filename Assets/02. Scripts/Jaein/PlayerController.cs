using UnityEngine;
using System.Collections;

public class PlayerController : PlayerBase
{
    [Header("Data")]
    [SerializeField] private PlayerStatsData _statsData;

    [Header("References")]
    [SerializeField] private Transform _playerBody;

    [Header("Weapon System")]
    [SerializeField] private BatWeaponManager _weaponSystem;

    [Header("Camera Effect")]
    [SerializeField] private ChargeCameraEffect _cameraEffect;

    public Vector2 FacingDirection => _playerBody != null ? (Vector2)_playerBody.right : Vector2.right;

    private PlayerInputHandler _inputHandler;
    private PlayerMovement _movement;
    private PlayerDash _dash;

    protected override void Awake()
    {
        base.Awake();

        if (_playerBody == null && transform.childCount > 0)
            _playerBody = transform.GetChild(0);

        if (_weaponSystem == null)
            _weaponSystem = GetComponentInChildren<BatWeaponManager>();

        if (_cameraEffect == null)
        {
            var effect = FindAnyObjectByType<ChargeCameraEffect>();
            if (effect != null)
                _cameraEffect = effect;
        }

        // Initialize subsystems
        _inputHandler = GetComponent<PlayerInputHandler>();
        if (_inputHandler == null) _inputHandler = gameObject.AddComponent<PlayerInputHandler>();

        _movement = GetComponent<PlayerMovement>();
        if (_movement == null) _movement = gameObject.AddComponent<PlayerMovement>();

        _dash = GetComponent<PlayerDash>();
        if (_dash == null) _dash = gameObject.AddComponent<PlayerDash>();

        InitFromStatsData();
    }

    private void InitFromStatsData()
    {
        if (_statsData == null) return;

        _maxHp = _statsData.maxHp;
        _hp = _maxHp;
        _invincibleDuration = _statsData.invincibleDuration;
        _flashInterval = _statsData.flashInterval;
        _movement.SetStats(_statsData.moveSpeed, _statsData.rotationSpeed);
        _dash.SetStats(_statsData.dashSpeed, _statsData.dashDuration, _statsData.dashCooldown);
    }

    void Update()
    {
        if (_isDead) return;

        _inputHandler.HandleInput();
        _dash.HandleDash(_inputHandler.InputVec);
        _movement.RotateTowardsMouse();
        _inputHandler.HandleAttack();
    }

    void FixedUpdate()
    {
        if (_isDead || _dash.IsDashing) return;

        _movement.Move(_inputHandler.InputVec);
        _movement.ClampInsideBoundary();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IEnemyProjectile>(out _))
            Destroy(other.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (_movement == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(0, 0, 0), 24f);
    }
}
