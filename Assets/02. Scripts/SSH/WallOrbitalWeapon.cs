using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 벽공: 닿은 적을 공과 함께 이동시킴 (끌어당기는 게 아니라 같이 이동)
/// </summary>
public class WallOrbitalWeapon : OrbitalWeapon
{
    [Header("Wall Ability")]
    [SerializeField] private LayerMask _enemyLayer;

    private struct AttachedEnemy
    {
        public EnemyBase enemy;
        public Rigidbody2D rb;
        public Vector2 offset; // 충돌 시 공 기준 적의 오프셋
    }

    private readonly List<AttachedEnemy> _attachedEnemies = new();

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_state == BallState.Orbit) return;

        base.OnTriggerEnter2D(other);

        if ((_enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;

        // 이미 부착된 적이면 무시
        for (int i = 0; i < _attachedEnemies.Count; i++)
            if (_attachedEnemies[i].enemy == enemy) return;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        Vector2 offset = (Vector2)enemy.transform.position - (Vector2)transform.position;

        _attachedEnemies.Add(new AttachedEnemy { enemy = enemy, rb = rb, offset = offset });
    }

    private void FixedUpdate()
    {
        if (_state != BallState.Launched || _attachedEnemies.Count == 0) return;

        for (int i = _attachedEnemies.Count - 1; i >= 0; i--)
        {
            var attached = _attachedEnemies[i];
            if (attached.enemy == null)
            {
                _attachedEnemies.RemoveAt(i);
                continue;
            }

            // 공 위치 + 오프셋으로 적 위치 고정
            Vector2 targetPos = (Vector2)transform.position + attached.offset;
            attached.rb.MovePosition(targetPos);
        }
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        _attachedEnemies.Clear();
        base.RejoinOrbit(currentPos);
    }
}
