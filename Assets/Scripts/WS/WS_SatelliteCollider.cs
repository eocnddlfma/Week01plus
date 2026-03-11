using UnityEngine;

public class WS_SatelliteCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.gameObject.GetComponent<fbdfbd_EnemyBase>();
        if (enemy == null)
            return;

        enemy.TakeDamage(100);
    }
}

