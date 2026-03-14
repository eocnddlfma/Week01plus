using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossSkillPhase1Rain : BossSkillBase
    {
        [SerializeField] private SSH_SOBossSkillRain _so;

        [Header("Gizmos")]
        [SerializeField] private float _gizmoDropLength = 3f;

        private bool _patternFinished = false;

        IEnumerator Pattern()
        {
            foreach (SSH_SOBossSkillRain.RainPhase phase in _so.Phases)
            {
                for (int i = 0; i < phase.count; i++)
                {
                    // Phase 7: 풀에서 투사체 획득
                    if (EnemyProjectilePool.Instance == null) yield break;

                    float x = Random.Range(-_so.RangeWidth * 0.5f, _so.RangeWidth * 0.5f);
                    EnemyProjectile proj = EnemyProjectilePool.Instance.Get();
                    if (proj != null)
                    {
                        proj.transform.position = new Vector3(x, _so.SpawnY, 0f);
                        proj.transform.rotation = Quaternion.identity;
                        proj.transform.localScale = phase.scale;
                        proj.Init(_so.Damage, new Vector2(0, -1), phase.speed, 10f, _so.TargetMask, gameObject);
                    }

                    yield return new WaitForSeconds(phase.spawnDelay);
                }
                
                yield return new WaitForSeconds(_so.StepDelay);
            }

            _patternFinished = true;
        }

        public override void Enter()
        {
            _patternFinished = false;
            StartCoroutine(Pattern());
        }

        public override void Execute()
        {
            if (_patternFinished) Exit();
        }

        public override void Exit() { }

        private void OnDrawGizmosSelected()
        {
            if (_so == null) return;

            Gizmos.color = new Color(0.3f, 0.6f, 1f);
            Vector3 center = new Vector3(0f, _so.SpawnY, 0f);
            Vector3 left   = center + Vector3.left  * _so.RangeWidth * 0.5f;
            Vector3 right  = center + Vector3.right * _so.RangeWidth * 0.5f;

            Gizmos.DrawLine(left, right);
            Gizmos.DrawLine(left,  left  + Vector3.down * _gizmoDropLength);
            Gizmos.DrawLine(right, right + Vector3.down * _gizmoDropLength);

            Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.15f);
            Gizmos.DrawCube(center + Vector3.down * _gizmoDropLength * 0.5f,
                            new Vector3(_so.RangeWidth, _gizmoDropLength, 0f));
        }
    }
}