using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class fbdfbd_BossBase : fbdfbd_EnemyBase
{
    [System.Serializable]
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

    protected override void Awake()
    {
        base.Awake();
        BuildSkillRuntimeState();
    }

    protected override void FixedUpdate()
    {
        if ( _stopMoveWhileCasting)
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

    // 기본 선택 규칙
    protected virtual int PickReadySkillIndex()
    {
        int readyCount = BuildReadySkillIndexes();
        if (readyCount <= 0)
            return -1;

        // 가중치 랜덤 선택
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

    protected virtual void OnBeforeCast(BossSkillSlot slot) { }
    protected virtual void OnAfterCast(BossSkillSlot slot) { }

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
        if (skillIndex < 0 || skillIndex >= _skillSlots.Count)
            yield break;

        BossSkillSlot slot = _skillSlots[skillIndex];
        fbdfbd_SOBossSkillBase data = slot.SkillData;
        fbdfbd_BossSkillBase logic = slot.SkillLogic;

        if (data == null || logic == null)
            yield break;

        IsCastingSkill = true;
        OnBeforeCast(slot);

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

        OnAfterCast(slot);
        IsCastingSkill = false;
    }
}
