using UnityEngine;
using System.Collections;

public class SSH_SplitOrbitalWeapon : Jaein_OrbitalWeapon
{
    [Header("Special Ability")]
    [Tooltip("적으로 판정할 레이어. 발사 중 해당 레이어와 충돌 시 반대 방향 복제구 생성")]
    [SerializeField] private LayerMask _enemyLayer;

    private bool _isClone = false;

    // Instantiate 직후 Start() 실행 전에 호출해 클론을 초기화
    public void InitAsClone(Vector2 velocity, float stateTimer)
    {
        _isClone = true;
        _state = BallState.Launched;
        _velocity = velocity;
        _stateTimer = stateTimer;
        _returnTimeElapsed = 0f;
    }

    protected override void Start()
    {
        if (_isClone) return;
        base.Start();
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        if (_isClone)
        {
            Destroy(gameObject);
            return;
        }
        base.RejoinOrbit(currentPos);
    }

    private void SpawnOppositeClone()
    {
        Vector2 radial = ((Vector2)transform.position - (Vector2)_center.position).normalized;
        Vector2 reflected = 2f * Vector2.Dot(_velocity, radial) * radial - _velocity;

        GameObject cloneObj = Instantiate(gameObject, transform.position, transform.rotation);
        SSH_SplitOrbitalWeapon clone = cloneObj.GetComponent<SSH_SplitOrbitalWeapon>();
        clone.InitAsClone(reflected, _launchDuration);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        if (_state != BallState.Orbit
            && (_enemyLayer.value & (1 << other.gameObject.layer)) != 0
            && !other.TryGetComponent<fbdfbd_EnemyProjectile>(out _))
        {
            SpawnOppositeClone();
        }
    }
}
