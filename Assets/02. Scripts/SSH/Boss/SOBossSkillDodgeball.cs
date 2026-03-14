using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_Dodgeball", menuName = "Scriptable Objects/BossSkill/Dodgeball")]
public class SSH_SOBossSkillDodgeball : fbdfbd_SOBossSkillBase
{
    [Header("Prefab")]
    [SerializeField] private GameObject _patternPrefab;

    [Header("Pattern")]
    [SerializeField] private float _stepDelay       = 1.7f;
    [SerializeField] private float _lineOffset      = 9f;
    [SerializeField] private float _warningDuration = 2f;
    [SerializeField] private float _blinkInterval   = 0.2f;

    [Header("Burst")]
    [SerializeField] private int   _burstCount  = 16;
    [SerializeField] private float _burstRadius = 14f;

    [Header("Rotation Sweep")]
    [SerializeField] private int   _rotCount      = 45;
    [SerializeField] private float _rotDelay      = 0.25f;
    [SerializeField] private float _rotLaps       = 2.2f;
    [SerializeField] private int   _rotSpawnCount = 25;
    [SerializeField] private float _rotSpeed      = 150f;

    public GameObject PatternPrefab    => _patternPrefab;
    public float StepDelay            => _stepDelay;
    public float LineOffset           => _lineOffset;
    public float WarningDuration      => _warningDuration;
    public float BlinkInterval        => _blinkInterval;
    public int   BurstCount        => _burstCount;
    public float BurstRadius       => _burstRadius;
    public int   RotCount          => _rotCount;
    public float RotDelay          => _rotDelay;
    public float RotLaps           => _rotLaps;
    public int   RotSpawnCount     => _rotSpawnCount;
    public float RotSpeed          => _rotSpeed;
}
