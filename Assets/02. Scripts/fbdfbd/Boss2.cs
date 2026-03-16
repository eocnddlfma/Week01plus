using UnityEngine;

/// <summary>
/// 2번째 보스.
///
/// === 스킬 구성 ===
/// [0] 회전 링샷 (Boss2SkillRotatingShot) - 360도 전방위 발사, 매 Execute마다 회전
/// [1] 지뢰 스캐터 (fbdfbd_BossSkillMineShot)  - 기존 지뢰 패턴 재활용
/// [2] 돌진 + 후방 발사 (Boss2SkillSpiralDash) - 2페이즈 전용
///
/// === 페이즈 전환 ===
/// HP가 Phase2HpThreshold(기본 50%) 이하로 떨어지면 2페이즈로 전환.
/// 2페이즈: 돌진 스킬이 최우선 사용, 회전 링샷 속도도 빨라짐.
///
/// === Inspector 설정 ===
/// 1. BossBase._skillSlots 에 3개 슬롯 추가:
///    - [0] SkillData: SOBoss2SkillRotatingShot, SkillLogic: Boss2SkillRotatingShot
///    - [1] SkillData: fbdfbd_SOBossSkillMineShot,  SkillLogic: fbdfbd_BossSkillMineShot
///    - [2] SkillData: SOBoss2SkillSpiralDash,      SkillLogic: Boss2SkillSpiralDash
/// 2. StopMoveWhileCasting = false (돌진 스킬 이동이 필요하므로)
/// 3. 각 스킬 SO 설정 권장값은 아래 주석 참고
/// </summary>
public class Boss2 : BossBase
{
    [Header("Boss2 Phase")]
    [Range(0f, 1f)][SerializeField] private float _phase2HpThreshold = 0.5f;

    [Header("Boss2 Skill Indices (Inspector의 _skillSlots 순서와 맞춰주세요)")]
    [SerializeField] private int _rotatingShotIndex = 0;
    [SerializeField] private int _mineShotIndex = 1;
    [SerializeField] private int _spiralDashIndex = 2;

    private int _bossMaxHp;
    private bool _isPhase2;

    public override void SetTarget(Transform player)
    {
        base.SetTarget(player);
        // SetTarget → ApplyWaveScaling → _hp = _maxHp
        // 이 시점에 Hp == 스케일된 최대 체력
        _bossMaxHp = Hp;
    }

    protected override int PickReadySkillIndex()
    {
        // 페이즈 전환 체크
        if (!_isPhase2 && _bossMaxHp > 0 && Hp <= Mathf.RoundToInt(_bossMaxHp * _phase2HpThreshold))
        {
            _isPhase2 = true;
            OnEnterPhase2();
        }

        return _isPhase2 ? PickPhase2Skill() : PickPhase1Skill();
    }

    private void OnEnterPhase2()
    {
        Debug.Log("[Boss2] === 2페이즈 전환! ===");
    }

    /// <summary>
    /// 1페이즈: 회전 링샷과 지뢰를 6:4 비율로 사용
    /// </summary>
    private int PickPhase1Skill()
    {
        bool shotReady = IsSkillReady(_rotatingShotIndex);
        bool mineReady = IsSkillReady(_mineShotIndex);

        if (!shotReady && !mineReady) return -1;
        if (shotReady && !mineReady)  return _rotatingShotIndex;
        if (!shotReady && mineReady)  return _mineShotIndex;

        return Random.value < 0.6f ? _rotatingShotIndex : _mineShotIndex;
    }

    /// <summary>
    /// 2페이즈: 돌진 최우선 → 회전 링샷 → 지뢰 순서
    /// </summary>
    private int PickPhase2Skill()
    {
        if (IsSkillReady(_spiralDashIndex)) return _spiralDashIndex;
        if (IsSkillReady(_rotatingShotIndex)) return _rotatingShotIndex;
        if (IsSkillReady(_mineShotIndex))     return _mineShotIndex;

        return -1;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = _isPhase2 ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
