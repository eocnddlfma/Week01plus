using UnityEngine;

public interface IEnemyProjectile
{
    void ReflectAsBatHit(int damage, LayerMask targetLayer);
    Rigidbody2D Rb { get; }
    float Speed { get; }
    bool IsReflected { get; }
}
