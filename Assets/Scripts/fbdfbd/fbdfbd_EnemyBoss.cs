using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fbdfbd_EnemyBoss : fbdfbd_EnemyBase
{
    [System.Serializable]
    private class BossSkillSlot
    {
        [SerializeField] private fbdfbd_SOBossSkillBase _skillData;
        [SerializeField] private fbdfbd_BossSkillBase _skillLogic;

        public fbdfbd_SOBossSkillBase SkillData => _skillData;
        public fbdfbd_BossSkillBase SkillLogic => _skillLogic;
    }

    [Header("Boss Skill Loop")]
    [SerializeField] private bool _autoStartSkillLoop = true;
    [SerializeField] private bool _stopMoveWhileCasting = true;

    [Tooltip("플레이어와의 거리가 이 값 이하일 때 근접 스킬 풀에서 스킬을 뽑습니다.")]
    [Min(0.1f)][SerializeField] private float _meleeSkillDistance = 2.5f;

    [Min(0f)][SerializeField] private float _skillIntervalMin = 0.5f;
    [Min(0f)][SerializeField] private float _skillIntervalMax = 1.5f;

    /// <summary>
    /// 근접 스킬 리스트
    /// 슬롯에 들어있는 스킬들이 게임 중 실행됩니다
    /// 플레이어와의 거리가 가깝다면 사용합니다
    /// </summary>
    [SerializeField] private List<BossSkillSlot> _meleeSkillSlots = new List<BossSkillSlot>();

    /// <summary>
    /// 원거리 스킬 리스트
    /// 슬롯에 들어있는 스킬들이 게임 중 실행됩니다
    /// 플레이어와의 거리가 멀다면 사용합니다
    /// </summary>
    [SerializeField] private List<BossSkillSlot> _rangedSkillSlots = new List<BossSkillSlot>();

    private readonly List<float> _nextMeleeSkillReadyTimes = new List<float>();
    private readonly List<float> _nextRangedSkillReadyTimes = new List<float>();
    private readonly List<int> _readySkillIndexes = new List<int>();
    private Coroutine _skillLoopRoutine;
    private bool _isCastingSkill;

    protected override void Awake()
    {
        base.Awake();

        BuildSkillRuntimeState();
        Debug.Log("[Boss] Awake complete.");
    }

    private void OnEnable()
    {
        if (_autoStartSkillLoop)
        {
            StartSkillLoop();
        }
    }

    private void OnDisable()
    {
        StopSkillLoop();
    }

    protected override void FixedUpdate()
    {
        /*
        if (_isCastingSkill && _stopMoveWhileCasting)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }
        */

        // target 에게 이동 로직
        base.FixedUpdate();
    }

    // 기본공격 사용안함
    protected override bool CanAttack(float distanceToTarget)
    { 
        return false;
    }
    // 기본공격 사용안함2
    protected override void DoAttack()
    {
        
    }


    public void StartSkillLoop()
    {
        if (_skillLoopRoutine != null) return;
        _skillLoopRoutine = StartCoroutine(SkillLoopCoroutine());
        Debug.Log("[Boss] 스킬 루프 시작.");
    }

    public void StopSkillLoop()
    {
        if (_skillLoopRoutine == null) return;
        StopCoroutine(_skillLoopRoutine);
        _skillLoopRoutine = null;
        _isCastingSkill = false;
        Debug.Log("[Boss] 스킬 루프 정지.");
    }

    private void BuildSkillRuntimeState()
    {
        _nextMeleeSkillReadyTimes.Clear();
        for (int i = 0; i < _meleeSkillSlots.Count; i++)
        {
            _nextMeleeSkillReadyTimes.Add(0f);
        }

        _nextRangedSkillReadyTimes.Clear();
        for (int i = 0; i < _rangedSkillSlots.Count; i++)
        {
            _nextRangedSkillReadyTimes.Add(0f);
        }
    }

    private IEnumerator SkillLoopCoroutine()
    {
        while (enabled)
        {
            if (Target == null)
            {
                yield return null;
                continue;
            }

            // 플레이어와의 거리에 따라 근접/원거리 스킬 풀 선택
            bool useMeleePool = IsTargetInMeleeRange();

            List<BossSkillSlot> selectedSlots = useMeleePool ? _meleeSkillSlots : _rangedSkillSlots;
            List<float> selectedReadyTimes = useMeleePool ? _nextMeleeSkillReadyTimes : _nextRangedSkillReadyTimes;

            // 뽑기
            int readyIndex = FindRandomReadySkillIndex(selectedSlots, selectedReadyTimes);
            if (readyIndex >= 0)
            {
                string poolName = useMeleePool ? "Melee" : "Ranged";
                yield return RunSkillCoroutine(selectedSlots, selectedReadyTimes, readyIndex, poolName);

                float interval = GetSkillInterval();
                if (interval > 0f)
                {
                    Debug.Log($"[Boss] Wait {interval:F2}s then re-check distance.");
                    yield return new WaitForSeconds(interval);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private bool IsTargetInMeleeRange()
    {
        if (Target == null) return false;

        Vector2 toTarget = (Vector2)Target.position - Rb.position;
        float dist = toTarget.magnitude;
        return dist <= _meleeSkillDistance;
    }

    private int FindRandomReadySkillIndex(List<BossSkillSlot> slots, List<float> readyTimes)
    {
        _readySkillIndexes.Clear();

        int count = Mathf.Min(slots.Count, readyTimes.Count);
        for (int i = 0; i < count; i++)
        {
            BossSkillSlot slot = slots[i];
            if (slot == null || slot.SkillData == null || slot.SkillLogic == null) continue;
            if (Time.time < readyTimes[i]) continue;
            _readySkillIndexes.Add(i);
        }

        if (_readySkillIndexes.Count == 0) return -1;

        int pick = Random.Range(0, _readySkillIndexes.Count);
        return _readySkillIndexes[pick];
    }

    private float GetSkillInterval()
    {
        float min = Mathf.Max(0f, _skillIntervalMin);
        float max = Mathf.Max(min, _skillIntervalMax);
        return Random.Range(min, max);
    }

    private IEnumerator RunSkillCoroutine(
        List<BossSkillSlot> slots,
        List<float> readyTimes,
        int skillIndex,
        string poolName)
    {
        BossSkillSlot slot = slots[skillIndex];
        fbdfbd_SOBossSkillBase data = slot.SkillData;
        fbdfbd_BossSkillBase logic = slot.SkillLogic;

        _isCastingSkill = true;

        Debug.Log($"[Boss] 스킬 시작 {poolName}): {data.SkillName} (idx={skillIndex})");
        logic.Enter();

        float castTime = Mathf.Max(0f, data.SkillCastTime);
        if (castTime > 0f)
        {
            yield return new WaitForSeconds(castTime);
        }

        int repeatCount = Mathf.Max(1, data.SkillRepeatCount);
        for (int i = 0; i < repeatCount; i++)
        {
            logic.Execute();
            Debug.Log($"[Boss] 스킬 실행: {data.SkillName} ({i + 1}/{repeatCount})");
            yield return null;
        }

        logic.Exit();
        Debug.Log($"[Boss] 스킬 끝: {data.SkillName}");

        readyTimes[skillIndex] = Time.time + Mathf.Max(0f, data.SkillCooldown);
        _isCastingSkill = false;
    }
}
