using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_BossPhase1 : fbdfbd_EnemyBossBase
    {
        [Header("Camera")]
        [SerializeField] private float _targetCameraSize   = 35f;
        [SerializeField] private float _cameraYOffset      = 5f;
        [SerializeField] private float _cameraTweenDuration = 1.2f;

        [Header("Switches")]
        [SerializeField] private GameObject  _switchPrefab;
        [SerializeField] private Transform[] _switchSpawnPoints;

        [Header("Wall")]
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private Transform  _wallSpawnPoint;

        private GameObject   _wall;
        private SSH_Switch[] _spawnedSwitches;
        [SerializeField] private bool _dodgeballUsed = false;
        [SerializeField] private bool _rainUsed      = false;

        protected override void Awake()
        {
            base.Awake();
            SetupCamera();
            SpawnWall();
            SpawnSwitches();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            cam.DOOrthoSize(_targetCameraSize, _cameraTweenDuration).SetEase(Ease.OutCubic);
            cam.transform.DOMoveY(cam.transform.position.y + _cameraYOffset, _cameraTweenDuration).SetEase(Ease.OutCubic);
        }

        #region Switches

        private void SpawnWall()
        {
            if (_wallPrefab == null) return;
            Vector3    pos = _wallSpawnPoint != null ? _wallSpawnPoint.position : Vector3.zero;
            Quaternion rot = _wallSpawnPoint != null ? _wallSpawnPoint.rotation : Quaternion.identity;
            _wall = Instantiate(_wallPrefab, pos, rot);
        }

        private void SpawnSwitches()
        {
            if (_switchPrefab == null || _switchSpawnPoints == null) return;
            _spawnedSwitches = new SSH_Switch[_switchSpawnPoints.Length];
            for (int i = 0; i < _switchSpawnPoints.Length; i++)
            {
                if (_switchSpawnPoints[i] == null) continue;
                GameObject obj = Instantiate(_switchPrefab, _switchSpawnPoints[i].position, _switchSpawnPoints[i].rotation);
                _spawnedSwitches[i] = obj.GetComponent<SSH_Switch>();
                if (_spawnedSwitches[i] != null) _spawnedSwitches[i].SetBoss(this);
            }
        }

        public bool AllSwitchesOn
        {
            get
            {
                if (_spawnedSwitches == null || _spawnedSwitches.Length == 0) return false;
                foreach (SSH_Switch sw in _spawnedSwitches)
                    if (sw == null || !sw.IsOn) return false;
                return true;
            }
        }

        public void OnSwitchActivated()
        {
            if (!AllSwitchesOn) return;
            if (_wall != null) _wall.GetComponent<SSH_Wall>()?.Open();
        }

        public void ClearSwitches()
        {
            if (_spawnedSwitches == null) return;
            foreach (SSH_Switch sw in _spawnedSwitches)
                if (sw != null) Destroy(sw.gameObject);
        }

        #endregion

        private bool CanActivateStab() => _dodgeballUsed && _rainUsed && AllSwitchesOn;

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
