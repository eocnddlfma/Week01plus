using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_StabAttack : MonoBehaviour
    {
        private static readonly float _blinkHalf = 0.08f;

        private Collider2D       _collider;
        private SpriteRenderer   _renderer;
        private int              _damage;
        private LayerMask        _targetMask;
        private float            _warningDuration;
        private GameObject       _warningPrefab;
        private Transform        _bossTransform;

        [SerializeField] private Vector3 _mapCenter = Vector3.zero;

        private void Awake()
        {
            _collider      = GetComponentInChildren<Collider2D>();
            _renderer      = GetComponentInChildren<SpriteRenderer>();
            _bossTransform = transform.parent;
            if (_collider) _collider.enabled = false;
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }

        public void Init(float warningDuration, int damage, LayerMask targetMask, GameObject warningPrefab)
        {
            _warningDuration = warningDuration;
            _damage          = damage;
            _targetMask      = targetMask;
            _warningPrefab   = warningPrefab;
        }

        public void Attack(SSH_SOBossSkillStab.StabStep step, Action onComplete = null)
        {
            StartCoroutine(AttackSequence(step, onComplete));
        }

        private IEnumerator AttackSequence(SSH_SOBossSkillStab.StabStep step, Action onComplete)
        {
            transform.DOKill();
            SetColliderEnabled(false);

            // 보스 기준 중점 방향 계산
            Vector3 startBossPos = _bossTransform != null ? _bossTransform.position : transform.position;
            Vector3 toCenter     = (_mapCenter - startBossPos).normalized;
            Vector3 bossTarget   = startBossPos + toCenter * step.thrustDist;

            // 경고 위치 기준각 (무기 월드 위치 기준)
            float warnAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg + 90f;

            // 경고 생성
            GameObject[] warnings = SpawnWarnings(step, transform.position, warnAngle);

            // 경고 깜빡임
            float elapsed = 0f;
            while (elapsed < _warningDuration)
            {
                bool bright = Mathf.FloorToInt(elapsed / _blinkHalf) % 2 == 0;
                foreach (GameObject w in warnings)
                {
                    if (w == null) continue;
                    SpriteRenderer sr = w.GetComponentInChildren<SpriteRenderer>();
                    if (sr) sr.enabled = bright;
                }
                SetColor(bright ? new Color(1f, 0.3f, 0.3f, 0.8f)
                                : new Color(1f, 0.3f, 0.3f, 0.1f));
                yield return new WaitForSeconds(_blinkHalf);
                elapsed += _blinkHalf;
            }

            foreach (GameObject w in warnings)
                if (w != null) Destroy(w);
            SetColor(Color.red);

            // 무기를 중점 방향으로 회전
            float targetAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg + 90f;
            yield return transform.DORotate(new Vector3(0f, 0f, targetAngle), 0.1f)
                                  .SetEase(Ease.OutCubic)
                                  .SetLink(gameObject).WaitForCompletion();
            if (this == null) yield break;

            SetColliderEnabled(true);

            if (_bossTransform == null)
            {
                SetColliderEnabled(false);
                onComplete?.Invoke();
                yield break;
            }

            if (!step.swing)
            {
                // 보스가 전진 → 복귀 (N회), 무기는 자식으로 따라감
                for (int i = 0; i < step.thrustCount; i++)
                {
                    yield return _bossTransform.DOMove(bossTarget, step.thrustDuration)
                                               .SetEase(Ease.OutCubic)
                                               .SetLink(_bossTransform.gameObject).WaitForCompletion();
                    if (this == null) yield break;
                    yield return _bossTransform.DOMove(startBossPos, step.thrustReturn)
                                               .SetEase(Ease.InCubic)
                                               .SetLink(_bossTransform.gameObject).WaitForCompletion();
                    if (this == null) yield break;
                }
            }
            else
            {
                // 보스 전진 → 무기 회전 휘두르기 → 보스 복귀
                yield return _bossTransform.DOMove(bossTarget, step.thrustDuration)
                                           .SetEase(Ease.OutCubic)
                                           .SetLink(_bossTransform.gameObject).WaitForCompletion();
                if (this == null) yield break;

                yield return transform.DOLocalRotate(new Vector3(0f, 0f, step.swingAngle),
                                                     step.swingDuration,
                                                     RotateMode.LocalAxisAdd)
                                      .SetEase(Ease.InOutSine)
                                      .SetLink(gameObject).WaitForCompletion();
                if (this == null) yield break;

                yield return _bossTransform.DOMove(startBossPos, step.thrustReturn)
                                           .SetEase(Ease.InCubic)
                                           .SetLink(_bossTransform.gameObject).WaitForCompletion();
                if (this == null) yield break;
            }

            SetColliderEnabled(false);
            onComplete?.Invoke();
        }

        public void UpdateFacing()
        {
            Vector3 toCenter = (_mapCenter - transform.position).normalized;
            float angle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private GameObject[] SpawnWarnings(SSH_SOBossSkillStab.StabStep step,
                                            Vector3 worldOrigin, float baseAngleDeg)
        {
            if (_warningPrefab == null) return Array.Empty<GameObject>();

            if (!step.swing)
            {
                // 찌르기: 시작~끝점의 중간에 1개
                float dirRad     = (baseAngleDeg - 90f) * Mathf.Deg2Rad;
                Vector3 dir      = new Vector3(Mathf.Cos(dirRad), Mathf.Sin(dirRad), 0f);
                Vector3 midpoint = worldOrigin + dir * step.thrustDist * 0.5f;
                return new[] { Instantiate(_warningPrefab, midpoint, Quaternion.Euler(0f, 0f, baseAngleDeg)) };
            }
            else
            {
                // 스윙: 찌르기 끝점을 기준으로 부채꼴 배치
                float initDirRad = (baseAngleDeg - 90f) * Mathf.Deg2Rad;
                Vector3 initDir  = new Vector3(Mathf.Cos(initDirRad), Mathf.Sin(initDirRad), 0f);
                Vector3 endpoint = worldOrigin + initDir * step.thrustDist;

                int count = Mathf.Max(2, step.swingWarningCount);
                var list  = new GameObject[count + 1];
                for (int w = 0; w <= count; w++)
                {
                    float t      = (float)w / count;
                    float angle  = baseAngleDeg + step.swingAngle * t;
                    list[w] = Instantiate(_warningPrefab, endpoint, Quaternion.Euler(0f, 0f, angle));
                }
                return list;
            }
        }

        private void SetColliderEnabled(bool state)
        {
            if (_collider) _collider.enabled = state;
        }

        private void SetColor(Color c)
        {
            if (_renderer) _renderer.color = c;
        }

        private void OnDrawGizmos()
        {
            Vector3 toCenter = (_mapCenter - transform.position).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + toCenter * 3f);
            Gizmos.DrawWireSphere(transform.position + toCenter * 3f, 0.2f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;
            other.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}
