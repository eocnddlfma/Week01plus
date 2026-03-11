using System.Collections;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossSkillPhase1Dodgeball : fbdfbd_BossSkillBase
    {
        [SerializeField] private SSH_SOBossSkillDodgeball _so;

        [Header("Gizmos")]
        [SerializeField] private float _gizmoLineLength = 8f;

        private bool _patternFinished = false;

        IEnumerator Pattern()
        {
            GameObject pat = _so.PatternPrefab;
            float line     = _so.LineOffset;
            float step     = _so.StepDelay;

            // Step 1: 가로 라인 어택 1개
            Instantiate(pat, Vector3.zero,          Quaternion.Euler(0f, 0f, 90f));
            yield return new WaitForSeconds(step);

            // Step 2: 세로 라인 어택 좌우 2개
            Instantiate(pat, Vector3.left  * line,  Quaternion.identity);
            Instantiate(pat, Vector3.right * line,  Quaternion.identity);
            yield return new WaitForSeconds(step);

            // Step 3: 가로 라인 상하 (lineOffset) + 세로 라인 좌우 (lineOffset*2)
            Instantiate(pat, Vector3.up    * line,       Quaternion.Euler(0f, 0f, 90f));
            Instantiate(pat, Vector3.down  * line,       Quaternion.Euler(0f, 0f, 90f));
            Instantiate(pat, Vector3.left  * line * 2f,  Quaternion.identity);
            Instantiate(pat, Vector3.right * line * 2f,  Quaternion.identity);
            yield return new WaitForSeconds(step);

            // Step 4: 대각선 (0) + 가로 라인 상하 (lineOffset*2)
            Instantiate(pat, Vector3.zero,              Quaternion.Euler(0f, 0f,  45f));
            Instantiate(pat, Vector3.zero,              Quaternion.Euler(0f, 0f, -45f));
            Instantiate(pat, Vector3.up   * line * 2f,  Quaternion.Euler(0f, 0f, 90f));
            Instantiate(pat, Vector3.down * line * 2f,  Quaternion.Euler(0f, 0f, 90f));
            yield return new WaitForSeconds(step);

            // Step 5: 십자 (0) + 세로 (lineOffset) + 가로 (lineOffset*2)
            Instantiate(pat, Vector3.zero,              Quaternion.Euler(0f, 0f, 90f));
            Instantiate(pat, Vector3.zero,              Quaternion.identity);
            Instantiate(pat, Vector3.left  * line,      Quaternion.identity);
            Instantiate(pat, Vector3.right * line,      Quaternion.identity);
            Instantiate(pat, Vector3.up    * line * 2f, Quaternion.Euler(0f, 0f, 90f));
            Instantiate(pat, Vector3.down  * line * 2f, Quaternion.Euler(0f, 0f, 90f));
            yield return new WaitForSeconds(step);

            // Step 6: 원형 버스트
            float angleStep = 360f / Mathf.Max(1, _so.BurstCount);
            for (int i = 0; i < _so.BurstCount; i++)
            {
                float rad   = angleStep * i * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _so.BurstRadius;
                Instantiate(pat, pos, Quaternion.Euler(0f, 0f, angleStep * i));
            }
            yield return new WaitForSeconds(step);

            // Step 7: 회전 스윕
            float rotAngleStep = (360f * _so.RotLaps) / Mathf.Max(1, _so.RotCount);
            for (int i = 0; i < _so.RotCount; i++)
            {
                GameObject obj = Instantiate(pat, Vector3.zero, Quaternion.Euler(0f, 0f, rotAngleStep * i));
                SSH_ProjectileSpawner spawner = obj.GetComponentInChildren<SSH_ProjectileSpawner>();
                if (spawner != null)
                {
                    spawner.SetCount(_so.RotSpawnCount);
                    spawner.SetSpeed(_so.RotSpeed);
                }
                yield return new WaitForSeconds(_so.RotDelay);
            }
            yield return new WaitForSeconds(step);

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
            if (_so == null) return;

            float line = _so.LineOffset;
            Matrix4x4 prev = Gizmos.matrix;
            Vector3 lineV = new Vector3(0.4f, _gizmoLineLength, 0f);
            Vector3 lineH = new Vector3(_gizmoLineLength, 0.4f, 0f);

            // Step 1: 가로 라인
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, lineH);

            // Step 2: 세로 라인 좌우
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.left  * line, lineV);
            Gizmos.DrawWireCube(Vector3.right * line, lineV);
            Gizmos.DrawWireSphere(Vector3.zero, 0.4f);

            // Step 3: 가로 상하 (lineOffset) + 세로 좌우 (lineOffset*2)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.up    * line,      lineH);
            Gizmos.DrawWireCube(Vector3.down  * line,      lineH);
            Gizmos.DrawWireCube(Vector3.left  * line * 2f, lineV);
            Gizmos.DrawWireCube(Vector3.right * line * 2f, lineV);

            // Step 4: 대각선 (0) + 가로 상하 (lineOffset*2)
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f,  45f), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, lineV);
            Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -45f), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, lineV);
            Gizmos.matrix = prev;
            Gizmos.DrawWireCube(Vector3.up   * line * 2f, lineH);
            Gizmos.DrawWireCube(Vector3.down * line * 2f, lineH);

            // Step 5: 십자 (0) + 세로 (lineOffset) + 가로 (lineOffset*2)
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireCube(Vector3.zero,            lineH);
            Gizmos.DrawWireCube(Vector3.zero,            lineV);
            Gizmos.DrawWireCube(Vector3.left  * line,    lineV);
            Gizmos.DrawWireCube(Vector3.right * line,    lineV);
            Gizmos.DrawWireCube(Vector3.up    * line * 2f, lineH);
            Gizmos.DrawWireCube(Vector3.down  * line * 2f, lineH);

            // Step 6: 원형 버스트
            Gizmos.color = Color.magenta;
            float angleStep = 360f / Mathf.Max(1, _so.BurstCount);
            for (int i = 0; i < _so.BurstCount; i++)
            {
                float rad   = angleStep * i * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _so.BurstRadius;
                Gizmos.DrawWireSphere(pos, 0.35f);
            }
            Gizmos.DrawWireSphere(Vector3.zero, _so.BurstRadius);

            // Step 7: 회전 스윕
            Gizmos.color = Color.white;
            float rotAngleStep = (360f * _so.RotLaps) / Mathf.Max(1, _so.RotCount);
            for (int i = 0; i < _so.RotCount; i++)
            {
                float rad   = rotAngleStep * i * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
                Gizmos.DrawLine(Vector3.zero, dir * _gizmoLineLength * 0.5f);
            }
            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);

            Gizmos.matrix = prev;
        }
    }
}