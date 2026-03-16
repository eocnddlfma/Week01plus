using System.Collections;
using UnityEngine;

/// <summary>
/// 닷지볼 적: 일정 거리에서 멈추고 플레이어 위치를 중심으로
/// P1 보스의 닷지볼 패턴(라인 → 버스트)을 발사하는 원거리 적
/// </summary>
public class EnemyDodgeball : EnemyBase
{
    [Header("Dodgeball Attack")]
    [SerializeField] private SSH_SOBossSkillDodgeball _so;
    [SerializeField] private float _attackRange = 8f;

    [Header("Wave Scaling (Dodgeball)")]
    [Min(0f)][SerializeField] private float _attackRangeScalePerWave = 0.05f;

    private float _baseAttackRange;
    private bool _isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        _baseAttackRange = _attackRange;
    }

    protected override bool CanAttack(float distanceToTarget) =>
        !_isAttacking && distanceToTarget <= _attackRange;

    protected override void ApplyWaveScaling(int waveIndex)
    {
        base.ApplyWaveScaling(waveIndex);
        _attackRange = _baseAttackRange * (1f + waveIndex * _attackRangeScalePerWave);
    }

    protected override void DoAttack()
    {
        if (_so == null || _so.PatternPrefab == null) return;
        StartCoroutine(DodgeballPattern());
    }

    private GameObject SpawnPattern(Vector3 worldPos, Quaternion rot)
    {
        GameObject obj = Instantiate(_so.PatternPrefab, worldPos, rot);
        ProjectileSpawner spawner = obj.GetComponentInChildren<ProjectileSpawner>();
        if (spawner != null)
        {
            spawner.SetWarningDuration(_so.WarningDuration);
            spawner.SetBlinkInterval(_so.BlinkInterval);
        }
        return obj;
    }

    private IEnumerator DodgeballPattern()
    {
        _isAttacking = true;

        // 패턴 기준점: 공격 시작 시점의 플레이어 위치 (스냅)
        Vector3 center = Target != null ? Target.position : transform.position;
        float line  = _so.LineOffset * 0.5f;   // 보스보다 좁은 간격
        float step  = _so.StepDelay  * 0.6f;   // 보스보다 빠른 진행

        // Step 1: 가로 라인 (플레이어 통과)
        SpawnPattern(center, Quaternion.Euler(0f, 0f, 90f));
        yield return new WaitForSeconds(step);

        // Step 2: 세로 라인 좌우 2개
        SpawnPattern(center, Quaternion.identity);
        yield return new WaitForSeconds(step);

        yield return new WaitForSeconds(step);

        _isAttacking = false;
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
