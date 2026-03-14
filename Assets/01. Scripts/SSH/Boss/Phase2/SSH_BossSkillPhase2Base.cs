using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    public abstract class SSH_BossSkillPhase2Base : BossSkillBase
    {
        protected BossPhase2 _boss;
        protected bool           _patternFinished = false;

        protected virtual void Awake()
        {
            _boss = GetComponent<BossPhase2>();
        }

        public override void Enter()
        {
            _boss?.SetOrbitActive(false);
            _patternFinished = false;
        }

        public override void Execute()
        {
            if (_patternFinished) Exit();
        }

        public override void Exit()
        {
            _boss?.SetOrbitActive(true);
        }
    }
}
