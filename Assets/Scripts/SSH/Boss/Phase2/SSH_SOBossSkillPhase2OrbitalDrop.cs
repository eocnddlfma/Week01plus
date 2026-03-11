using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_Phase2OrbitalDrop", menuName = "Scriptable Objects/BossSkill/Phase2/OrbitalDrop")]
public class SSH_SOBossSkillPhase2OrbitalDrop : fbdfbd_SOBossSkillBase
{
    [Header("Projectile")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private int        _damage          = 15;
    [SerializeField] private int        _bossDamage      = 30;
    [SerializeField] private LayerMask  _playerMask;

    [Header("Warning")]
    [SerializeField] private float _warningDuration = 1f;
    [SerializeField] private float _skillDuration   = 3f;  // 패턴 대기 시간

    public GameObject ProjectilePrefab => _projectilePrefab;
    public int        Damage           => _damage;
    public int        BossDamage       => _bossDamage;
    public LayerMask  PlayerMask       => _playerMask;
    public float      WarningDuration  => _warningDuration;
    public float      SkillDuration    => _skillDuration;
}
