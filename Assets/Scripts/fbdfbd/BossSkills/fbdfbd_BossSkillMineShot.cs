using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class fbdfbd_BossSkillMineShot : fbdfbd_BossSkillBase
{
    [Header("Skill Data")]
    [SerializeField] private fbdfbd_SOBossSkillMineShot _data;

    [Header("Runtime Reference")]
    [FormerlySerializedAs("_firePos")]
    [SerializeField] private Transform _firePoint;

    private Coroutine _fireRoutine;
    private float _wobbleSeed;

    private void Awake()
    {
        if (_firePoint == null)
            _firePoint = transform;
    }

    public override void Enter()
    {
        _wobbleSeed = Random.Range(0f, 100f);
    }

    public override void Execute()
    {
        if (_data == null || _data.MineProjectilePrefab == null)
            return;

        if (_fireRoutine != null)
            StopCoroutine(_fireRoutine);

        _fireRoutine = StartCoroutine(FireRoutine());
    }

    public override void Exit()
    {
    }

    private void OnDisable()
    {
        if (_fireRoutine == null)
            return;

        StopCoroutine(_fireRoutine);
        _fireRoutine = null;
    }

    private IEnumerator FireRoutine()
    {
        int count = Mathf.Max(1, _data.ProjectilesCount);
        float step = 360f / count;
        Vector2 baseDir = _firePoint != null ? (Vector2)_firePoint.up : Vector2.up;
        if (baseDir.sqrMagnitude <= 0.0001f)
            baseDir = Vector2.up;
        else
            baseDir.Normalize();

        for (int i = 0; i < count; i++)
        {
            Vector2 origin = _firePoint != null ? (Vector2)_firePoint.position : (Vector2)transform.position;

            float angle = _data.StartAngle - (i * step); // clockwise
            angle += Mathf.Sin((i + _wobbleSeed) * _data.WobbleFrequency) * _data.WobbleAmplitude;
            angle += Random.Range(-_data.RandomAngleJitter, _data.RandomAngleJitter);

            Vector2 dir = Rotate(baseDir, angle);
            float speedScale = Random.Range(1f - _data.SpeedRandomPercent, 1f + _data.SpeedRandomPercent);
            float speed = Mathf.Max(0f, _data.ProjectileSpeed * speedScale);

            fbdfbd_EnemyBossMineProjectile mine =
                Instantiate(_data.MineProjectilePrefab, origin, Quaternion.identity);

            mine.Init(
                _data.Damage,
                dir,
                speed,
                _data.Deceleration,
                _data.StopSpeedThreshold,
                _data.BlinkDuration,
                _data.ExplodeDelay,
                _data.TargetMask,
                gameObject);

            if (_data.ProjectileInterval > 0f && i < count - 1)
                yield return new WaitForSeconds(_data.ProjectileInterval);
        }

        _fireRoutine = null;
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
