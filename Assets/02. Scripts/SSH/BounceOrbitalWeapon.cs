using UnityEngine;

/// <summary>
/// 튕기는 공: 원형 경계에 반사되며, 돌아올 때는 경계 무시
/// 업그레이드: 작용반작용(NoDamp), 예술적 각도(CollisionDamageBonus)
/// </summary>
public class BounceOrbitalWeapon : OrbitalWeapon
{
    [Header("Bounce Ability")]
    [SerializeField] private float _bounceDamper = 0.9f;
    [SerializeField] private LayerMask _enemyLayer;

    // ── 업그레이드 스태틱 ──
    public static bool NoDamp = false;              // 작용반작용: 속도 손실 없음
    public static bool CollisionDamageBonus = false; // 예술적 각도: 반사 횟수 비례 데미지

    private WS_CircleLineRenderer _boundaryRing;
    private int _bounceCount = 0;

    protected override void Start()
    {
        base.Start();
        _boundaryRing = FindAnyObjectByType<WS_CircleLineRenderer>();
    }

    private void LateUpdate()
    {
        if (State != BallState.Launched) return;
        if (_boundaryRing == null) return;

        Vector2 pos = transform.position;
        float dist = pos.magnitude;
        float boundary = _boundaryRing.Radius;

        if (dist >= boundary)
        {
            Vector2 outwardNormal = pos.normalized;
            float damper = NoDamp ? 1f : _bounceDamper;
            _velocity = Vector2.Reflect(_velocity, -outwardNormal) * damper;
            transform.position = outwardNormal * (boundary - 0.05f);

            if (CollisionDamageBonus)
                _bounceCount++;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        // 적 충돌 후 반사 카운트 초기화
        if (CollisionDamageBonus && (_enemyLayer.value & (1 << other.gameObject.layer)) != 0)
            _bounceCount = 0;
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        _bounceCount = 0;
        base.RejoinOrbit(currentPos);
    }

    protected override int CalculateDamage(float chargePercent, float attackPower = 1f)
    {
        int damage = base.CalculateDamage(chargePercent, attackPower);
        if (CollisionDamageBonus && _bounceCount > 0)
            damage = Mathf.RoundToInt(damage * (1f + _bounceCount * 0.1f));
        return damage;
    }
}
