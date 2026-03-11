using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_Rain", menuName = "Scriptable Objects/BossSkill/Rain")]
public class SSH_SOBossSkillRain : fbdfbd_SOBossSkillBase
{
    [System.Serializable]
    public class RainPhase
    {
        public int     count      = 5;
        public float   spawnDelay = 0.5f;
        public float   speed      = 5f;
        public Vector3 scale      = Vector3.one;
    }

    [Header("Prefab")]
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

    public GameObject  ProjectilePrefab => _projectilePrefab;
    public float       SpawnY           => _spawnY;
    public float       RangeWidth       => _rangeWidth;
    public float       StepDelay        => _stepDelay;
    public int         Damage           => _damage;
    public LayerMask   TargetMask       => _targetMask;
    public RainPhase[] Phases           => _phases;
}
