using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    // 스윕 한 번 (swing = true)
    public class SSH_BossSkillPhase2Sweep : SSH_BossSkillPhase2Base
    {
        [SerializeField] private SSH_SOBossSkillStab _so;

        private StabAttack _stab;

        IEnumerator Pattern()
        {
            if (_so?.WeaponPrefab == null) { _patternFinished = true; yield break; }

            GameObject weaponObj = Instantiate(_so.WeaponPrefab,
                                               transform.position + _so.WeaponOffset,
                                               Quaternion.identity,
                                               transform);
            _stab = weaponObj.GetComponent<StabAttack>();
            _stab?.Init(_so.WarningDuration, _so.Damage, _so.TargetMask, _so.WarningPrefab);
            _stab?.UpdateFacing();

            // swing = true 인 스텝 우선 탐색, 없으면 기본값
            SSH_SOBossSkillStab.StabStep step = new SSH_SOBossSkillStab.StabStep
                { swing = true, thrustDist = 3f, swingAngle = 180f, swingDuration = 0.4f };
            if (_so.Steps != null)
                foreach (var s in _so.Steps)
                    if (s.swing) { step = s; break; }

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
