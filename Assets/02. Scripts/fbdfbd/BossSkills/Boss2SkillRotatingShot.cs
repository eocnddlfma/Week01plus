using System.Collections;
using UnityEngine;

/// <summary>
/// 360도 회전 링샷 스킬.
/// Execute가 호출될 때마다 링이 RotationPerExecute도씩 회전합니다.
/// 반복 횟수(SkillRepeatCount)만큼 회전하면서 전 방향을 서서히 덮습니다.
/// </summary>
public class Boss2SkillRotatingShot : BossSkillBase
{
    [Header("Skill Data")]
    [SerializeField] private SOBoss2SkillRotatingShot _data;

    [Header("Runtime Reference")]
    [SerializeField] private Transform _firePoint;

    private EnemyBase _owner;
    private float _currentAngle;
    private int _executeStep;
    private float _castStartTime;

    private void Awake()
    {
        _owner = GetComponent<EnemyBase>();
        if (_firePoint == null)
            _firePoint = transform;
    }

    public override void Enter()
    {
        _currentAngle = _data != null ? _data.InitialAngleOffset : 0f;
        _executeStep = 0;
    }

    public override void Execute()
    {
        if (_data == null) return;

        int step = _executeStep;
        _executeStep++;

        if (step == 0)
            _castStartTime = Time.time;

        float interval = Mathf.Max(0f, _data.WaveInterval);
        if (interval <= 0f)
        {
            FireRingAndAdvance();
        }
        else
        {
            float targetTime = _castStartTime + step * interval;
            StartCoroutine(FireRingAtTime(targetTime));
        }
    }

    public override void Exit()
    {
        _executeStep = 0;
    }

    private IEnumerator FireRingAtTime(float targetTime)
    {
        float remain = targetTime - Time.time;
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        FireRingAndAdvance();
    }

    private void FireRingAndAdvance()
    {
        FireRing(_currentAngle);
        _currentAngle += _data.RotationPerExecute;
    }

    private void FireRing(float startAngle)
    {
        if (EnemyProjectilePool.Instance == null) return;

        Vector2 origin = _firePoint != null
            ? (Vector2)_firePoint.position
            : (Vector2)transform.position;

        int count = Mathf.Max(1, _data.ProjectilesPerRing);
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angleDeg = startAngle + i * step;
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            EnemyProjectile proj = EnemyProjectilePool.Instance.Get();
            if (proj == null) continue;

            proj.transform.SetPositionAndRotation(origin, Quaternion.identity);
            proj.Init(_data.Damage, dir, _data.ProjectileSpeed, _data.ProjectileLifeTime, _data.TargetMask, gameObject);
        }
    }
}
