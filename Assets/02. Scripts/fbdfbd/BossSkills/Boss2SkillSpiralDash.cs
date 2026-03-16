using System.Collections;
using UnityEngine;

/// <summary>
/// 돌진 + 후방 부채꼴 연속 발사 스킬.
/// 플레이어 방향으로 돌진하면서 뒤쪽에 탄환을 퍼뜨립니다.
/// 2페이즈 전용 스킬로 사용됩니다.
/// </summary>
public class Boss2SkillSpiralDash : BossSkillBase
{
    [Header("Skill Data")]
    [SerializeField] private SOBoss2SkillSpiralDash _data;

    [Header("Runtime Reference")]
    [SerializeField] private Transform _firePoint;

    private EnemyBase _owner;
    private bool _isDashing;
    private static readonly WaitForFixedUpdate _waitFixed = new WaitForFixedUpdate();

    private void Awake()
    {
        _owner = GetComponent<EnemyBase>();
        if (_firePoint == null)
            _firePoint = transform;
    }

    public override void Enter() { }

    public override void Execute()
    {
        if (_isDashing || _data == null || _owner == null) return;
        StartCoroutine(DashAndFireRoutine());
    }

    public override void Exit() { }

    private IEnumerator DashAndFireRoutine()
    {
        _isDashing = true;

        Vector2 dashDir = GetDashDirection();
        float elapsed = 0f;
        float nextShotTime = 0f;
        int shotsFired = 0;
        int maxShots = Mathf.Max(1, _data.ShotsCount);

        while (elapsed < _data.DashDuration)
        {
            _owner.AddExternalVelocity(dashDir * _data.DashSpeed);

            if (shotsFired < maxShots && elapsed >= nextShotTime)
            {
                FireFan(dashDir);
                nextShotTime += _data.ShotInterval;
                shotsFired++;
            }

            elapsed += Time.fixedDeltaTime;
            yield return _waitFixed;
        }

        _isDashing = false;
    }

    private Vector2 GetDashDirection()
    {
        if (_owner.Target != null)
        {
            Vector2 toTarget = (Vector2)_owner.Target.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
                return toTarget.normalized;
        }
        return Vector2.down;
    }

    private void FireFan(Vector2 dashDir)
    {
        if (EnemyProjectilePool.Instance == null) return;

        Vector2 origin = _firePoint != null
            ? (Vector2)_firePoint.position
            : (Vector2)transform.position;

        // 돌진 반대 방향(후방) 중심으로 부채꼴 발사
        Vector2 backDir = -dashDir;
        int count = Mathf.Max(1, _data.ProjectilesPerShot);
        float halfFan = _data.FanAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float t = count <= 1 ? 0f : (float)i / (count - 1);
            float angleDeg = Mathf.Lerp(-halfFan, halfFan, t);
            Vector2 dir = Rotate(backDir, angleDeg);

            EnemyProjectile proj = EnemyProjectilePool.Instance.Get();
            if (proj == null) continue;

            proj.transform.SetPositionAndRotation(origin, Quaternion.identity);
            proj.Init(_data.Damage, dir, _data.ProjectileSpeed, _data.ProjectileLifeTime, _data.TargetMask, gameObject);
        }
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
    }
}
