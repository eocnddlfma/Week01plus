using UnityEngine;

/// <summary>
/// Weapon coordinator that delegates to three systems while maintaining prefab compatibility
/// - WeaponChargeSystem: Charge input and visuals
/// - WeaponSwingSystem: Swing animation and rotation
/// - WeaponHitProcessor: Collision and hit handling
/// </summary>
public class BatWeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class ChargeLevel
    {
        public string levelName;
        [Range(0f, 360f)] public float rotationAngle = 180f;
        public float attackPower = 1f;
        public float rotationDuration = 0.3f;
        public Color weaponColor = Color.white;
        public float weaponSizeMultiplier = 1f;
        public bool knockbackEnemies = true;
        public float knockbackForce = 5f;
        public int damageAmount = 1;
        public float hitStopDurationMult = 1f;
    }

    [Header("Data")]
    [SerializeField] private WeaponStatsData _statsData;

    [Header("Charge Levels")]
    [SerializeField] private ChargeLevel[] _chargeLevels = new ChargeLevel[4];

    [Header("Weapon References")]
    [SerializeField] private Transform _weaponTransform;
    [SerializeField] private SpriteRenderer _weaponSpriteRenderer;
    [SerializeField] private Transform _endpointTransform;

    [Header("Attack Settings")]
    [SerializeField] private LayerMask _enemyReflectLayerMask;
    [SerializeField] private AnimationCurve _rotationEasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private WeaponChargeSystem _chargeSystem;
    private WeaponSwingSystem _swingSystem;
    private WeaponHitProcessor _hitProcessor;

    private void Awake()
    {
        // Initialize subsystems
        _chargeSystem = GetComponent<WeaponChargeSystem>();
        if (_chargeSystem == null) _chargeSystem = gameObject.AddComponent<WeaponChargeSystem>();

        _swingSystem = GetComponent<WeaponSwingSystem>();
        if (_swingSystem == null) _swingSystem = gameObject.AddComponent<WeaponSwingSystem>();

        _hitProcessor = GetComponent<WeaponHitProcessor>();
        if (_hitProcessor == null) _hitProcessor = gameObject.AddComponent<WeaponHitProcessor>();

        // Weapon references 자동 할당
        if (_weaponTransform == null) _weaponTransform = GetComponentInChildren<Transform>();
        if (_weaponSpriteRenderer == null) _weaponSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_endpointTransform == null) _endpointTransform = GetComponentInChildren<Transform>();

        // Connect systems
        _chargeSystem.OnChargeReleased += OnChargeReleased;
    }

    private void OnChargeReleased(float chargePercent)
    {
        chargePercent = Mathf.Clamp01(chargePercent);
        _swingSystem.ExecuteSwing(chargePercent, 1, 1, _chargeSystem.ChargeLevels);
        _chargeSystem.ResetCharge();
    }

    public void HandleAttackInput(bool pressed, bool held, bool released)
    {
        _chargeSystem.HandleAttackInput(pressed, held, released);
    }

    public void OnWeaponTriggerEnter2D(Collider2D collision)
    {
        _hitProcessor.OnWeaponTriggerEnter2D(collision);
    }

    public void OnWeaponTriggerStay2D(Collider2D collision)
    {
        // Unused callback
    }

    public void OnWeaponTriggerExit2D(Collider2D collision)
    {
        // Unused callback
    }

    public bool IsAttacking => _hitProcessor.IsAttacking;
    public bool IsCharging => _chargeSystem.IsCharging;
    public float ChargePercent => _chargeSystem.ChargePercent;
    public int CurrentChargeLevel => _chargeSystem.CurrentChargeLevel;
}
