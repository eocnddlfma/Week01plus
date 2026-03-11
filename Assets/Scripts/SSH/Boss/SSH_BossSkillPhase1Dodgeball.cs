using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossSkillPhase1Dodgeball : fbdfbd_BossSkillBase
    {
        [SerializeField] private GameObject DodgeballPattern;

        [Header("Pattern Settings")]
        [SerializeField] private float _stepDelay   = 2f;
        [SerializeField] private float _lineOffset  = 18f;
        [SerializeField] private int   _burstCount  = 8;
        [SerializeField] private float _burstRadius = 8f;
        [SerializeField] private int   _rotCount      = 30;
        [SerializeField] private float _rotDelay      = 1f;
        [SerializeField] private float _rotLaps       = 3f;
        [SerializeField] private int   _rotSpawnCount = 20;
        [SerializeField] private float _rotSpeed      = 2f;

        [Header("Gizmos")]
        [SerializeField] private float _gizmoLineLength = 8f;

        private bool _patternFinished = false;

        IEnumerator Pattern()
        {
            // Step 1: 가로 라인 어택 1개
            Instantiate(DodgeballPattern, Vector3.zero,                Quaternion.Euler(0f, 0f, 90f));
            yield return new WaitForSeconds(_stepDelay);

            // Step 2: 세로 라인 어택 좌우 2개
            Instantiate(DodgeballPattern, Vector3.left  * _lineOffset, Quaternion.identity);
            Instantiate(DodgeballPattern, Vector3.right * _lineOffset, Quaternion.identity);
            yield return new WaitForSeconds(_stepDelay);

            // Step 3: 가로 라인 상하 (lineOffset) + 세로 라인 좌우 (lineOffset*2)
            Instantiate(DodgeballPattern, Vector3.up    * _lineOffset,      Quaternion.Euler(0f, 0f, 90f));
            Instantiate(DodgeballPattern, Vector3.down  * _lineOffset,      Quaternion.Euler(0f, 0f, 90f));
            Instantiate(DodgeballPattern, Vector3.left  * _lineOffset * 2f, Quaternion.identity);
            Instantiate(DodgeballPattern, Vector3.right * _lineOffset * 2f, Quaternion.identity);
            yield return new WaitForSeconds(_stepDelay);

            // Step 4: 대각선 (0) + 가로 라인 상하 (lineOffset*2)
            Instantiate(DodgeballPattern, Vector3.zero,                      Quaternion.Euler(0f, 0f,  45f));
            Instantiate(DodgeballPattern, Vector3.zero,                      Quaternion.Euler(0f, 0f, -45f));
            Instantiate(DodgeballPattern, Vector3.up   * _lineOffset * 2f,   Quaternion.Euler(0f, 0f, 90f));
            Instantiate(DodgeballPattern, Vector3.down * _lineOffset * 2f,   Quaternion.Euler(0f, 0f, 90f));
            yield return new WaitForSeconds(_stepDelay);

            // Step 5: 십자 (0) + 세로 (lineOffset) + 가로 (lineOffset*2)
            Instantiate(DodgeballPattern, Vector3.zero,                      Quaternion.Euler(0f, 0f, 90f));
            Instantiate(DodgeballPattern, Vector3.zero,                      Quaternion.identity);
            Instantiate(DodgeballPattern, Vector3.left  * _lineOffset,       Quaternion.identity);
            Instantiate(DodgeballPattern, Vector3.right * _lineOffset,       Quaternion.identity);
            Instantiate(DodgeballPattern, Vector3.up    * _lineOffset * 2f,  Quaternion.Euler(0f, 0f, 90f));
            Instantiate(DodgeballPattern, Vector3.down  * _lineOffset * 2f,  Quaternion.Euler(0f, 0f, 90f));
            yield return new WaitForSeconds(_stepDelay);

            // Step 6: 원형 버스트
            float angleStep = 360f / Mathf.Max(1, _burstCount);
            for (int i = 0; i < _burstCount; i++)
            {
                float rad = angleStep * i * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _burstRadius;
                float angle = angleStep * i;
                Instantiate(DodgeballPattern, pos, Quaternion.Euler(0f, 0f, angle));
            }
            yield return new WaitForSeconds(_stepDelay);

            // Step 7: 회전 스윕 (샌즈전 스타일) - 중앙에서 순차 회전 소환
            float rotAngleStep = (360f * _rotLaps) / Mathf.Max(1, _rotCount);
            for (int i = 0; i < _rotCount; i++)
            {
                float angle = rotAngleStep * i;
                GameObject obj = Instantiate(DodgeballPattern, Vector3.zero, Quaternion.Euler(0f, 0f, angle));
                SSH_ProjectileSpawner spawner = obj.GetComponentInChildren<SSH_ProjectileSpawner>();
                if (spawner != null)
                {
                    spawner.SetCount(_rotSpawnCount);
                    spawner.SetSpeed(_rotSpeed);
                }
                yield return new WaitForSeconds(_rotDelay);
            }
            yield return new WaitForSeconds(_stepDelay);

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

        public override void Exit()
        {

        }

        private void OnDrawGizmosSelected()
        {
            Matrix4x4 prev = Gizmos.matrix;
            Vector3 lineV = new Vector3(0.4f, _gizmoLineLength, 0f);
            Vector3 lineH = new Vector3(_gizmoLineLength, 0.4f, 0f);

            // Step 1: 가로 라인
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, lineH);

            // Step 2: 세로 라인 좌우 + 중앙 스포너
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.left  * _lineOffset, lineV);
            Gizmos.DrawWireCube(Vector3.right * _lineOffset, lineV);
            Gizmos.DrawWireSphere(Vector3.zero, 0.4f);

            // Step 3: 가로 상하 (lineOffset) + 세로 좌우 (lineOffset*2)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.up    * _lineOffset,      lineH);
            Gizmos.DrawWireCube(Vector3.down  * _lineOffset,      lineH);
            Gizmos.DrawWireCube(Vector3.left  * _lineOffset * 2f, lineV);
            Gizmos.DrawWireCube(Vector3.right * _lineOffset * 2f, lineV);

            // Step 4: 대각선 (0) + 가로 상하 (lineOffset*2)
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f,  45f), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, lineV);
            Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -45f), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, lineV);
            Gizmos.matrix = prev;
            Gizmos.DrawWireCube(Vector3.up   * _lineOffset * 2f, lineH);
            Gizmos.DrawWireCube(Vector3.down * _lineOffset * 2f, lineH);

            // Step 5: 십자 (0) + 세로 (lineOffset) + 가로 (lineOffset*2)
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireCube(Vector3.zero,                     lineH);
            Gizmos.DrawWireCube(Vector3.zero,                     lineV);
            Gizmos.DrawWireCube(Vector3.left  * _lineOffset,      lineV);
            Gizmos.DrawWireCube(Vector3.right * _lineOffset,      lineV);
            Gizmos.DrawWireCube(Vector3.up    * _lineOffset * 2f, lineH);
            Gizmos.DrawWireCube(Vector3.down  * _lineOffset * 2f, lineH);

            // Step 6: 원형 버스트
            Gizmos.color = Color.magenta;
            float angleStep = 360f / Mathf.Max(1, _burstCount);
            for (int i = 0; i < _burstCount; i++)
            {
                float rad = angleStep * i * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _burstRadius;
                Gizmos.DrawWireSphere(pos, 0.35f);
            }
            Gizmos.DrawWireSphere(Vector3.zero, _burstRadius);

            // Step 7: 회전 스윕
            Gizmos.color = Color.white;
            float rotAngleStep = (360f * _rotLaps) / Mathf.Max(1, _rotCount);
            for (int i = 0; i < _rotCount; i++)
            {
                float rad = rotAngleStep * i * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
                Gizmos.DrawLine(Vector3.zero, dir * _gizmoLineLength * 0.5f);
            }
            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);

            Gizmos.matrix = prev;
        }
    }
}