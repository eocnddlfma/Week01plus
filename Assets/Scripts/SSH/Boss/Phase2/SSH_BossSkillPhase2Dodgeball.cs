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

            GameObject obj     = Instantiate(_so.PatternPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, 90f));
            SSH_ProjectileSpawner spawner = obj.GetComponentInChildren<SSH_ProjectileSpawner>();
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
