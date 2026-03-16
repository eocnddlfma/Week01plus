using UnityEngine;
using System.Collections;
using System;

public class PlayerBase : EntityBase
{
    [Header("Player Visuals")]
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] protected float _invincibleDuration = 1.0f;
    [SerializeField] protected float _flashInterval = 0.1f;

    private bool _isInvincible = false;
    private float _regenTickTimer = 0f;
    private float _regenTickInterval = 0.1f;  // 0.1초마다 회복 체크

    protected override void Awake()
    {
        base.Awake();
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _hp = _maxHp;
        _isDead = false;
    }

    private void Update()
    {
        if (_isDead) return;

        // 초당 회복
        _regenTickTimer += Time.deltaTime;
        if (_regenTickTimer >= _regenTickInterval)
        {
            _regenTickTimer = 0f;

            float regenAmount = _regenTickInterval * (PlayerStatModifier.Instance?.HpRegenAmount ?? 0f);
            if (regenAmount > 0)
            {
                Heal((int)regenAmount);
            }
        }
    }

    public override void TakeDamage(int damage, bool isCharge = false)
    {
        if (_isDead || _isInvincible) return;

        _hp = Mathf.Clamp(_hp - damage, 0, _maxHp);
        GameEvents.RaisePlayerDamaged(_hp);
        Debug.Log($"현재 체력: {_hp}");

        if (_hp == 0)
        {
            _isDead = true;
            OnDeath();
        }
        else
        {
            float dur = _invincibleDuration * (PlayerStatModifier.Instance != null ? PlayerStatModifier.Instance.InvincibilityMult : 1f);
            StartInvincible(dur, flash: true);
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
        GameManager.Instance.GameOver();
    }

    public void StartInvincible(float duration, bool flash = true)
    {
        StartCoroutine(InvincibleRoutine(duration, flash));
    }

    private IEnumerator InvincibleRoutine(float duration, bool flash)
    {
        _isInvincible = true;

        if (flash)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (_spriteRenderer != null)
                {
                    Color c = _spriteRenderer.color;
                    c.a = (c.a == 1f) ? 0.2f : 1f;
                    _spriteRenderer.color = c;
                }
                yield return new WaitForSeconds(_flashInterval);
                elapsed += _flashInterval;
            }

            if (_spriteRenderer != null)
            {
                Color finalColor = _spriteRenderer.color;
                finalColor.a = 1f;
                _spriteRenderer.color = finalColor;
            }
        }
        else
        {
            // 깜빡임 없이 시간만 대기 (예: 아이템 획득 무적 등)
            yield return new WaitForSeconds(duration);
        }

        _isInvincible = false;
    }

    public void Heal(int heal)
    {
        _hp = Math.Clamp(_hp + heal, 1, MaxHp);
    }

    /// <summary>
    /// 최대 체력 증가 (증강 시스템에서 호출)
    /// </summary>
    public void IncreaseMaxHp(int amount)
    {
        _maxHp += amount;
        _hp = Mathf.Min(_hp + amount, _maxHp);
        GameEvents.RaisePlayerDamaged(_hp);
    }
}