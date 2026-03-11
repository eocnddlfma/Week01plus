using UnityEngine;
using System.Collections;
using System;

public class Jaein_PlayerBase : Jaein_ObjectBase
{
    [Header("Player Visuals")]
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] private float _invincibleDuration = 1.0f;
    [SerializeField] private float _flashInterval = 0.1f;

    [Header("Action by Woosung")]
    public Action<int> OnDamaged; //받은 데미지가 아니라, 남은 체력을 보내야함.

    private bool _isInvincible = false;

    protected override void Awake()
    {
        base.Awake();
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _hp = _maxHp;
        _isDead = false;
    }

    public override void TakeDamage(int damage)
    {
        if (_isDead || _isInvincible) return;

        _hp = Mathf.Clamp(_hp - damage, 0, _maxHp);
        //데미지를 입었을 때 체력바에게 전송
        OnDamaged.Invoke(_hp);
        Debug.Log($"현재 체력: {_hp}");

        if (_hp == 0)
        {
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

        if (Rb != null)
        {
            Rb.linearVelocity = Vector2.zero;
            Rb.angularVelocity = 0f;
            Rb.simulated = false;
        }


        //종료 처리
        Ryeol_GameManager.Instance.GameOver();
    }

    private IEnumerator InvincibleRoutine()
    {
        _isInvincible = true;
        // Debug.Log("[Player] 무적 상태");
        // 피격 시 깜빡임 효과 (투명도 조절)
        float elapsed = 0f;
        while (elapsed < _invincibleDuration)
        {
            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = (c.a == 1f) ? 0.2f : 1f; // 켰다 껐다 반복
                _spriteRenderer.color = c;
            }
            yield return new WaitForSeconds(_flashInterval);
            elapsed += _flashInterval;
        }

        // 복구
        if (_spriteRenderer != null)
        {
            Color finalColor = _spriteRenderer.color;
            finalColor.a = 1f;
            _spriteRenderer.color = finalColor;
        }
        _isInvincible = false;
        // Debug.Log("[Player] 무적 해제");
    }
}