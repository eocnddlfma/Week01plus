using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fbdfbd_EnemyBossBase : fbdfbd_EnemyBase
{
    [System.Serializable]
    /// <summary>
    /// 보스가 사용하는 스킬 슬롯
    /// </summary>
    protected class BossSkillSlot
    {
        [SerializeField] private fbdfbd_SOBossSkillBase _skillData;
        [SerializeField] private fbdfbd_BossSkillBase _skillLogic;
        [Min(1)][SerializeField] private int _weight = 1;

        public fbdfbd_SOBossSkillBase SkillData => _skillData;
        public fbdfbd_BossSkillBase SkillLogic => _skillLogic;
        public int Weight => _weight;
    }

    [Header("Boss Common")]
    [SerializeField] private bool _stopMoveWhileCasting = false;
    [SerializeField] private List<BossSkillSlot> _skillSlots = new List<BossSkillSlot>();

    private readonly List<float> _nextSkillReadyTimes = new List<float>();
    private readonly List<int> _readySkillIndexes = new List<int>();

    protected bool IsCastingSkill { get; private set; }
    protected int SkillSlotCount => _skillSlots.Count;

    protected override void Awake()
    {
        base.Awake();
        BuildSkillRuntimeState();
    }

    // 기본 FixedUpdate는 타겟 추적 및 이동처리 
    protected override void FixedUpdate()
    {
        if (_stopMoveWhileCasting && IsCastingSkill)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        base.FixedUpdate();
    }

    
    protected override bool CanAttack(float distanceToTarget)
    {
        if (IsCastingSkill || Target == null)
            return false;

        // 쿨다운 다 지난 스킬이 존재하는지 여부
        return BuildReadySkillIndexes() > 0;
    }

    protected override void DoAttack()
    {
        if (IsCastingSkill || Target == null)
            return;

        int skillIndex = PickReadySkillIndex();
        if (skillIndex < 0)
            return;

        StartCoroutine(RunSkillCoroutine(skillIndex));
    }

    // 기본 룰: 가중치로 판별. 2개중 특정 조건없이 사용하는 등에 사용
    // 보스마다 필요한 스킬 선택 로직 변경이 필요하다면 변경
    protected virtual int PickReadySkillIndex()
    {
        return PickWeightedRandomFromReady();
    }

    protected virtual void OnBeforeCast(int skillIndex, BossSkillSlot slot) { }
    protected virtual void OnAfterCast(int skillIndex, BossSkillSlot slot) { }

    protected bool IsValidSkillIndex(int skillIndex)
    {
        return skillIndex >= 0 && skillIndex < _skillSlots.Count;
    }

    protected bool IsSkillReady(int skillIndex)
    {
        EnsureRuntimeStateSize();

        if (!IsValidSkillIndex(skillIndex))
            return false;

        BossSkillSlot slot = _skillSlots[skillIndex];
        if (slot == null || slot.SkillData == null || slot.SkillLogic == null)
            return false;

        return Time.time >= _nextSkillReadyTimes[skillIndex];
    }

    protected int FindSkillIndexByName(string skillName)
    {
        if (string.IsNullOrEmpty(skillName))
            return -1;

        for (int i = 0; i < _skillSlots.Count; i++)
        {
            BossSkillSlot slot = _skillSlots[i];
            if (slot == null || slot.SkillData == null)
                continue;

            if (slot.SkillData.SkillName == skillName)
                return i;
        }

        return -1;
    }

    protected int FindSkillIndexByLogic<T>() where T : fbdfbd_BossSkillBase
    {
        for (int i = 0; i < _skillSlots.Count; i++)
        {
            BossSkillSlot slot = _skillSlots[i];
            if (slot == null || slot.SkillLogic == null)
                continue;

            if (slot.SkillLogic is T)
                return i;
        }

        return -1;
    }

    protected int FillReadySkillIndexes(List<int> output)
    {
        if (output == null)
            return 0;

        BuildReadySkillIndexes();
        output.Clear();

        for (int i = 0; i < _readySkillIndexes.Count; i++)
            output.Add(_readySkillIndexes[i]);

        return output.Count;
    }

    protected int PickWeightedRandomFromReady()
    {
        int readyCount = BuildReadySkillIndexes();
        if (readyCount <= 0)
            return -1;

        int totalWeight = 0;
        for (int i = 0; i < _readySkillIndexes.Count; i++)
        {
            int idx = _readySkillIndexes[i];
            totalWeight += Mathf.Max(1, _skillSlots[idx].Weight);
        }

        int pick = Random.Range(0, totalWeight);
        for (int i = 0; i < _readySkillIndexes.Count; i++)
        {
            int idx = _readySkillIndexes[i];
            int weight = Mathf.Max(1, _skillSlots[idx].Weight);

            if (pick < weight)
                return idx;

            pick -= weight;
        }

        return _readySkillIndexes[_readySkillIndexes.Count - 1];
    }

    private void BuildSkillRuntimeState()
    {
        _nextSkillReadyTimes.Clear();
        for (int i = 0; i < _skillSlots.Count; i++)
            _nextSkillReadyTimes.Add(0f);
    }

    private void EnsureRuntimeStateSize()
    {
        while (_nextSkillReadyTimes.Count < _skillSlots.Count)
            _nextSkillReadyTimes.Add(0f);

        if (_nextSkillReadyTimes.Count > _skillSlots.Count)
            _nextSkillReadyTimes.RemoveRange(_skillSlots.Count, _nextSkillReadyTimes.Count - _skillSlots.Count);
    }


    // 쿨타임이 다 지난 스킬이 존재하는지 여부
    private int BuildReadySkillIndexes()
    {
        EnsureRuntimeStateSize();
        _readySkillIndexes.Clear();

        int count = Mathf.Min(_skillSlots.Count, _nextSkillReadyTimes.Count);
        for (int i = 0; i < count; i++)
        {
            BossSkillSlot slot = _skillSlots[i];
            if (slot == null || slot.SkillData == null || slot.SkillLogic == null)
                continue;

            if (Time.time < _nextSkillReadyTimes[i])
                continue;

            _readySkillIndexes.Add(i);
        }

        return _readySkillIndexes.Count;
    }

    private IEnumerator RunSkillCoroutine(int skillIndex)
    {
        if (!IsValidSkillIndex(skillIndex))
            yield break;

        BossSkillSlot slot = _skillSlots[skillIndex];
        fbdfbd_SOBossSkillBase data = slot.SkillData;
        fbdfbd_BossSkillBase logic = slot.SkillLogic;

        if (data == null || logic == null)
            yield break;

        IsCastingSkill = true;
        OnBeforeCast(skillIndex, slot);

        logic.Enter();

        float castTime = Mathf.Max(0f, data.SkillCastTime);
        if (castTime > 0f)
            yield return new WaitForSeconds(castTime);

        int repeatCount = Mathf.Max(1, data.SkillRepeatCount);
        for (int i = 0; i < repeatCount; i++)
        {
            logic.Execute();
            yield return null;
        }

        logic.Exit();
        _nextSkillReadyTimes[skillIndex] = Time.time + Mathf.Max(0f, data.SkillCooldown);

        OnAfterCast(skillIndex, slot);
        IsCastingSkill = false;
    }
}
