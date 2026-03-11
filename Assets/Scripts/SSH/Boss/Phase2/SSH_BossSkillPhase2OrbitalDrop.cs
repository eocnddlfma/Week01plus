using System.Collections;
using UnityEngine;

// SSH_BossSkillPhase2OrbitalDropProjectile은 글로벌 네임스페이스
namespace SSH.Boss
{
    public class SSH_BossSkillPhase2OrbitalDrop : SSH_BossSkillPhase2Base
    {
        [SerializeField] private SSH_SOBossSkillPhase2OrbitalDrop _so;

        IEnumerator Pattern()
        {
            if (_so?.ProjectilePrefab == null || _boss?.Target == null)
            {
                _patternFinished = true;
                yield break;
            }

            yield return new WaitForSeconds(_so.WarningDuration);

            GameObject proj = Instantiate(_so.ProjectilePrefab, transform.position, Quaternion.identity);
            SSH_BossSkillPhase2OrbitalDropProjectile script =
                proj.GetComponent<SSH_BossSkillPhase2OrbitalDropProjectile>();

            if (script != null)
                script.Init(transform, _boss.Target,
                            _so.Damage, _so.BossDamage, _so.PlayerMask);

            yield return new WaitForSeconds(_so.SkillDuration);
            _patternFinished = true;
        }

        public override void Enter()
        {
            base.Enter();
            StartCoroutine(Pattern());
        }
    }
}
