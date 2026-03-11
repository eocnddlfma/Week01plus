using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossSkillPhase1Stab : fbdfbd_BossSkillBase
    {
        [SerializeField] private SSH_SOBossSkillStab _so;
        [SerializeField] private Transform[]         _movePoints;

        private SSH_BossPhase1 _boss;
        private bool           _patternFinished = false;
        private Vector3[]      _cachedPositions;

        private void Awake()
        {
            _boss = GetComponent<SSH_BossPhase1>();

            if (_movePoints != null)
            {
                _cachedPositions = new Vector3[_movePoints.Length];
                for (int i = 0; i < _movePoints.Length; i++)
                    _cachedPositions[i] = _movePoints[i] != null ? _movePoints[i].position : Vector3.zero;
            }
        }

        IEnumerator Pattern()
        {
            // 무기를 보스 자식으로 생성 (보스와 함께 이동)
            SSH_StabAttack stab = null;
            if (_so?.WeaponPrefab == null)
            {
                Debug.LogError("[Stab] WeaponPrefab이 SO에 할당되지 않았습니다.");
            }
            else
            {
                GameObject weaponObj = Instantiate(_so.WeaponPrefab,
                                                   transform.position + _so.WeaponOffset,
                                                   Quaternion.identity,
                                                   transform);
                stab = weaponObj.GetComponent<SSH_StabAttack>();
                if (stab == null)
                    Debug.LogError($"[Stab] WeaponPrefab '{_so.WeaponPrefab.name}'에 SSH_StabAttack 컴포넌트가 없습니다.");
                else
                    stab.Init(_so.WarningDuration, _so.Damage, _so.TargetMask, _so.WarningPrefab);
            }

            SSH_SOBossSkillStab.StabStep[] steps = _so.Steps;
            var defaultStep = new SSH_SOBossSkillStab.StabStep { swing = false, thrustCount = 1 };

            int pointCount = _cachedPositions != null ? _cachedPositions.Length : 0;
            for (int i = 0; i < pointCount; i++)
            {
                // 보스 순간이동 후 무기 방향 갱신
                transform.position = _cachedPositions[i];
                stab?.UpdateFacing();

                // 매 이동 후 무조건 찌르기 (스텝에 swing=true면 휘두르기도 포함)
                if (stab != null)
                {
                    SSH_SOBossSkillStab.StabStep step = (steps != null && i < steps.Length)
                                                        ? steps[i]
                                                        : defaultStep;
                    bool done = false;
                    stab.Attack(step, () => done = true);
                    yield return new WaitUntil(() => done);
                }

                yield return new WaitForSeconds(_so.WaitAtPoint);
            }

            if (stab != null) Destroy(stab.gameObject);
            _patternFinished = true;
        }

        public override void Enter()
        {
            _boss?.ClearSwitches();
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
            if (_movePoints == null) return;

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            for (int i = 0; i < _movePoints.Length; i++)
            {
                if (_movePoints[i] == null) continue;
                Gizmos.DrawWireSphere(_movePoints[i].position, 0.5f);
                if (i > 0 && _movePoints[i - 1] != null)
                    Gizmos.DrawLine(_movePoints[i - 1].position, _movePoints[i].position);
            }

            if (_so != null)
                Gizmos.DrawWireSphere(transform.position + _so.WeaponOffset, 0.35f);
        }
    }
}
