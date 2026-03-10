using UnityEngine;

public class fbdfbd_EnemyMelee : fbdfbd_EnemyBase
{
    [Header("Melee")]
    [Min(0.1f)][SerializeField] private float attackRange = 1.2f;
    [Min(1)][SerializeField] private int damage = 1;
    [SerializeField] private LayerMask targetMask;

    protected override bool CanAttack(float distanceToTarget) => distanceToTarget <= attackRange;

    protected override void DoAttack()
    {
        Vector2 origin = Rb.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRange, targetMask);

        for (int i = 0; i < hits.Length; i++)
        {
            Debug.Log($"Melee {targetMask}, 타격 {i}");
        }
    }
}
