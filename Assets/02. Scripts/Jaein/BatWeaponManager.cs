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

    private void OnChargeReleased(float chargePercent)
    {
        chargePercent = Mathf.Clamp01(chargePercent);
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
