using UnityEngine;

/// <summary>
/// 회오리 공
/// 업그레이드: 가출(_noReturnSO로 SO 교체), 몰아치기(CollisionStack)
/// Inspector에서 _noReturnSO에 noReturn=true인 SO를 연결해두세요.
/// </summary>
public class WhirlOrbitalWeapon : OrbitalWeapon
{
    [Header("가출 업그레이드용 SO")]
    [SerializeField] private OrbitalStatsData _noReturnSO;

    // ── 업그레이드 스태틱 ──
    public static bool ApplyNoReturn = false;
    public static bool CollisionStack = false;

    private int _damageStack = 0;

    protected override void Start()
    {
        base.Start();

        if (ApplyNoReturn && _noReturnSO != null)
            SwapToNoReturnSO();

        GameEvents.OnPlayerDamaged += OnPlayerHit;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEvents.OnPlayerDamaged -= OnPlayerHit;
    }

    public void SwapToNoReturnSO()
    {
        if (_noReturnSO == null) return;
        _statsData = _noReturnSO;
        InitFromStatsData();
    }

    private void OnPlayerHit(int remainingHp)
    {
        if (CollisionStack)
            _damageStack = 0;
    }

    protected override void OnTriggerEnter2D(UnityEngine.Collider2D other)
    {
        if (_state != BallState.Orbit && CollisionStack)
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
                _damageStack++;
        }
        base.OnTriggerEnter2D(other);
    }

    protected override int CalculateDamage(float chargePercent, float attackPower = 1f)
    {
        int damage = base.CalculateDamage(chargePercent, attackPower);
        if (CollisionStack)
            damage += _damageStack;
        return damage;
    }
}
