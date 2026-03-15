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
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
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

        Time.timeScale = 1f;

        Debug.Log($"[Upgrade] 선택 완료: {selected.upgradeName}");
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
        }

        Debug.Log($"[Upgrade] 적용: {data.upgradeName} ({data.statType} {(data.value >= 0 ? "+" : "")}{data.value})");
    }

    private bool IsUnlocked(UpgradeData data)
    {
        if (data.prerequisites == null || data.prerequisites.Count == 0) return true;
        foreach (var req in data.prerequisites)
            if (!_appliedUpgrades.Contains(req)) return false;
        return true;
    }

    private List<UpgradeData> GetRandomUpgrades(int count)
    {
        List<UpgradeData> pool = new List<UpgradeData>();
        foreach (var data in _upgradePool)
            if (IsUnlocked(data)) pool.Add(data);
        List<UpgradeData> result = new List<UpgradeData>();

        count = Mathf.Min(count, pool.Count);

        // Fisher-Yates 셔플로 중복 없이 선택
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
