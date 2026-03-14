using UnityEngine;

/// <summary>
/// Manages weapon charge input and visual feedback
/// Responsible for: charge timing, charge level calculation, visual updates
/// </summary>
public class WeaponChargeSystem : MonoBehaviour
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
    [SerializeField] private Transform _pivotParent;

    [Header("Charge Settings")]
    [SerializeField] private float _chargeCooldown = 0.3f;
    [SerializeField] private float _maxChargeTime = 1.2f;
    [SerializeField] private float _chargeThreshold = 0.2f;

    private float _currentChargeTimer = 0f;
    private int _currentChargeLevel = 0;
    private bool _isCharging = false;
    private float _chargePercent = 0f;
    private float _lastAttackTime = -100f;

    private Vector3 _initialWeaponScale;
    private Color _initialWeaponColor;
    private Vector3 _initialPivotScale;
    private Quaternion _initialPivotRotation;

    public event System.Action<float> OnChargeReleased;

    private void Awake()
    {
        if (_weaponTransform == null) _weaponTransform = transform;
        if (_weaponSpriteRenderer == null) _weaponSpriteRenderer = GetComponent<SpriteRenderer>();

        _initialWeaponScale = _weaponTransform.localScale;
        if (_weaponSpriteRenderer != null)
            _initialWeaponColor = _weaponSpriteRenderer.color;

        Transform pivot = transform.parent;
        if (pivot != null)
        {
            _initialPivotScale = pivot.localScale;
            _initialPivotRotation = pivot.localRotation;
            _pivotParent = pivot.parent;
        }

        SetupLevels();
        InitFromStatsData();
    }

    private void InitFromStatsData()
    {
        if (_statsData == null) return;
        _chargeCooldown = _statsData.chargeCooldown;
        _maxChargeTime = _statsData.maxChargeTime;
        _chargeThreshold = _statsData.chargeThreshold;
    }

    private void SetupLevels()
    {
        if (_chargeLevels == null || _chargeLevels.Length == 0)
            _chargeLevels = new ChargeLevel[4];

        if (_statsData != null && _statsData.chargeLevels != null && _statsData.chargeLevels.Length >= 4)
        {
            for (int i = 0; i < 4; i++)
            {
                WeaponStatsData.ChargeLevelData soData = _statsData.chargeLevels[i];
                _chargeLevels[i] = new ChargeLevel
                {
                    levelName = soData.levelName,
                    rotationAngle = soData.rotationAngle,
                    attackPower = soData.attackPower,
                    rotationDuration = soData.rotationDuration,
                    weaponColor = soData.weaponColor,
                    weaponSizeMultiplier = soData.weaponSizeMultiplier,
                    knockbackForce = soData.knockbackForce,
                    damageAmount = soData.damageAmount,
                    hitStopDurationMult = soData.hitStopDurationMult
                };
            }
            return;
        }

        _chargeLevels[0] = new ChargeLevel
        {
            levelName = "Basic Swing",
            rotationAngle = 180f,
            attackPower = 1f,
            rotationDuration = 0.3f,
            weaponColor = Color.white,
            weaponSizeMultiplier = 1f,
            knockbackForce = 3f,
            damageAmount = 1,
            hitStopDurationMult = 1f
        };

        _chargeLevels[1] = new ChargeLevel
        {
            levelName = "Strong Swing",
            rotationAngle = 210f,
            attackPower = 1.5f,
            rotationDuration = 0.2f,
            weaponColor = new Color(1f, 0.8f, 0.5f),
            weaponSizeMultiplier = 1.1f,
            knockbackForce = 5f,
            damageAmount = 2,
            hitStopDurationMult = 1.2f
        };

        _chargeLevels[2] = new ChargeLevel
        {
            levelName = "Powerful Swing",
            rotationAngle = 250f,
            attackPower = 2f,
            rotationDuration = 0.15f,
            weaponColor = new Color(1f, 0.5f, 0.2f),
            weaponSizeMultiplier = 1.2f,
            knockbackForce = 7f,
            damageAmount = 3,
            hitStopDurationMult = 1.5f
        };

        _chargeLevels[3] = new ChargeLevel
        {
            levelName = "Maximum Swing",
            rotationAngle = 300f,
            attackPower = 3f,
            rotationDuration = 0.1f,
            weaponColor = Color.red,
            weaponSizeMultiplier = 1.3f,
            knockbackForce = 10f,
            damageAmount = 4,
            hitStopDurationMult = 2f
        };
    }

    public void HandleAttackInput(bool pressed, bool held, bool released)
    {
        if (pressed && !_isCharging && Time.time >= _lastAttackTime + _chargeCooldown)
        {
            _currentChargeTimer = 0f;
            _isCharging = false;
            _currentChargeLevel = 0;
        }

        if (held && !_isCharging)
        {
            float chargeMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.ChargeSpeedMult : 1f;
            _currentChargeTimer += Time.deltaTime * chargeMult;
            if (!_isCharging && _currentChargeTimer > _chargeThreshold)
                _isCharging = true;

            if (_isCharging)
                UpdateChargeVisuals();
        }

        if (released && !_isCharging)
        {
            ReleaseCharge();
        }
    }

    private void UpdateChargeVisuals()
    {
        float chargeTime = _currentChargeTimer - _chargeThreshold;
        _chargePercent = Mathf.Clamp01(chargeTime / (_maxChargeTime - _chargeThreshold));
        _currentChargeLevel = Mathf.Min(3, Mathf.FloorToInt(_chargePercent * 4f));

        float dynamicSizeMultiplier = Mathf.Lerp(_chargeLevels[0].weaponSizeMultiplier, _chargeLevels[3].weaponSizeMultiplier, _chargePercent);
        Transform pivot = transform.parent;
        if (pivot != null)
            pivot.localScale = _initialPivotScale * dynamicSizeMultiplier;

        if (_weaponSpriteRenderer != null)
            _weaponSpriteRenderer.color = Color.Lerp(_chargeLevels[0].weaponColor, _chargeLevels[3].weaponColor, _chargePercent);

        float minAngle = _chargeLevels[0].rotationAngle;
        float maxAngle = _chargeLevels[3].rotationAngle;
        float dynamicAngle = Mathf.Lerp(minAngle, maxAngle, _chargePercent);
        pivot.localRotation = Quaternion.Euler(0f, 0f, -dynamicAngle * 0.5f);
    }

    private void ReleaseCharge()
    {
        _lastAttackTime = Time.time;
        _isCharging = false;
        OnChargeReleased?.Invoke(_chargePercent);
    }

    public void ResetCharge()
    {
        _currentChargeTimer = 0f;
        _isCharging = false;
        ResetVisuals();
    }

    private void ResetVisuals()
    {
        Transform pivot = transform.parent;
        if (pivot != null)
        {
            pivot.localScale = _initialPivotScale;
            pivot.localRotation = _initialPivotRotation;
        }

        if (_weaponSpriteRenderer != null)
            _weaponSpriteRenderer.color = _initialWeaponColor;
    }

    public bool IsCharging => _isCharging;
    public float ChargePercent => _chargePercent;
    public int CurrentChargeLevel => _currentChargeLevel;
    public ChargeLevel[] ChargeLevels => _chargeLevels;
}
