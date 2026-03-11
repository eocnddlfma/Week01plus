using UnityEngine;

public class fbdfbd_EnemySplitClone : fbdfbd_EnemyBase
{
    protected override bool ShouldTrackEnemyCount => false;

    protected override bool CanAttack(float distanceToTarget) => false;

    protected override void DoAttack() { }
}
