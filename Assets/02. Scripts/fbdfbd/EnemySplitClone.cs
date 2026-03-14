using UnityEngine;

public class EnemySplitClone : EnemyBase
{
    protected override bool ShouldTrackEnemyCount => false;

    protected override bool CanAttack(float distanceToTarget) => false;

    protected override void DoAttack() { }
}
