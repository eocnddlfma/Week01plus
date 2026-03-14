using UnityEngine;

public interface IEnemyProjectile
{
    void ReflectAsBatHit(int damage, LayerMask targetLayer, Color color);
    Rigidbody2D Rb { get; }
}
