using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySplit : EnemyBase
{
    [Header("Split")]
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private GameObject _splitEnemyClonePrefab;
    [Min(1)][SerializeField] private int _splitEnemyCloneCount = 4;
    [SerializeField] private List<Transform> _spawnPosList = new();

    [Header("Split Timing")]
    [Min(0f)][SerializeField] private float _splitInterval = 0.3f;
    [Min(0f)][SerializeField] private float _splitCycleInterval = 2f;

    [Header("Wave Scaling (Split)")]
    [Min(0f)][SerializeField] private float _cloneCountScalePerWave = 0.15f; // 클론 수 증가율
    [Min(0f)][SerializeField] private float _splitIntervalReducePerWave = 0.05f; // 클론 소환 간격 감소율
    [Min(0f)][SerializeField] private float _cycleIntervalReducePerWave = 0.05f; // 사이클 주기 감소율

    private int _baseCloneCount;
    private float _baseSplitInterval;
    private float _baseCycleInterval;

    private Coroutine _splitRoutine;
    private bool _isSplitting = false;

    protected override bool CanAttack(float distanceToTarget) => false;

    protected override void DoAttack() { }

    protected override void Awake()
    {
        base.Awake();
        _baseCloneCount = _splitEnemyCloneCount;
        _baseSplitInterval = _splitInterval;
        _baseCycleInterval = _splitCycleInterval;
    }

    protected override void ApplyWaveScaling(int waveIndex)
    {
        base.ApplyWaveScaling(waveIndex);
        _splitEnemyCloneCount = Mathf.Max(1, Mathf.RoundToInt(_baseCloneCount * (1f + waveIndex * _cloneCountScalePerWave)));
        _splitInterval = _baseSplitInterval / (1f + waveIndex * _splitIntervalReducePerWave);
        _splitCycleInterval = _baseCycleInterval / (1f + waveIndex * _cycleIntervalReducePerWave);
    }

    protected override void FixedUpdate()
    {
        if (IsKnockedBack)
        {
            base.FixedUpdate();
            return;
        }

        if (_isSplitting)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }
        base.FixedUpdate();
    }

    private void Start()
    {
        if (!CanStartSplit())
            return;

        _splitRoutine = StartCoroutine(SplitRoutine());
    }

    private IEnumerator SplitRoutine()
    {
        while (true)
        {
            _isSplitting = true;

            for (int i = 0; i < _splitEnemyCloneCount; i++)
            {
                SpawnClone();
                yield return new WaitForSeconds(_splitInterval);
            }

            _isSplitting = false;

            yield return new WaitForSeconds(_splitCycleInterval);
        }
    }

    private bool CanStartSplit()
    {
        return _splitEnemyClonePrefab != null
            && _spawnPosList != null
            && _spawnPosList.Count > 0;
    }

    private void SpawnClone()
    {
        Transform spawnPos = _spawnPosList[Random.Range(0, _spawnPosList.Count)];

        if (spawnPos == null)
            return;

        // Phase 7: 풀에서 클론 획득
        if (EnemyPoolManager.Instance == null)
            return;

        Transform container = transform.parent;
        GameObject clone = EnemyPoolManager.Instance.Get(_splitEnemyClonePrefab);
        if (clone != null)
        {
            clone.transform.position = spawnPos.position;
            clone.transform.rotation = spawnPos.rotation;
            clone.transform.SetParent(container);

            if (clone.TryGetComponent(out EnemySplitClone splitClone))
            {
                splitClone.SetTarget(Target);
            }
        }
    }

    protected override void ResetState()
    {
        base.ResetState();
        StopSplitRoutine();
        _isSplitting = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopSplitRoutine();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopSplitRoutine();
    }

    private void StopSplitRoutine()
    {
        if (_splitRoutine == null)
            return;

        StopCoroutine(_splitRoutine);
        _splitRoutine = null;
        _isSplitting = false;
    }
}