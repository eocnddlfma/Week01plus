using UnityEngine;

/// <summary>
/// Weapon coordinator that delegates to three systems while maintaining prefab compatibility
/// - WeaponChargeSystem: Charge input and visuals
/// - WeaponSwingSystem: Swing animation and rotation
/// - WeaponHitProcessor: Collision and hit handling
/// </summary>
public class BatWeaponManager : MonoBehaviour
{
    private WeaponChargeSystem _chargeSystem;
    private WeaponSwingSystem _swingSystem;
    private WeaponHitProcessor _hitProcessor;

    private void Awake()
    {
        _chargeSystem = GetComponent<WeaponChargeSystem>();
        if (_chargeSystem == null) _chargeSystem = gameObject.AddComponent<WeaponChargeSystem>();

        _swingSystem = GetComponent<WeaponSwingSystem>();
        if (_swingSystem == null) _swingSystem = gameObject.AddComponent<WeaponSwingSystem>();

        _hitProcessor = GetComponent<WeaponHitProcessor>();
        if (_hitProcessor == null) _hitProcessor = gameObject.AddComponent<WeaponHitProcessor>();

        _chargeSystem.OnChargeReleased += OnChargeReleased;
        _swingSystem.OnSwingComplete += OnSwingComplete;
    }

    // chargePercent, batDamage, knockbackForce
    public event System.Action<float, int, float> OnSwingStarted;

    private void OnChargeReleased(float chargePercent)
    {
        chargePercent = Mathf.Clamp01(chargePercent);

        var levels = _chargeSystem.ChargeLevels;
        int maxChargeLevel = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.MaxChargeLevel : 4;
        int maxIdx = Mathf.Clamp(maxChargeLevel - 1, 0, 3);
        float batDmgMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.BatDamageMult : 1f;
        float kbMult = PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.KnockbackMult : 1f;
        int damage = Mathf.RoundToInt(Mathf.Lerp(levels[0].damageAmount, levels[maxIdx].damageAmount, chargePercent) * batDmgMult);
        float knockback = Mathf.Lerp(levels[0].knockbackForce, levels[maxIdx].knockbackForce, chargePercent) * kbMult;

        OnSwingStarted?.Invoke(chargePercent, damage, knockback);
        _swingSystem.ExecuteSwing(chargePercent, _chargeSystem.ChargeLevels);
    }

    private void OnSwingComplete()
    {
        _chargeSystem.ResetCharge();
    }

    public void HandleAttackInput(bool pressed, bool held, bool released)
    {
        _chargeSystem.HandleAttackInput(pressed, held, released);
    }

    public bool IsAttacking => _hitProcessor.IsAttacking;
    public bool IsCharging => _chargeSystem.IsCharging;
    public float ChargePercent => _chargeSystem.ChargePercent;
    public int CurrentChargeLevel => _chargeSystem.CurrentChargeLevel;
    public float OrbitalChargeSpeedBoostMax => _chargeSystem.OrbitalChargeSpeedBoostMax;
    public float OrbitalChargeClusterStrength => _chargeSystem.OrbitalChargeClusterStrength;
    public float OrbitalChargeBoostRampDuration => _chargeSystem.OrbitalChargeBoostRampDuration;
    public float ProjectileDeflectDuration => _chargeSystem.ProjectileDeflectDuration;
}
