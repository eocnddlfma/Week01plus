using UnityEngine;

/// <summary>
/// 돌진 + 후방 부채꼴 발사 스킬 데이터.
/// 플레이어에게 돌진하면서 뒤쪽으로 탄환을 퍼뜨립니다.
/// </summary>
[CreateAssetMenu(fileName = "BossSkill_SpiralDash", menuName = "Scriptable Objects/BossSkill/SpiralDash")]
public class SOBoss2SkillSpiralDash : fbdfbd_SOBossSkillBase
{
    [Header("Dash")]
    [Min(0.1f)][SerializeField] private float _dashSpeed = 14f;
    [Min(0.05f)][SerializeField] private float _dashDuration = 0.45f;

    [Header("Shot During Dash")]
    [Min(1)][SerializeField] private int _projectilesPerShot = 5;
    [Min(0f)][SerializeField] private float _fanAngle = 100f;       // 발사 부채꼴 각도
    [Min(1)][SerializeField] private int _shotsCount = 4;           // 돌진 중 발사 횟수
    [Min(0f)][SerializeField] private float _shotInterval = 0.1f;   // 발사 간격

    [Header("Projectile")]
    [Min(1)][SerializeField] private int _damage = 1;
    [Min(0.1f)][SerializeField] private float _projectileSpeed = 6f;
    [Min(0.1f)][SerializeField] private float _projectileLifeTime = 2.5f;
    [SerializeField] private LayerMask _targetMask;

    public float DashSpeed => _dashSpeed;
    public float DashDuration => _dashDuration;
    public int ProjectilesPerShot => _projectilesPerShot;
    public float FanAngle => _fanAngle;
    public int ShotsCount => _shotsCount;
    public float ShotInterval => _shotInterval;
    public int Damage => _damage;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileLifeTime => _projectileLifeTime;
    public LayerMask TargetMask => _targetMask;
}
