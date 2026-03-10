using UnityEngine;

public class fbdfbd_EnemySplit : fbdfbd_EnemyBase
{
    [Header("Split")]
    [Min(0.1f)][SerializeField] private float _attackRange = 1.2f;
    [Min(1)][SerializeField] private int _damage = 1;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private GameObject _SplitEnemyClonePrefab;

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
            Debug.Log($"Split {_targetMask}, 타격 {i}");
        }
    }

    /*
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (IsDead) return;
    }*/


}
