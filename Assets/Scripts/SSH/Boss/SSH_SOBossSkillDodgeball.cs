using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_Dodgeball", menuName = "Scriptable Objects/BossSkill/Dodgeball")]
public class SSH_SOBossSkillDodgeball : fbdfbd_SOBossSkillBase
{
    [Header("Prefab")]
    [SerializeField] private GameObject _patternPrefab;

    [Header("Pattern")]
    [SerializeField] private float _stepDelay   = 2f;
    [SerializeField] private float _lineOffset  = 18f;

    [Header("Burst")]
    [SerializeField] private int   _burstCount  = 8;
    [SerializeField] private float _burstRadius = 8f;

    [Header("Rotation Sweep")]
    [SerializeField] private int   _rotCount      = 30;
    [SerializeField] private float _rotDelay      = 1f;
    [SerializeField] private float _rotLaps       = 3f;
    [SerializeField] private int   _rotSpawnCount = 20;
    [SerializeField] private float _rotSpeed      = 2f;

    public GameObject PatternPrefab => _patternPrefab;
    public float StepDelay         => _stepDelay;
    public float LineOffset        => _lineOffset;
    public int   BurstCount        => _burstCount;
    public float BurstRadius       => _burstRadius;
    public int   RotCount          => _rotCount;
    public float RotDelay          => _rotDelay;
    public float RotLaps           => _rotLaps;
    public int   RotSpawnCount     => _rotSpawnCount;
    public float RotSpeed          => _rotSpeed;
}
