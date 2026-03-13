using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fbdfbd_EnemySplit : fbdfbd_EnemyBase
{
    [Header("Split")]
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private GameObject _splitEnemyClonePrefab;
    [Min(1)][SerializeField] private int _splitEnemyCloneCount = 4;
    [SerializeField] private List<Transform> _spawnPosList = new();

    [Header("Split Timing")]
    [Min(0f)][SerializeField] private float _splitInterval = 0.3f;
    [Min(0f)][SerializeField] private float _splitCycleInterval = 2f;

    private Coroutine _splitRoutine;
    private bool _isSplitting = false;

    protected override bool CanAttack(float distanceToTarget) => false;

    protected override void DoAttack() { }

    protected override void FixedUpdate()
    {
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

        Transform container = transform.parent;
        GameObject clone = Instantiate(_splitEnemyClonePrefab, spawnPos.position, spawnPos.rotation, container);


        if (clone.TryGetComponent(out fbdfbd_EnemySplitClone splitClone))
        {
            splitClone.SetTarget(Target);
        }
    }

    private void OnDisable()
    {
        StopSplitRoutine();
    }

    private void OnDestroy()
    {
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