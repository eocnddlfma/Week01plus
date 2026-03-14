using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BatWeaponManager _weaponSystem;
    [SerializeField] private ChargeCameraEffect _cameraEffect;

    private Vector2 _inputVec;

    public Vector2 InputVec => _inputVec;

    public event System.Action<bool, bool, bool> OnAttackInput;

    private void Awake()
    {
        if (_weaponSystem == null)
        {
            var weapon = FindAnyObjectByType<BatWeaponManager>();
            if (weapon != null)
                _weaponSystem = weapon;
        }
        if (_cameraEffect == null)
        {
            var effect = FindAnyObjectByType<ChargeCameraEffect>();
            if (effect != null)
                _cameraEffect = effect;
        }
    }

    public void HandleInput()
    {
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

    public void HandleAttack()
    {
        if (_weaponSystem == null) return;

        bool pressed = Mouse.current.leftButton.wasPressedThisFrame;
        bool held = Mouse.current.leftButton.isPressed;
        bool released = Mouse.current.leftButton.wasReleasedThisFrame;

        _weaponSystem.HandleAttackInput(pressed, held, released);

        if (released && _cameraEffect != null)
        {
            _cameraEffect.EndCharge(_weaponSystem.ChargePercent);
        }
        else if (held && _weaponSystem.IsCharging && _cameraEffect != null)
        {
            _cameraEffect.BeginCharge();
        }

        OnAttackInput?.Invoke(pressed, held, released);
    }
}
