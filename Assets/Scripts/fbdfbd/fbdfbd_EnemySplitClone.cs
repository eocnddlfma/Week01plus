using UnityEngine;

public class fbdfbd_EnemySplitClone : fbdfbd_EnemyBase
{
    [Header("SplitEnemy")]
    [Min(0.1f)][SerializeField] private float _attackRange = 1.2f;
    [Min(1)][SerializeField] private int _damage = 1;
    [SerializeField] private LayerMask _targetMask;

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
            Debug.Log($"Split Clone {GetInstanceID()} {_targetMask}, 타격 {i}");
        }
    }
}
