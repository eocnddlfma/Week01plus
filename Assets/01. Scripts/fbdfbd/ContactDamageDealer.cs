using System.Collections.Generic;
using UnityEngine;

public class ContactDamageDealer : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private LayerMask targetMask;

    private readonly Dictionary<int, float> nextHitTime = new();

    private void OnTriggerStay2D(Collider2D other)
    {
        if ((targetMask.value & (1 << other.gameObject.layer)) == 0) return;
        if (!other.TryGetComponent(out EntityBase target)) return;

        int id = other.GetInstanceID();
        if (nextHitTime.TryGetValue(id, out float t) && Time.time < t) return;

        target.TakeDamage(damage);
        nextHitTime[id] = Time.time + tickInterval;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        nextHitTime.Remove(other.GetInstanceID());
    }
}
