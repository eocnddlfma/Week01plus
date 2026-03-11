using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossPhase1 : fbdfbd_EnemyBossBase
    {
        [SerializeField] private SSH_SwitchManager _switchManager;

        [Header("Camera")]
        [SerializeField] private float _targetCameraSize = 35f;
        [SerializeField] private float _cameraYOffset    = 5f;
        [SerializeField] private float _cameraTweenDuration = 1.2f;

        [Header("Switches")]
        [SerializeField] private GameObject  _switchPrefab;
        [SerializeField] private Transform[] _switchSpawnPoints;

        private bool _dodgeballUsed = false;
        private bool _rainUsed      = false;

        protected override void Awake()
        {
            base.Awake();
            SetupCamera();
            SpawnSwitches();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            cam.DOOrthoSize(_targetCameraSize, _cameraTweenDuration).SetEase(Ease.OutCubic);
            cam.transform.DOMoveY(cam.transform.position.y + _cameraYOffset, _cameraTweenDuration).SetEase(Ease.OutCubic);
        }

        private void SpawnSwitches()
        {
            if (_switchPrefab == null || _switchSpawnPoints == null) return;

            foreach (Transform point in _switchSpawnPoints)
            {
                if (point != null)
                    Instantiate(_switchPrefab, point.position, point.rotation);
            }
        }

        private bool CanActivateStab()
        {
            bool switchesOn = _switchManager == null || _switchManager.AllOn;
            return _dodgeballUsed && _rainUsed && switchesOn;
        }

        protected override int PickReadySkillIndex()
        {
            int dodgeIdx = FindSkillIndexByLogic<SSH_BossSkillPhase1Dodgeball>();
            int rainIdx  = FindSkillIndexByLogic<SSH_BossSkillPhase1Rain>();
            int stabIdx  = FindSkillIndexByLogic<SSH_BossSkillPhase1Stab>();

            var valid = new List<int>();
            if (IsSkillReady(dodgeIdx)) valid.Add(dodgeIdx);
            if (IsSkillReady(rainIdx))  valid.Add(rainIdx);
            if (IsSkillReady(stabIdx) && CanActivateStab()) valid.Add(stabIdx);

            if (valid.Count == 0) return -1;
            return valid[Random.Range(0, valid.Count)];
        }

        protected override void OnAfterCast(int skillIndex, BossSkillSlot slot)
        {
            if (slot.SkillLogic is SSH_BossSkillPhase1Dodgeball) _dodgeballUsed = true;
            if (slot.SkillLogic is SSH_BossSkillPhase1Rain)      _rainUsed      = true;
        }
    }
}
