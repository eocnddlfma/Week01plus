using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossSkillPhase1Stab : fbdfbd_BossSkillBase
    {
        private SSH_BossPhase1 _boss;

        private void Awake()
        {
            _boss = GetComponent<SSH_BossPhase1>();
        }

        public override void Enter()
        {
            _boss?.ClearSwitches();
        }

        public override void Execute() { }
        public override void Exit()    { }
    }
}
