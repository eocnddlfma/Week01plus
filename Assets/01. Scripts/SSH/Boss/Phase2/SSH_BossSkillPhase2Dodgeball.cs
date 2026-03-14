using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    // 단일 패턴 하나만 생성
    public class SSH_BossSkillPhase2Dodgeball : SSH_BossSkillPhase2Base
    {
        [SerializeField] private SSH_SOBossSkillDodgeball _so;

        IEnumerator Pattern()
        {
            if (_so?.PatternPrefab == null) { _patternFinished = true; yield break; }

            float randomZ = Random.Range(0f, 90f);  // 90° ~ 180° = 기존 90°에서 반시계 0~90° 추가
            GameObject obj     = Instantiate(_so.PatternPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, randomZ));
            ProjectileSpawner spawner = obj.GetComponentInChildren<ProjectileSpawner>();
            if (spawner != null)
            {
                spawner.SetWarningDuration(_so.WarningDuration);
                spawner.SetBlinkInterval(_so.BlinkInterval);
            }

            yield return new WaitForSeconds(_so.StepDelay);
            _patternFinished = true;
        }

        public override void Enter()
        {
            base.Enter();
            StartCoroutine(Pattern());
        }
    }
}
