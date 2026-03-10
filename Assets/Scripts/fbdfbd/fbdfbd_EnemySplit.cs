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
    [SerializeField] private List<Transform> _splitCloneEnemyPosList = new();

    [Header("Split Timing")]
    [Min(0f)][SerializeField] private float _splitInterval = 2f;

    private Coroutine _splitRoutine;
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
        foreach (Transform spawnPoint in _splitCloneEnemyPosList)
        {
            yield return new WaitForSeconds(_splitInterval);

            if (ShouldStopSplit())
                yield break;

            if (spawnPoint == null)
                continue;

            SpawnClone(spawnPoint);
        }

        _isSplitComplete = true;
    }

    private bool CanStartSplit()
    {
        return _splitEnemyClonePrefab != null
            && _splitCloneEnemyPosList != null
            && _splitCloneEnemyPosList.Count > 0;
    }

    private bool ShouldStopSplit()
    {
        return IsDead || _isSplitComplete || !gameObject.activeInHierarchy;
    }

    private void SpawnClone(Transform spawnPoint)
    {
        GameObject clone = Instantiate(_splitEnemyClonePrefab, transform);

        clone.transform.localPosition = spawnPoint.localPosition;
        clone.transform.localRotation = spawnPoint.localRotation;

        if (clone.TryGetComponent<Rigidbody2D>(out Rigidbody2D cloneRb))
        {
            cloneRb.linearVelocity = Vector2.zero;
            cloneRb.angularVelocity = 0f;
            cloneRb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (clone.TryGetComponent<fbdfbd_EnemySplitClone>(out fbdfbd_EnemySplitClone splitClone))
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
