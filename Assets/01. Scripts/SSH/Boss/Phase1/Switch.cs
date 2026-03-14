using UnityEngine;
using SSH.Boss;

public class SSH_Switch : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Color _offColor = Color.gray;
    [SerializeField] private Color _onColor  = Color.yellow;

    public bool IsOn { get; private set; } = false;

    private BossPhase1 _boss;
    private SpriteRenderer _spriteRenderer;

    public void SetBoss(BossPhase1 boss) { _boss = boss; }

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            _spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null) break;
        }
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsOn) return;
        if (other.GetComponent<OrbitalWeapon>() == null) return;

        IsOn = true;
        UpdateVisual();
        _boss?.OnSwitchActivated();
    }

    private void UpdateVisual()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = IsOn ? _onColor : _offColor;
    }
}
