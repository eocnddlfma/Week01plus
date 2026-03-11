using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    // 찌르기 한 번 (swing = false)
    public class SSH_BossSkillPhase2Stab : SSH_BossSkillPhase2Base
    {
        [SerializeField] private SSH_SOBossSkillStab _so;

        private SSH_StabAttack _stab;

        IEnumerator Pattern()
        {
            if (_so?.WeaponPrefab == null) { _patternFinished = true; yield break; }

            GameObject weaponObj = Instantiate(_so.WeaponPrefab,
                                               transform.position + _so.WeaponOffset,
                                               Quaternion.identity,
                                               transform);
            _stab = weaponObj.GetComponent<SSH_StabAttack>();
            _stab?.Init(_so.WarningDuration, _so.Damage, _so.TargetMask, _so.WarningPrefab);
            _stab?.UpdateFacing();

            SSH_SOBossSkillStab.StabStep step = new SSH_SOBossSkillStab.StabStep
                { swing = false, thrustCount = 1 };
            if (_so.Steps != null && _so.Steps.Length > 0)
                step = _so.Steps[0];

            bool done = false;
            _stab?.Attack(step, () => done = true);
            yield return new WaitUntil(() => done);

            if (_stab != null) Destroy(_stab.gameObject);
            _patternFinished = true;
        }

        public override void Enter()
        {
            base.Enter();
            StartCoroutine(Pattern());
        }

        public override void Exit()
        {
            base.Exit();
            if (_stab != null) Destroy(_stab.gameObject);
        }
    }
}
