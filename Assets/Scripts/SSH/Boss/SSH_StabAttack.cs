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

        [SerializeField] private Vector3 _mapCenter = Vector3.zero;

        private void Awake()
        {
            _collider = GetComponentInChildren<Collider2D>();
            _renderer = GetComponentInChildren<SpriteRenderer>();
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

            // 로컬 좌표로 찌르기 방향 계산 (보스와 함께 이동하도록 DOLocalMove 사용)
            Vector3 startLocalPos = transform.localPosition;

            // 월드 기준 중점 방향 → 부모 로컬 방향으로 변환
            Vector3 startWorldPos = transform.position;
            Vector3 toCenter      = (startWorldPos - _mapCenter).normalized;
            Vector3 localDir      = transform.parent != null
                                    ? transform.parent.InverseTransformDirection(toCenter)
                                    : toCenter;
            Vector3 thrustTargetLocal = startLocalPos + localDir * step.thrustDist;

            // 경고 위치 및 회전: 월드 끝점
            Vector3    warnPos   = startWorldPos + toCenter * step.thrustDist;
            float      warnAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg - 90f;
            Quaternion warnRot   = Quaternion.Euler(0f, 0f, warnAngle);

            GameObject warning = _warningPrefab != null
                                 ? Instantiate(_warningPrefab, warnPos, warnRot)
                                 : null;
            SpriteRenderer warnSr = warning?.GetComponentInChildren<SpriteRenderer>();

            // 경고 깜빡임
            float elapsed = 0f;
            while (elapsed < _warningDuration)
            {
                bool bright = Mathf.FloorToInt(elapsed / _blinkHalf) % 2 == 0;
                if (warnSr != null) warnSr.enabled = bright;
                SetColor(bright ? new Color(1f, 0.3f, 0.3f, 0.8f)
                                : new Color(1f, 0.3f, 0.3f, 0.1f));
                yield return new WaitForSeconds(_blinkHalf);
                elapsed += _blinkHalf;
            }

            if (warning != null) Destroy(warning);
            SetColor(Color.red);

            // 중점 방향으로 회전
            float targetAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg - 90f;
            yield return transform.DORotate(new Vector3(0f, 0f, targetAngle), 0.1f)
                                  .SetEase(Ease.OutCubic)
                                  .SetLink(gameObject).WaitForCompletion();
            if (this == null) yield break;

            SetColliderEnabled(true);

            if (!step.swing)
            {
                // 찌르기만 (N회, 로컬 방향)
                for (int i = 0; i < step.thrustCount; i++)
                {
                    yield return transform.DOLocalMove(thrustTargetLocal, step.thrustDuration)
                                          .SetEase(Ease.OutCubic)
                                          .SetLink(gameObject).WaitForCompletion();
                    if (this == null) yield break;
                    yield return transform.DOLocalMove(startLocalPos, step.thrustReturn)
                                          .SetEase(Ease.InCubic)
                                          .SetLink(gameObject).WaitForCompletion();
                    if (this == null) yield break;
                }
            }
            else
            {
                // 로컬 방향으로 찌른 뒤 그 자리에서 휘두르고 복귀
                yield return transform.DOLocalMove(thrustTargetLocal, step.thrustDuration)
                                      .SetEase(Ease.OutCubic)
                                      .SetLink(gameObject).WaitForCompletion();
                if (this == null) yield break;

                yield return transform.DOLocalRotate(new Vector3(0f, 0f, step.swingAngle),
                                                     step.swingDuration,
                                                     RotateMode.LocalAxisAdd)
                                      .SetEase(Ease.InOutSine)
                                      .SetLink(gameObject).WaitForCompletion();
                if (this == null) yield break;

                yield return transform.DOLocalMove(startLocalPos, step.thrustReturn)
                                      .SetEase(Ease.InCubic)
                                      .SetLink(gameObject).WaitForCompletion();
                if (this == null) yield break;
            }

            SetColliderEnabled(false);
            onComplete?.Invoke();
        }

        private void SetColliderEnabled(bool state)
        {
            if (_collider) _collider.enabled = state;
        }

        private void SetColor(Color c)
        {
            if (_renderer) _renderer.color = c;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;
            other.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}
