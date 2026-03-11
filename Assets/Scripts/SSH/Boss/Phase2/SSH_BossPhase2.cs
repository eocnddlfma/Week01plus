using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossPhase2 : fbdfbd_EnemyBossBase
    {
        [Header("Orbit")]
        [SerializeField] private Vector3 _orbitCenter = Vector3.zero;
        [SerializeField] private float   _orbitRadius = 20f;
        [SerializeField] private float   _orbitSpeed  = 30f;   // 도/초
        [SerializeField] private float   _startAngle  = 90f;

        [Header("Orbit Speed Escalation")]
        [SerializeField] private float _orbitSpeedMaxMultiplier = 2f;  // 체력 0일 때 배수

        private float _currentAngle;
        private bool  _orbitActive = true;

        protected override void Awake()
        {
            base.Awake();
            _currentAngle = _startAngle;
            UpdateOrbitPosition();
            StartCoroutine(OrbitLoop());
        }

        private IEnumerator OrbitLoop()
        {
            while (true)
            {
                if (_orbitActive)
                {
                    float hpRatio       = _maxHp > 0 ? Mathf.Clamp01((float)_hp / _maxHp) : 1f;
                    float speedMult     = Mathf.Lerp(_orbitSpeedMaxMultiplier, 1f, hpRatio);
                    _currentAngle += _orbitSpeed * speedMult * Time.deltaTime;
                    if (_currentAngle >= 360f) _currentAngle -= 360f;
                    UpdateOrbitPosition();
                }
                yield return null;
            }
        }

        private void UpdateOrbitPosition()
        {
            float rad = _currentAngle * Mathf.Deg2Rad;
            transform.position = _orbitCenter + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _orbitRadius;
        }

        public void SetOrbitActive(bool active) => _orbitActive = active;

        public float CurrentAngle => _currentAngle;
        public float OrbitRadius  => _orbitRadius;

        protected override int PickReadySkillIndex()
        {
            int[] indices =
            {
                FindSkillIndexByLogic<SSH_BossSkillPhase2EnemySpawn>(),
                FindSkillIndexByLogic<SSH_BossSkillPhase2Stab>(),
                FindSkillIndexByLogic<SSH_BossSkillPhase2Sweep>(),
                FindSkillIndexByLogic<SSH_BossSkillPhase2Rain>(),
                FindSkillIndexByLogic<SSH_BossSkillPhase2Dodgeball>(),
                FindSkillIndexByLogic<SSH_BossSkillPhase2OrbitalDrop>(),
            };

            var valid = new List<int>();
            foreach (int idx in indices)
                if (IsSkillReady(idx)) valid.Add(idx);

            if (valid.Count == 0) return -1;
            return valid[Random.Range(0, valid.Count)];
        }

        protected override void OnAfterCast(int skillIndex, BossSkillSlot slot)
        {
            _orbitActive = true;
        }
    }
}
