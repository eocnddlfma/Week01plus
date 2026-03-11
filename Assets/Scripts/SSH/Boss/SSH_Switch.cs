using UnityEngine;

public class SSH_Switch : MonoBehaviour
{
    [SerializeField] private SSH_SwitchManager _manager;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _offColor = Color.gray;
    [SerializeField] private Color _onColor  = Color.yellow;

    public bool IsOn { get; private set; } = false;

    private void Awake()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsOn) return;
        if (other.GetComponent<Jaein_OrbitalWeapon>() == null) return;

        IsOn = true;
        UpdateVisual();
        _manager?.OnSwitchActivated();
    }

    private void UpdateVisual()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = IsOn ? _onColor : _offColor;
    }
}
