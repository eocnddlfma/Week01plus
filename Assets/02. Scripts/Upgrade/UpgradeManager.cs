using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업그레이드 시스템 매니저.
/// 웨이브 클리어 시 3개의 업그레이드 중 하나를 선택하는 시스템을 관리합니다.
///
/// === 사용법 ===
/// 1. 빈 GameObject 생성 → UpgradeManager 컴포넌트 추가
/// 2. 자식으로 Canvas 생성 → UpgradeUI 프리팹 배치
/// 3. Inspector에서 UpgradeData ScriptableObject들을 _upgradePool에 할당
/// 4. UpgradeUI 참조 연결
/// 5. 프리팹으로 저장 → 씬에 배치하면 완료!
///
/// === UpgradeData 생성 ===
/// Project 창에서 우클릭 → Create → Game → UpgradeData
///
/// === 기존 코드 변경사항 (최소) ===
/// - PlayerController: 이동속도, 대시 쿨다운에 PlayerStatModifier 적용
/// - BatWeaponManager: 차지속도, 데미지, 넉백, 공격범위에 적용
/// - OrbitalWeapon: 공 데미지, 발사속도에 적용
/// - PlayerBase: 무적시간 적용 + IncreaseMaxHp 메서드 추가
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("업그레이드 풀 (ScriptableObject 리스트)")]
    [SerializeField] private List<UpgradeData> _upgradePool;

    [Header("UI 참조")]
    [SerializeField] private UpgradeUI _upgradeUI;
    [SerializeField] private WS_BallGetter _ballGetter;

    [Header("설정")]
    [SerializeField] private int _choiceCount = 3;
    [SerializeField] private bool _showOnBossWaveOnly = false;

    private PlayerStatModifier _statModifier;
    private List<UpgradeData> _appliedUpgrades = new List<UpgradeData>();

    public List<UpgradeData> AppliedUpgrades => _appliedUpgrades;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Phase 6: WaveManager 이벤트에서 GameEvents로 변경 (WaveManager 참조 제거)
        GameEvents.OnWaveCleared += OnWaveClear;

        // 플레이어에 PlayerStatModifier 확보
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            _statModifier = player.GetComponent<PlayerStatModifier>();
            if (_statModifier == null)
                _statModifier = player.gameObject.AddComponent<PlayerStatModifier>();
        }

        if (_upgradeUI != null)
            _upgradeUI.Hide();
    }

    private void OnDestroy()
    {
        // Phase 6: WaveManager 이벤트에서 GameEvents로 변경
        GameEvents.OnWaveCleared -= OnWaveClear;

        if (Instance == this)
            Instance = null;
    }

    private void OnWaveClear(bool isBossWave)
    {
        // 보스 웨이브만 옵션이 켜져 있으면 보스 웨이브가 아닐 때 스킵
        if (_showOnBossWaveOnly && !isBossWave) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (_upgradePool == null || _upgradePool.Count == 0) return;

        ShowUpgradeSelection();
    }

    private void ShowUpgradeSelection()
    {
        Time.timeScale = 0f;

        List<UpgradeData> choices = GetRandomUpgrades(_choiceCount);

        if (_upgradeUI != null)
            _upgradeUI.Show(choices, OnUpgradeSelected);
        else
            Time.timeScale = 1f; // UI가 없으면 바로 재개
    }

    private void OnUpgradeSelected(UpgradeData selected)
    {
        ApplyUpgrade(selected);
        _appliedUpgrades.Add(selected);

        if (_upgradeUI != null)
            _upgradeUI.Hide();

        Debug.Log($"[Upgrade] 선택 완료: {selected.upgradeName}");

        // 업그레이드 선택 후 공 선택 UI 표시
        if (_ballGetter != null)
            _ballGetter.ShowBallSelection();
        else
            Time.timeScale = 1f;
    }

    private void ApplyUpgrade(UpgradeData data)
    {
        if (_statModifier == null) return;

        switch (data.statType)
        {
            case UpgradeStatType.MoveSpeed:
                _statModifier.MoveSpeedMult += data.value;
                break;
            case UpgradeStatType.BatDamage:
                _statModifier.BatDamageMult += data.value;
                break;
            case UpgradeStatType.BallDamage:
                _statModifier.BallDamageMult += data.value;
                break;
            case UpgradeStatType.Knockback:
                _statModifier.KnockbackMult += data.value;
                break;
            case UpgradeStatType.ChargeSpeed:
                _statModifier.ChargeSpeedMult += data.value;
                break;
            case UpgradeStatType.BallSpeed:
                _statModifier.BallSpeedMult += data.value;
                break;
            case UpgradeStatType.AttackRange:
                _statModifier.AttackRangeMult += data.value;
                break;
            case UpgradeStatType.DashCooldown:
                _statModifier.DashCooldownMult += data.value;
                break;
            case UpgradeStatType.Invincibility:
                _statModifier.InvincibilityMult += data.value;
                break;
            case UpgradeStatType.MaxHp:
                var playerBase = _statModifier.GetComponent<PlayerBase>();
                if (playerBase != null)
                    playerBase.IncreaseMaxHp(Mathf.RoundToInt(data.value));
                break;
            case UpgradeStatType.HpRegen:
                _statModifier.HpRegenAmount += data.value;
                break;
            case UpgradeStatType.BatAttackSpeed:
                _statModifier.BatAttackSpeedMult += data.value;
                break;
            case UpgradeStatType.BatAttackCooldown:
                _statModifier.BatAttackCooldownMult += data.value;
                break;

            // ── 공 특화 업그레이드 ──
            case UpgradeStatType.Split4Way:
                SplitOrbitalWeapon.SplitDirections = 4;
                break;
            case UpgradeStatType.Split8Way:
                SplitOrbitalWeapon.SplitDirections = 8;
                break;

            case UpgradeStatType.BombRadiusMult:
                BombOrbitalWeapon.UpgradeExplosionMult = data.value;
                break;
            case UpgradeStatType.BombAutoExplode:
                BombOrbitalWeapon.AutoExplode = true;
                break;

            case UpgradeStatType.BatSwingRadius:
                BatOrbitalWeapon.UpgradeSwingRadiusMult = data.value;
                foreach (var w in FindObjectsByType<BatOrbitalWeapon>(FindObjectsSortMode.None))
                    w.ApplySwingRadiusVisual();
                break;
            case UpgradeStatType.BatHalfHp:
                BatOrbitalWeapon.HalfHpOnHit = true;
                break;

            case UpgradeStatType.BounceNoDamp:
                BounceOrbitalWeapon.NoDamp = true;
                break;
            case UpgradeStatType.BounceHitBonus:
                BounceOrbitalWeapon.CollisionDamageBonus = true;
                break;

            case UpgradeStatType.GravityMult:
                GravityOrbitalWeapon.UpgradeStrengthMult = data.value;
                break;
            case UpgradeStatType.GravityRepel:
                GravityOrbitalWeapon.Repel = true;
                break;

            case UpgradeStatType.HeavyMaxHpDamage:
                HeavyOrbitalWeapon.MaxHpDamagePercent = data.value;
                break;
            case UpgradeStatType.HeavyCurling:
                HeavyOrbitalWeapon.Curling = true;
                break;

            case UpgradeStatType.NormalFixedDamage:
                NormalOrbitalWeapon.FixedDamage = Mathf.RoundToInt(data.value);
                break;

            case UpgradeStatType.PenFencingMaster:
                PenetrationOrbitalWeapon.UpgradeDurationMult = data.value;
                foreach (var w in FindObjectsByType<PenetrationOrbitalWeapon>(FindObjectsSortMode.None))
                    w.RefreshDurationUpgrade();
                break;
            case UpgradeStatType.PenContinuousStab:
                PenetrationOrbitalWeapon.ContinuousStab = true;
                break;

            case UpgradeStatType.SmallDamageMult:
                SmallOrbitalWeapon.DamageMult = data.value;
                break;
            case UpgradeStatType.SmallHackSlash:
                SmallOrbitalWeapon.DamageMult = data.value; // 15f
                SmallOrbitalWeapon.HackSlash = true;
                break;

            case UpgradeStatType.StraightRelaunch:
                StraightOrbitalWeapon.Relaunch = true;
                break;
            case UpgradeStatType.StraightKnockback:
                StraightOrbitalWeapon.KnockbackBonus = data.value;
                break;

            case UpgradeStatType.WallThresholdBlast:
                WallOrbitalWeapon.ThresholdBlast = true;
                break;
            case UpgradeStatType.WallThrowDetach:
                WallOrbitalWeapon.ThrowOnDetach = true;
                break;
            case UpgradeStatType.WallWideBody:
                WallOrbitalWeapon.WideBody = true;
                foreach (var w in FindObjectsByType<WallOrbitalWeapon>(FindObjectsSortMode.None))
                    w.ApplyWideBody();
                break;

            case UpgradeStatType.WhirlNoReturn:
                WhirlOrbitalWeapon.ApplyNoReturn = true;
                foreach (var w in FindObjectsByType<WhirlOrbitalWeapon>(FindObjectsSortMode.None))
                    w.SwapToNoReturnSO();
                break;
            case UpgradeStatType.WhirlCollisionStack:
                WhirlOrbitalWeapon.CollisionStack = true;
                break;

            case UpgradeStatType.UnlockChargeLevel:
                if (_statModifier != null)
                    _statModifier.MaxChargeLevel = Mathf.Min(4, _statModifier.MaxChargeLevel + 1);
                break;
        }

        Debug.Log($"[Upgrade] 적용: {data.upgradeName} ({data.statType} {(data.value >= 0 ? "+" : "")}{data.value})");
    }

    private bool IsUnlocked(UpgradeData data)
    {
        // 이미 선택된 업그레이드는 제외
        if (_appliedUpgrades.Contains(data)) return false;

        if (data.prerequisites == null || data.prerequisites.Count == 0) return true;
        foreach (var req in data.prerequisites)
            if (!_appliedUpgrades.Contains(req)) return false;
        return true;
    }

    private List<UpgradeData> GetRandomUpgrades(int count)
    {
        var availableBalls = GetAvailableBallTypes();

        List<UpgradeData> pool = new List<UpgradeData>();
        foreach (var data in _upgradePool)
            if (IsUnlocked(data) && HasRequiredBall(data.statType, availableBalls))
                pool.Add(data);

        List<UpgradeData> result = new List<UpgradeData>();
        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    private HashSet<System.Type> GetAvailableBallTypes()
    {
        var types = new HashSet<System.Type>();
        foreach (var orbital in FindObjectsByType<OrbitalWeapon>(FindObjectsSortMode.None))
            types.Add(orbital.GetType());
        return types;
    }

    private bool HasRequiredBall(UpgradeStatType statType, HashSet<System.Type> availableBalls)
    {
        switch (statType)
        {
            case UpgradeStatType.Split4Way:
            case UpgradeStatType.Split8Way:
                return availableBalls.Contains(typeof(SplitOrbitalWeapon));

            case UpgradeStatType.BombRadiusMult:
            case UpgradeStatType.BombAutoExplode:
                return availableBalls.Contains(typeof(BombOrbitalWeapon));

            case UpgradeStatType.BatSwingRadius:
            case UpgradeStatType.BatHalfHp:
                return availableBalls.Contains(typeof(BatOrbitalWeapon));

            case UpgradeStatType.BounceNoDamp:
            case UpgradeStatType.BounceHitBonus:
                return availableBalls.Contains(typeof(BounceOrbitalWeapon));

            case UpgradeStatType.GravityMult:
            case UpgradeStatType.GravityRepel:
                return availableBalls.Contains(typeof(GravityOrbitalWeapon));

            case UpgradeStatType.HeavyMaxHpDamage:
            case UpgradeStatType.HeavyCurling:
                return availableBalls.Contains(typeof(HeavyOrbitalWeapon));

            case UpgradeStatType.NormalFixedDamage:
                return availableBalls.Contains(typeof(NormalOrbitalWeapon));

            case UpgradeStatType.PenFencingMaster:
            case UpgradeStatType.PenContinuousStab:
                return availableBalls.Contains(typeof(PenetrationOrbitalWeapon));

            case UpgradeStatType.SmallDamageMult:
            case UpgradeStatType.SmallHackSlash:
                return availableBalls.Contains(typeof(SmallOrbitalWeapon));

            case UpgradeStatType.StraightRelaunch:
            case UpgradeStatType.StraightKnockback:
                return availableBalls.Contains(typeof(StraightOrbitalWeapon));

            case UpgradeStatType.WallThresholdBlast:
            case UpgradeStatType.WallThrowDetach:
                return availableBalls.Contains(typeof(WallOrbitalWeapon));

            case UpgradeStatType.WhirlNoReturn:
            case UpgradeStatType.WhirlCollisionStack:
                return availableBalls.Contains(typeof(WhirlOrbitalWeapon));

            default:
                return true; // 일반 업그레이드는 항상 허용
        }
    }
}
