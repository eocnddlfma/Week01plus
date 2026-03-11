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

    public override void TakeDamage(int damage, bool isCharge = false)
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
            StartCoroutine(InvincibleRoutine(true));
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

    // PlayerController에서 대쉬 시 무적 코루틴 호출
    protected IEnumerator InvincibleRoutine(bool isHit)
    {
        _isInvincible = true;

        if (isHit)
        {
            // [피격 시] 깜빡임 효과 수행
            float elapsed = 0f;
            while (elapsed < _invincibleDuration)
            {
                if (_spriteRenderer != null)
                {
                    Color c = _spriteRenderer.color;
                    // 투명도를 0.2f와 1f 사이에서 토글
                    c.a = (c.a == 1f) ? 0.2f : 1f;
                    _spriteRenderer.color = c;
                }

                yield return new WaitForSeconds(_flashInterval);
                elapsed += _flashInterval;
            }

            // 루프 종료 후 불투명도 원상복구
            if (_spriteRenderer != null)
            {
                Color finalColor = _spriteRenderer.color;
                finalColor.a = 1f;
                _spriteRenderer.color = finalColor;
            }
        }
        else
        {
            // [피격 아님] 깜빡임 없이 시간만 대기 (예: 아이템 획득 무적 등)
            yield return new WaitForSeconds(_invincibleDuration);
        }

        _isInvincible = false;
    }
}