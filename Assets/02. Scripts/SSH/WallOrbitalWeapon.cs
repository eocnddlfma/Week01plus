using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 벽공: 발사 중 닿은 적을 끈적하게 붙여서 같이 이동,
/// 가장 멀리 갔을 때(Launched→Returning 전환) 현재 방향으로 날려보냄
/// 업그레이드: 벽력일섬(ThresholdBlast), 벽치기(ThrowOnDetach – 더 강하게)
/// </summary>
public class WallOrbitalWeapon : OrbitalWeapon
{
    [Header("Wall Ability")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _throwForce = 15f;

    // ── 업그레이드 스태틱 ──
    public static bool ThresholdBlast = false; // 벽력일섬: 10명 이상 현재 체력 절반
    public static bool ThrowOnDetach = false;  // 벽치기: 날리는 힘 2배
    public static bool WideBody = false;       // 판때기: 가로 크기 4배

    private struct AttachedEnemy
    {
        public EnemyBase enemy;
        public Rigidbody2D rb;
        public Vector2 offset;
    }

    private readonly List<AttachedEnemy> _attachedEnemies = new();
    private bool _blastTriggered = false;
    private BallState _wallPrevState = BallState.Orbit;

    protected override void Start()
    {
        base.Start();
        if (WideBody)
            ApplyWideBody();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_state == BallState.Orbit) return;

        base.OnTriggerEnter2D(other);

        if ((_enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;

        // 이미 붙어있으면 무시
        for (int i = 0; i < _attachedEnemies.Count; i++)
            if (_attachedEnemies[i].enemy == enemy) return;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        Vector2 offset = (Vector2)enemy.transform.position - (Vector2)transform.position;
        _attachedEnemies.Add(new AttachedEnemy { enemy = enemy, rb = rb, offset = offset });

        // 벽력일섬: 10명 이상 붙으면 체력 절반
        if (ThresholdBlast && _attachedEnemies.Count >= 10 && !_blastTriggered)
        {
            _blastTriggered = true;
            TriggerThresholdBlast();
        }
    }

    private void FixedUpdate()
    {
        // Launched → Returning 전환 감지: 가장 멀리 간 시점에 던지기
        if (_wallPrevState == BallState.Launched && _state == BallState.Returning)
        {
            ThrowAttachedEnemies();
        }
        _wallPrevState = _state;

        // 발사 중에는 붙어서 같이 이동
        if (_state != BallState.Launched || _attachedEnemies.Count == 0) return;

        for (int i = _attachedEnemies.Count - 1; i >= 0; i--)
        {
            var attached = _attachedEnemies[i];
            if (attached.enemy == null || attached.enemy.IsDead)
            {
                _attachedEnemies.RemoveAt(i);
                continue;
            }
            attached.rb.MovePosition((Vector2)transform.position + attached.offset);
        }
    }

    private void ThrowAttachedEnemies()
    {
        if (_attachedEnemies.Count == 0) return;

        Vector2 throwDir = _velocity.normalized;
        if (throwDir.sqrMagnitude < 0.001f) throwDir = Vector2.right;

        float force = _throwForce * (ThrowOnDetach ? 2f : 1f);

        foreach (var attached in _attachedEnemies)
        {
            if (attached.enemy != null && !attached.enemy.IsDead)
                attached.enemy.AddExternalVelocity(throwDir * force, 0.5f);
        }

        _attachedEnemies.Clear();
        _blastTriggered = false;
    }

    private void TriggerThresholdBlast()
    {
        foreach (var attached in _attachedEnemies)
        {
            if (attached.enemy != null && !attached.enemy.IsDead)
                attached.enemy.TakeDamage(Mathf.Max(1, attached.enemy.Hp / 2));
        }
        Debug.Log("[벽공] 벽력일섬 발동!");
    }

    public void ApplyWideBody()
    {
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(s.x * 4f, s.y, s.z);
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        // 혹시 남은 적 정리 (비정상 복귀 등)
        _attachedEnemies.Clear();
        _blastTriggered = false;
        base.RejoinOrbit(currentPos);
    }
}
