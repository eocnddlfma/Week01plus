using UnityEngine;
using System.Collections;

public class fbdfbd_BossSkillShot : fbdfbd_BossSkillBase
{
    [Header("Skill Data")]
    [SerializeField] private fbdfbd_SOBossSkillShot _data;

    [Header("Runtime Reference")]
    [SerializeField] private Transform _firePoint;

    private fbdfbd_EnemyBase _owner;
    private int _shotStep;
    private float _castStartTime;

    private void Awake()
    {
        _owner = GetComponent<fbdfbd_EnemyBase>();
        if (_firePoint == null)
            _firePoint = transform;
    }

    public override void Enter()
    {
        _shotStep = 0;
    }

    public override void Execute()
    {
        if (_owner == null || _data == null || _data.ProjectilePrefab == null)
            return;

        int waveIndex = _shotStep;
        _shotStep++;

        if (waveIndex == 0)
            _castStartTime = Time.time;

        float interval = Mathf.Max(0f, _data.WaveInterval);
        if (interval <= 0f)
        {
            FireWave(waveIndex);
            return;
        }

        float targetTime = _castStartTime + (waveIndex * interval);
        StartCoroutine(FireWaveAtTime(waveIndex, targetTime));
    }

    public override void Exit()
    {
        _shotStep = 0;
    }

    private IEnumerator FireWaveAtTime(int waveIndex, float targetTime)
    {
        float remain = targetTime - Time.time;
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        FireWave(waveIndex);
    }

    private void FireWave(int waveIndex)
    {
        Vector2 origin = _firePoint != null ? (Vector2)_firePoint.position : (Vector2)transform.position;
        Vector2 baseDir = GetAimDirection(origin);
        float centerOffset = GetCenterAngleOffset(waveIndex);
        int count = Mathf.Max(1, _data.ProjectilesPerExecute);

        for (int i = 0; i < count; i++)
        {
            float fanOffset = GetFanAngleOffset(i, count, _data.FanAngle);
            Vector2 finalDir = Rotate(baseDir, centerOffset + fanOffset);

            fbdfbd_EnemyProjectile projectile = Instantiate(_data.ProjectilePrefab, origin, Quaternion.identity);
            projectile.Init(
                _data.Damage,
                finalDir,
                _data.ProjectileSpeed,
                _data.ProjectileLifeTime,
                _data.TargetMask,
                gameObject);
        }
    }

    private Vector2 GetAimDirection(Vector2 origin)
    {
        if (_owner.Target != null)
        {
            Vector2 toTarget = (Vector2)_owner.Target.position - origin;
            if (toTarget.sqrMagnitude > 0.0001f)
                return toTarget.normalized;
        }

        return Vector2.down;
    }

    private float GetCenterAngleOffset(int step)
    {
        float[] offsets = _data.AngleOffsets;
        if (offsets == null || offsets.Length == 0)
            return 0f;

        int clampedStep = Mathf.Clamp(step, 0, offsets.Length - 1);
        return offsets[clampedStep];
    }

    private static float GetFanAngleOffset(int index, int count, float fanAngle)
    {
        if (count <= 1 || fanAngle <= 0f)
            return 0f;

        float t = (float)index / (count - 1);
        return Mathf.Lerp(-fanAngle * 0.5f, fanAngle * 0.5f, t);
    }

    private static Vector2 Rotate(Vector2 vector, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        ).normalized;
    }
}
