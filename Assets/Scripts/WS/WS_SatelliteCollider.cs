using System;
using System.Collections.Generic;
using System.Text;
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
    //private void OnTriggerEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
    //        return;

    //    if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
    //    {
    //        Debug.Log($"Hit + {collision.gameObject.name}");
    //    }
    //}
}

