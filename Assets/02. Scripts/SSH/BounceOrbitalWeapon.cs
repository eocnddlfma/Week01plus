using UnityEngine;

/// <summary>
/// 튕기는 공: 원형 경계에 반사되며, 돌아올 때는 경계 무시
/// </summary>
public class BounceOrbitalWeapon : OrbitalWeapon
{
    [Header("Bounce Ability")]
    [SerializeField] private float _bounceDamper = 0.9f;

    private WS_CircleLineRenderer _boundaryRing;

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
        float dist = pos.magnitude; // 중심 (0,0) 기준
        float boundary = _boundaryRing.Radius;

        if (dist >= boundary)
        {
            Vector2 outwardNormal = pos.normalized;

            // 경계 법선(안쪽)으로 반사
            _velocity = Vector2.Reflect(_velocity, -outwardNormal) * _bounceDamper;

            // 경계 안으로 밀어넣기
            transform.position = outwardNormal * (boundary - 0.05f);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }
}
