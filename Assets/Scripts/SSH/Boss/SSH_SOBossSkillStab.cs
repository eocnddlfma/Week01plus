using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_Stab", menuName = "Scriptable Objects/BossSkill/Stab")]
public class SSH_SOBossSkillStab : fbdfbd_SOBossSkillBase
{
    [System.Serializable]
    public class StabStep
    {
        public bool  swing          = false;
        public int   thrustCount    = 1;
        public float thrustDist     = 3f;
        public float thrustDuration = 0.15f;
        public float thrustReturn   = 0.1f;
        public float swingAngle     = 180f;
        public float swingDuration  = 0.4f;
    }

    [Header("Weapon")]
    [SerializeField] private GameObject _weaponPrefab;
    [SerializeField] private Vector3    _weaponOffset = new Vector3(0f, -2f, 0f);
    [SerializeField] private int        _damage       = 10;
    [SerializeField] private LayerMask  _targetMask;

    [Header("Warning")]
    [SerializeField] private float      _warningDuration = 0.8f;
    [SerializeField] private GameObject _warningPrefab;

    [Header("Movement")]
    [SerializeField] private float _waitAtPoint  = 0.3f;

    [Header("Steps")]
    [SerializeField] private StabStep[] _steps = new StabStep[]
    {
        new StabStep { swing = false },
        new StabStep { swing = true, thrustCount = 2, thrustDist = 3f, swingAngle = 180f },
    };

    public GameObject  WeaponPrefab    => _weaponPrefab;
    public Vector3     WeaponOffset    => _weaponOffset;
    public int         Damage          => _damage;
    public LayerMask   TargetMask      => _targetMask;
    public float       WarningDuration => _warningDuration;
    public GameObject  WarningPrefab   => _warningPrefab;
    public float       WaitAtPoint     => _waitAtPoint;
    public StabStep[]  Steps           => _steps;
}
