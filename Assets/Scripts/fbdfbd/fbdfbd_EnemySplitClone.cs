using UnityEngine;

public class fbdfbd_EnemySplitClone : fbdfbd_EnemyBase
{
    [Header("SplitEnemy")]
    [Min(0.1f)][SerializeField] private float _attackRange = 1.2f;
    [Min(1)][SerializeField] private int _damage = 1;
    [SerializeField] private LayerMask _targetMask;
    protected override bool ShouldTrackEnemyCount => false;

    protected override bool CanAttack(float distanceToTarget)
    {
        return distanceToTarget <= _attackRange;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (CanAttackToTarget())
        {
            ScheduleNextAttack();
            DoAttack();
        }
    }

    protected override void DoAttack()
    {
        Vector2 origin = Rb.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, _attackRange, _targetMask);

        for (int i = 0; i < hits.Length; i++)
        {
            Jaein_ObjectBase dmg = hits[i].GetComponent<Jaein_ObjectBase>();
            if (dmg != null)
            {
                dmg.TakeDamage(_damage);
            }
        }
    }
}
