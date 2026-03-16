using UnityEngine;

/// <summary>
/// 닷지볼 적: 일정 거리에서 멈추고, P1보스처럼 ProjectileSpawner 프리팹을
/// 플레이어 주변에 스폰해 경고 → 라인 발사 패턴을 사용하는 원거리 적.
/// </summary>
public class EnemyDodgeball : EnemyBase
{
    [Header("Ranged")]
    [Min(0.1f)][SerializeField] private float _attackRange = 10f;

    [Header("Dodgeball Pattern")]
    [SerializeField] private GameObject _patternPrefab;         // ProjectileSpawner가 붙어있는 프리팹
    [Min(1)][SerializeField] private int _laneCount = 2;        // 스폰할 레인 수
    [Min(0f)][SerializeField] private float _laneSpacing = 2f;  // 레인 간격
    [Min(0f)][SerializeField] private float _heightOffset = 5f; // 플레이어 위쪽으로 스폰 높이
    [Min(1)][SerializeField] private int _projectilesPerLane = 3;
    [Min(0f)][SerializeField] private float _warningDuration = 1.5f;
    [Min(0f)][SerializeField] private float _blinkInterval = 0.2f;
    [Min(0.1f)][SerializeField] private float _projectileSpeed = 3f;
    [Min(1)][SerializeField] private int _damage = 1;

    [Header("Wave Scaling (Dodgeball)")]
    [Min(0f)][SerializeField] private float _damageScalePerWave = 0.1f;
    [Min(0f)][SerializeField] private float _attackRangeScalePerWave = 0.05f;
    [Min(0f)][SerializeField] private float _projectileSpeedScalePerWave = 0.04f;

    private int _baseDamage;
    private float _baseAttackRange;
    private float _baseProjectileSpeed;

    protected override bool CanAttack(float distanceToTarget) => distanceToTarget <= _attackRange;

    protected override void Awake()
    {
        base.Awake();
        _baseDamage = _damage;
        _baseAttackRange = _attackRange;
        _baseProjectileSpeed = _projectileSpeed;
    }

    protected override void ApplyWaveScaling(int waveIndex)
    {
        base.ApplyWaveScaling(waveIndex);
        _damage = Mathf.Max(1, Mathf.RoundToInt(_baseDamage * (1f + waveIndex * _damageScalePerWave)));
        _attackRange = _baseAttackRange * (1f + waveIndex * _attackRangeScalePerWave);
        _projectileSpeed = _baseProjectileSpeed * (1f + waveIndex * _projectileSpeedScalePerWave);
    }

    protected override void DoAttack()
    {
        if (_patternPrefab == null || Target == null) return;

        Vector3 playerPos = Target.position;

        float halfSpread = (_laneCount - 1) * _laneSpacing * 0.5f;

        for (int i = 0; i < _laneCount; i++)
        {
            float xOffset = -halfSpread + i * _laneSpacing;

            // 플레이어 위쪽에 레인 스폰 (ProjectileSpawner는 항상 아래로 발사)
            Vector3 spawnPos = new(playerPos.x + xOffset, playerPos.y + _heightOffset, 0f);

            GameObject obj = Instantiate(_patternPrefab, spawnPos, Quaternion.identity);

            ProjectileSpawner spawner = obj.GetComponentInChildren<ProjectileSpawner>();
            if (spawner != null)
            {
                spawner.SetWarningDuration(_warningDuration);
                spawner.SetBlinkInterval(_blinkInterval);
                spawner.SetCount(_projectilesPerLane);
                spawner.SetSpeed(_projectileSpeed);
                spawner.SetDamage(_damage);
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
#endif
}
