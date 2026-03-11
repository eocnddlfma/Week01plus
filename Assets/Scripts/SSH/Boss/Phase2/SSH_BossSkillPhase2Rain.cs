using System.Collections;
using UnityEngine;

// ssh_EnemyProjectile은 글로벌 네임스페이스 (fbdfbd/BossSkills)
namespace SSH.Boss
{
    // Phase1Rain 재사용, maxPhases로 아주 짧게 실행
    public class SSH_BossSkillPhase2Rain : SSH_BossSkillPhase2Base
    {
        [SerializeField] private SSH_SOBossSkillRain _so;
        [SerializeField] private int _maxPhases = 1;

        IEnumerator Pattern()
        {
            if (_so?.ProjectilePrefab == null || _so.Phases == null)
            {
                _patternFinished = true;
                yield break;
            }

            int count = Mathf.Min(_maxPhases, _so.Phases.Length);
            for (int p = 0; p < count; p++)
            {
                var phase = _so.Phases[p];
                for (int i = 0; i < phase.count; i++)
                {
                    float   x        = Random.Range(-_so.RangeWidth * 0.5f, _so.RangeWidth * 0.5f);
                    Vector3 spawnPos = new Vector3(x, _so.SpawnY, 0f);
                    GameObject proj  = Instantiate(_so.ProjectilePrefab, spawnPos, Quaternion.identity);
                    proj.transform.localScale = phase.scale;

                    SSH_EnemyProjectile script = proj.GetComponent<SSH_EnemyProjectile>();
                    if (script != null)
                        script.Init(_so.Damage, phase.speed, _so.TargetMask, gameObject);

                    yield return new WaitForSeconds(phase.spawnDelay);
                }
                if (p < count - 1)
                    yield return new WaitForSeconds(_so.StepDelay);
            }

            _patternFinished = true;
        }

        public override void Enter()
        {
            base.Enter();
            StartCoroutine(Pattern());
        }
    }
}
