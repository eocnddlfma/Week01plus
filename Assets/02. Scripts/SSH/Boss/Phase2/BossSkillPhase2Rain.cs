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
            if (_so?.Phases == null)
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
                    // Phase 7: 풀에서 투사체 획득
                    if (EnemyProjectilePool.Instance == null) yield break;

                    float   x        = Random.Range(-_so.RangeWidth * 0.5f, _so.RangeWidth * 0.5f);
                    Vector3 spawnPos = new Vector3(x, _so.SpawnY, 0f);
                    EnemyProjectile script = EnemyProjectilePool.Instance.Get();
                    if (script != null)
                    {
                        script.transform.position = spawnPos;
                        script.transform.rotation = Quaternion.identity;
                        script.transform.localScale = phase.scale;
                        script.Init(_so.Damage, new Vector2(0, -1), phase.speed, 10f, _so.TargetMask, gameObject);
                    }

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
