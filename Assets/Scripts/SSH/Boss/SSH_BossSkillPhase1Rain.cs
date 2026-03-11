using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossSkillPhase1Rain : fbdfbd_BossSkillBase
    {
        [System.Serializable]
        private class RainPhase
        {
            public int    count      = 5;
            public float  spawnDelay = 0.5f;
            public float  speed      = 5f;
            public Vector3 scale     = Vector3.one;
        }

        [SerializeField] private GameObject _projectilePrefab;

        [Header("Rain Settings")]
        [SerializeField] private float     _spawnY     = 15f;
        [SerializeField] private float     _rangeWidth = 30f;
        [SerializeField] private float     _stepDelay  = 1f;
        [SerializeField] private int       _damage     = 10;
        [SerializeField] private LayerMask _targetMask;

        [Header("Phases")]
        [SerializeField] private RainPhase[] _phases = new RainPhase[]
        {
            new RainPhase { count = 5,  spawnDelay = 0.6f, speed = 4f,  scale = new Vector3(0.5f, 0.5f, 0.5f) },
            new RainPhase { count = 7,  spawnDelay = 0.4f, speed = 7f,  scale = new Vector3(0.8f, 0.8f, 0.8f) },
           };

        [Header("Gizmos")]
        [SerializeField] private float _gizmoDropLength = 3f;

        private bool _patternFinished = false;

        IEnumerator Pattern()
        {
            foreach (RainPhase phase in _phases)
            {
                for (int i = 0; i < phase.count; i++)
                {
                    float x = Random.Range(-_rangeWidth * 0.5f, _rangeWidth * 0.5f);
                    GameObject obj = Instantiate(_projectilePrefab,
                                                 new Vector3(x, _spawnY, 0f),
                                                 Quaternion.identity);
                    obj.transform.localScale = phase.scale;

                    ssh_EnemyProjectile proj = obj.GetComponent<ssh_EnemyProjectile>();
                    if (proj != null) proj.Init(_damage, phase.speed, _targetMask, gameObject);

                    yield return new WaitForSeconds(phase.spawnDelay);
                }

                yield return new WaitForSeconds(_stepDelay);
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
            Gizmos.color = new Color(0.3f, 0.6f, 1f);
            Vector3 center = new Vector3(0f, _spawnY, 0f);
            Vector3 left   = center + Vector3.left  * _rangeWidth * 0.5f;
            Vector3 right  = center + Vector3.right * _rangeWidth * 0.5f;

            Gizmos.DrawLine(left, right);
            Gizmos.DrawLine(left,  left  + Vector3.down * _gizmoDropLength);
            Gizmos.DrawLine(right, right + Vector3.down * _gizmoDropLength);

            Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.15f);
            Gizmos.DrawCube(center + Vector3.down * _gizmoDropLength * 0.5f,
                            new Vector3(_rangeWidth, _gizmoDropLength, 0f));
        }
    }
}