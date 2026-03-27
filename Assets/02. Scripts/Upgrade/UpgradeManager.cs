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
    [SerializeField] private int _selectCount = 2;
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
            _upgradeUI.Show(choices, OnUpgradeSelected, OnAllUpgradesSelected, _selectCount);
        else
            OnAllUpgradesSelected();
    }

    private void OnUpgradeSelected(UpgradeData selected)
    {
        ApplyUpgrade(selected);
        _appliedUpgrades.Add(selected);
        Debug.Log($"[Upgrade] 선택 완료: {selected.upgradeName}");
    }

    private void OnAllUpgradesSelected()
    {
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
                Debug.Log($"[Upgrade] 이동속도 x{_statModifier.MoveSpeedMult}");
                break;
            case UpgradeStatType.BatDamage:
                _statModifier.BatDamageMult += data.value;
                Debug.Log($"[Upgrade] 배트 데미지 x{_statModifier.BatDamageMult}");
                break;
            case UpgradeStatType.BallDamage:
                _statModifier.BallDamageMult += data.value;
                Debug.Log($"[Upgrade] 공 데미지 x{_statModifier.BallDamageMult}");
                break;
            case UpgradeStatType.Knockback:
                _statModifier.KnockbackMult += data.value;
                Debug.Log($"[Upgrade] 넉백 x{_statModifier.KnockbackMult}");
                break;
            case UpgradeStatType.ChargeSpeed:
                _statModifier.ChargeSpeedMult += data.value;
                Debug.Log($"[Upgrade] 차지 속도 x{_statModifier.ChargeSpeedMult}");
                break;
            case UpgradeStatType.BallSpeed:
                _statModifier.BallSpeedMult += data.value;
                Debug.Log($"[Upgrade] 공 속도 x{_statModifier.BallSpeedMult}");
                break;
            case UpgradeStatType.AttackRange:
                _statModifier.AttackRangeMult += data.value;
                Debug.Log($"[Upgrade] 공격 범위 x{_statModifier.AttackRangeMult}");
                break;
            case UpgradeStatType.DashCooldown:
                _statModifier.DashCooldownMult += data.value;
                Debug.Log($"[Upgrade] 대시 쿨다운 x{_statModifier.DashCooldownMult}");
                break;
            case UpgradeStatType.Invincibility:
                _statModifier.InvincibilityMult += data.value;
                Debug.Log($"[Upgrade] 무적 시간 x{_statModifier.InvincibilityMult}");
                break;
            case UpgradeStatType.MaxHp:
                var playerBase = _statModifier.GetComponent<PlayerBase>();
                if (playerBase != null)
                    playerBase.IncreaseMaxHp(Mathf.RoundToInt(data.value));
                Debug.Log($"[Upgrade] 최대 HP +{(int)data.value}");
                break;
            case UpgradeStatType.HpRegen:
                _statModifier.HpRegenAmount += data.value;
                Debug.Log($"[Upgrade] HP 회복량 {_statModifier.HpRegenAmount}");
                break;
            case UpgradeStatType.BatAttackSpeed:
                _statModifier.BatAttackSpeedMult += data.value;
                Debug.Log($"[Upgrade] 배트 공격속도 x{_statModifier.BatAttackSpeedMult}");
                break;
            case UpgradeStatType.BatAttackCooldown:
                _statModifier.BatAttackCooldownMult += data.value;
                Debug.Log($"[Upgrade] 배트 공격 쿨다운 x{_statModifier.BatAttackCooldownMult}");
                break;

            // ── 공 특화 업그레이드 ──
            case UpgradeStatType.Split4Way:
                SplitOrbitalWeapon.SplitDirections = 4;
                Debug.Log("[Upgrade] 스플릿 4방향 발동");
                break;
            case UpgradeStatType.Split8Way:
                SplitOrbitalWeapon.SplitDirections = 8;
                Debug.Log("[Upgrade] 스플릿 8방향 발동");
                break;

            case UpgradeStatType.BombRadiusMult:
                BombOrbitalWeapon.UpgradeExplosionMult = data.value;
                Debug.Log($"[Upgrade] 폭탄 폭발 범위 x{data.value}");
                break;
            case UpgradeStatType.BombAutoExplode:
                BombOrbitalWeapon.AutoExplode = true;
                Debug.Log("[Upgrade] 폭탄 자동 폭발 발동");
                break;

            case UpgradeStatType.BatSwingRadius:
                BatOrbitalWeapon.UpgradeSwingRadiusMult = data.value;
                foreach (var w in FindObjectsByType<BatOrbitalWeapon>(FindObjectsSortMode.None))
                    w.ApplySwingRadiusVisual();
                Debug.Log($"[Upgrade] 배트공 스윙 범위 x{data.value}");
                break;
            case UpgradeStatType.BatHalfHp:
                BatOrbitalWeapon.HalfHpOnHit = true;
                Debug.Log("[Upgrade] 배트공 반피 발동");
                break;

            case UpgradeStatType.BounceNoDamp:
                BounceOrbitalWeapon.NoDamp = true;
                Debug.Log("[Upgrade] 바운스 감속 없음 발동");
                break;
            case UpgradeStatType.BounceHitBonus:
                BounceOrbitalWeapon.CollisionDamageBonus = true;
                Debug.Log("[Upgrade] 바운스 충돌 데미지 보너스 발동");
                break;

            case UpgradeStatType.GravityMult:
                GravityOrbitalWeapon.UpgradeStrengthMult = data.value;
                Debug.Log($"[Upgrade] 중력공 인력 x{data.value}");
                break;
            case UpgradeStatType.GravityRepel:
                GravityOrbitalWeapon.Repel = true;
                Debug.Log("[Upgrade] 중력공 척력 발동");
                break;

            case UpgradeStatType.HeavyMaxHpDamage:
                HeavyOrbitalWeapon.MaxHpDamagePercent = data.value;
                Debug.Log($"[Upgrade] 헤비공 최대HP 비례 데미지 {data.value * 100f}%");
                break;
            case UpgradeStatType.HeavyCurling:
                HeavyOrbitalWeapon.Curling = true;
                Debug.Log("[Upgrade] 헤비공 컬링 발동");
                break;

            case UpgradeStatType.NormalFixedDamage:
                NormalOrbitalWeapon.FixedDamage = Mathf.RoundToInt(data.value);
                Debug.Log($"[Upgrade] 일반공 고정 데미지 {NormalOrbitalWeapon.FixedDamage}");
                break;

            case UpgradeStatType.PenFencingMaster:
                PenetrationOrbitalWeapon.UpgradeDurationMult = data.value;
                foreach (var w in FindObjectsByType<PenetrationOrbitalWeapon>(FindObjectsSortMode.None))
                    w.RefreshDurationUpgrade();
                Debug.Log($"[Upgrade] 관통공 지속시간 x{data.value}");
                break;
            case UpgradeStatType.PenContinuousStab:
                PenetrationOrbitalWeapon.ContinuousStab = true;
                Debug.Log("[Upgrade] 관통공 연속 찌르기 발동");
                break;

            case UpgradeStatType.SmallDamageMult:
                SmallOrbitalWeapon.DamageMult = data.value;
                Debug.Log($"[Upgrade] 작은공 데미지 x{data.value}");
                break;
            case UpgradeStatType.SmallHackSlash:
                SmallOrbitalWeapon.DamageMult = data.value;
                SmallOrbitalWeapon.HackSlash = true;
                Debug.Log($"[Upgrade] 작은공 난도질 발동 (데미지 x{data.value})");
                break;

            case UpgradeStatType.StraightRelaunch:
                StraightOrbitalWeapon.Relaunch = true;
                Debug.Log("[Upgrade] 직선공 재발사 발동");
                break;
            case UpgradeStatType.StraightKnockback:
                StraightOrbitalWeapon.KnockbackBonus = data.value;
                Debug.Log($"[Upgrade] 직선공 넉백 보너스 +{data.value}");
                break;

            case UpgradeStatType.WallThresholdBlast:
                WallOrbitalWeapon.ThresholdBlast = true;
                Debug.Log("[Upgrade] 벽공 임계 폭발 발동");
                break;
            case UpgradeStatType.WallThrowDetach:
                WallOrbitalWeapon.ThrowOnDetach = true;
                Debug.Log("[Upgrade] 벽공 분리 시 투척 발동");
                break;
            case UpgradeStatType.WallWideBody:
                WallOrbitalWeapon.WideBody = true;
                foreach (var w in FindObjectsByType<WallOrbitalWeapon>(FindObjectsSortMode.None))
                    w.ApplyWideBody();
                Debug.Log("[Upgrade] 판때기 발동 (가로 x4)");
                break;

            case UpgradeStatType.WhirlNoReturn:
                WhirlOrbitalWeapon.ApplyNoReturn = true;
                foreach (var w in FindObjectsByType<WhirlOrbitalWeapon>(FindObjectsSortMode.None))
                    w.SwapToNoReturnSO();
                Debug.Log("[Upgrade] 회오리공 귀환 없음 발동");
                break;
            case UpgradeStatType.WhirlCollisionStack:
                WhirlOrbitalWeapon.CollisionStack = true;
                Debug.Log("[Upgrade] 회오리공 충돌 스택 발동");
                break;

            case UpgradeStatType.UnlockChargeLevel:
                if (_statModifier != null)
                    _statModifier.MaxChargeLevel = Mathf.Min(4, _statModifier.MaxChargeLevel + 1);
                Debug.Log($"[Upgrade] 차지 레벨 해금 → 최대 {_statModifier.MaxChargeLevel}단계");
                break;
        }
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
        return WS_BallGetter.OwnedBallTypes;
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
            case UpgradeStatType.WallWideBody:
                return availableBalls.Contains(typeof(WallOrbitalWeapon));

            case UpgradeStatType.WhirlNoReturn:
            case UpgradeStatType.WhirlCollisionStack:
                return availableBalls.Contains(typeof(WhirlOrbitalWeapon));

            default:
                return true; // 일반 업그레이드는 항상 허용
        }
    }
}
