using UnityEngine;

public interface IEnemyProjectile
{
    void ReflectAsBatHit(int damage, LayerMask targetLayer);
    void Deflect(float duration);
    void DisableProjectile();
    Rigidbody2D Rb { get; }
    float Speed { get; }
    bool IsReflected { get; }
}
