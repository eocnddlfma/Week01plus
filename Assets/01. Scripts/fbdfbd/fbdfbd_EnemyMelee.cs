using UnityEngine;

public class EnemyMelee : EnemyBase
{
    protected override bool CanAttack(float distanceToTarget) => false;

    protected override void DoAttack() { }
}
