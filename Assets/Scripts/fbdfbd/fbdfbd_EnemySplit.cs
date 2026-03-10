using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fbdfbd_EnemySplit : fbdfbd_EnemyBase
{
    [Header("Split")]
    [Min(0.1f)][SerializeField] private float _attackRange = 1.2f;
    [Min(1)][SerializeField] private int _damage = 1;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private GameObject _splitEnemyClonePrefab;
    [Min(1)][SerializeField] private int _splitEnemyCloneCount = 4;
    [SerializeField] private List<Transform> _spawnPosList = new();

    [Header("Split Timing")]
    [Min(0f)][SerializeField] private float _splitInterval = 2f;

    private Coroutine _splitRoutine;
    private bool _isSpliting = false;
    private bool _isSplitComplete;

    protected override bool CanAttack(float distanceToTarget)
    {
        return distanceToTarget <= _attackRange;
    }

    protected override void DoAttack()
    {
        Vector2 origin = Rb.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, _attackRange, _targetMask);

        for (int i = 0; i < hits.Length; i++)
        {
            Debug.Log($"Split {this.name} : {_targetMask}, 타격 {i}");
        }
    }

    private void Start()
    {
        if (!CanStartSplit())
            return;

        _splitRoutine = StartCoroutine(SplitRoutine());
    }

    private IEnumerator SplitRoutine()
    {
        _isSpliting = true;
        for (int i = 0; i < _splitEnemyCloneCount; i++)
        {
            yield return new WaitForSeconds(_splitInterval);

            SpawnClone();
        }
        _isSplitComplete = true;
    }

    private bool CanStartSplit()
    {
        return _splitEnemyClonePrefab != null;
    }

    private void SpawnClone()
    {
        if (_spawnPosList == null || _spawnPosList.Count == 0)
            return;

        Transform spawnPos = _spawnPosList[Random.Range(0, _spawnPosList.Count)];

        if (spawnPos == null)
            return;

        GameObject clone = Instantiate(
            _splitEnemyClonePrefab,
            spawnPos.position,
            spawnPos.rotation);

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
    }
}
