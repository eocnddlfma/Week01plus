using UnityEngine;

public interface IEnemyProjectile
{
    void ReflectAsBatHit(int damage, LayerMask targetLayer);
    Rigidbody2D Rb { get; }
}
