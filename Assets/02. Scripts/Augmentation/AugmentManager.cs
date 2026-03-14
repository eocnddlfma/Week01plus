using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 증강 시스템 매니저.
/// 웨이브 클리어 시 3개의 증강 중 하나를 선택하는 시스템을 관리합니다.
///
/// === 사용법 ===
/// 1. 빈 GameObject 생성 → AugmentManager 컴포넌트 추가
/// 2. 자식으로 Canvas 생성 → AugmentUI 프리팹 배치
/// 3. Inspector에서 AugmentData ScriptableObject들을 _augmentPool에 할당
/// 4. AugmentUI 참조 연결
/// 5. 프리팹으로 저장 → 씬에 배치하면 완료!
///
/// === AugmentData 생성 ===
/// Project 창에서 우클릭 → Create → Game → AugmentData
///
/// === 기존 코드 변경사항 (최소) ===
/// - PlayerController: 이동속도, 대시 쿨다운에 PlayerStatModifier 적용
/// - BatWeaponManager: 차지속도, 데미지, 넉백, 공격범위에 적용
/// - OrbitalWeapon: 공 데미지, 발사속도에 적용
/// - PlayerBase: 무적시간 적용 + IncreaseMaxHp 메서드 추가
/// </summary>
public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance { get; private set; }

    [Header("증강 풀 (ScriptableObject 리스트)")]
    [SerializeField] private List<AugmentData> _augmentPool;

    [Header("UI 참조")]
    [SerializeField] private AugmentUI _augmentUI;

    [Header("설정")]
    [SerializeField] private int _choiceCount = 3;
    [SerializeField] private bool _showOnBossWaveOnly = false;

    private PlayerStatModifier _statModifier;
    private WaveManager _spawner;
    private List<AugmentData> _appliedAugments = new List<AugmentData>();

    public List<AugmentData> AppliedAugments => _appliedAugments;

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
        // EnemySpawner 이벤트 구독
        _spawner = FindAnyObjectByType<WaveManager>();
        if (_spawner != null)
            _spawner.OnWaveClear += OnWaveClear;

        // 플레이어에 PlayerStatModifier 확보
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            _statModifier = player.GetComponent<PlayerStatModifier>();
            if (_statModifier == null)
                _statModifier = player.gameObject.AddComponent<PlayerStatModifier>();
        }

        if (_augmentUI != null)
            _augmentUI.Hide();
    }

    private void OnDestroy()
    {
        if (_spawner != null)
            _spawner.OnWaveClear -= OnWaveClear;

        if (Instance == this)
            Instance = null;
    }

    private void OnWaveClear(bool isBossWave)
    {
        // 보스 웨이브만 옵션이 켜져 있으면 보스 웨이브가 아닐 때 스킵
        if (_showOnBossWaveOnly && !isBossWave) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (_augmentPool == null || _augmentPool.Count == 0) return;

        ShowAugmentSelection();
    }

    private void ShowAugmentSelection()
    {
        Time.timeScale = 0f;

        List<AugmentData> choices = GetRandomAugments(_choiceCount);

        if (_augmentUI != null)
            _augmentUI.Show(choices, OnAugmentSelected);
        else
            Time.timeScale = 1f; // UI가 없으면 바로 재개
    }

    private void OnAugmentSelected(AugmentData selected)
    {
        ApplyAugment(selected);
        _appliedAugments.Add(selected);

        if (_augmentUI != null)
            _augmentUI.Hide();

        Time.timeScale = 1f;

        Debug.Log($"[Augment] 선택 완료: {selected.augmentName}");
    }

    private void ApplyAugment(AugmentData data)
    {
        if (_statModifier == null) return;

        switch (data.statType)
        {
            case AugmentStatType.MoveSpeed:
                _statModifier.MoveSpeedMult += data.value;
                break;
            case AugmentStatType.BatDamage:
                _statModifier.BatDamageMult += data.value;
                break;
            case AugmentStatType.BallDamage:
                _statModifier.BallDamageMult += data.value;
                break;
            case AugmentStatType.Knockback:
                _statModifier.KnockbackMult += data.value;
                break;
            case AugmentStatType.ChargeSpeed:
                _statModifier.ChargeSpeedMult += data.value;
                break;
            case AugmentStatType.BallSpeed:
                _statModifier.BallSpeedMult += data.value;
                break;
            case AugmentStatType.AttackRange:
                _statModifier.AttackRangeMult += data.value;
                break;
            case AugmentStatType.DashCooldown:
                _statModifier.DashCooldownMult += data.value;
                break;
            case AugmentStatType.Invincibility:
                _statModifier.InvincibilityMult += data.value;
                break;
            case AugmentStatType.MaxHp:
                var playerBase = _statModifier.GetComponent<PlayerBase>();
                if (playerBase != null)
                    playerBase.IncreaseMaxHp(Mathf.RoundToInt(data.value));
                break;
        }

        Debug.Log($"[Augment] 적용: {data.augmentName} ({data.statType} {(data.value >= 0 ? "+" : "")}{data.value})");
    }

    private List<AugmentData> GetRandomAugments(int count)
    {
        List<AugmentData> pool = new List<AugmentData>(_augmentPool);
        List<AugmentData> result = new List<AugmentData>();

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
