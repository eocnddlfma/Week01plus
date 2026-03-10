using UnityEngine;
using System.Collections;

public class Jaein_PlayerBase : Jaein_ObjectBase
{
    [Header("Player Combat Settings")]
    [SerializeField] private float _invincibleDuration = 1.0f;
    private bool _isInvincible = false;

    protected override void Awake()
    {
        base.Awake();
        _hp = _maxHp;
        _isDead = false;
    }

    public override void TakeDamage(int damage)
    {
        if (_isDead || _isInvincible) return;

        _hp -= damage;
        Debug.Log($"현재 체력: {_hp}");

        if (_hp <= 0)
        {
            _hp = 0;
            _isDead = true;
            OnDeath();
        }
        else
        {
            StartCoroutine(InvincibleRoutine());
        }
    }

    protected override void OnDeath()
    {
        Debug.Log("[Player] 사망");
    }

    private IEnumerator InvincibleRoutine()
    {
        _isInvincible = true;
        // TODO: 피격 시 시각적 효과(예: 깜빡임) 추가
        yield return new WaitForSeconds(_invincibleDuration);
        _isInvincible = false;
    }
}