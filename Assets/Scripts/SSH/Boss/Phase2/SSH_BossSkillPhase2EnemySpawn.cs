using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossSkillPhase2EnemySpawn : SSH_BossSkillPhase2Base
    {
        [SerializeField] private SSH_SOBossSkillPhase2EnemySpawn _so;

        IEnumerator Pattern()
        {
            if (_so?.EnemyPrefabs == null || _so.EnemyPrefabs.Length == 0)
            {
                _patternFinished = true;
                yield break;
            }

            for (int i = 0; i < _so.SpawnCount; i++)
            {
                GameObject prefab = _so.EnemyPrefabs[Random.Range(0, _so.EnemyPrefabs.Length)];
                Vector2    offset = Random.insideUnitCircle.normalized * _so.SpawnRadius;
                Vector3    pos    = transform.position + new Vector3(offset.x, offset.y, 0f);

                GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

                fbdfbd_EnemyBase enemyBase = enemy.GetComponent<fbdfbd_EnemyBase>();
                if (enemyBase != null)
                {
                    if (_boss?.Target != null) enemyBase.SetTarget(_boss.Target);
                    enemyBase.UnregisterFromEnemyCount();
                }

                yield return new WaitForSeconds(_so.SpawnDelay);
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
