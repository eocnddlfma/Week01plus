using UnityEngine;
using System.Collections;

public class SplitOrbitalWeapon : OrbitalWeapon
{
    [Header("Special Ability")]
    [Tooltip("적으로 판정할 레이어. 발사 중 해당 레이어와 충돌 시 반대 방향 복제구 생성")]
    [SerializeField] private LayerMask _enemyLayer;

    private bool _isClone = false;
    private float _spawnTime = -1f;
    private const float _spawnGrace = 0.1f;

    // Instantiate 직후 Start() 실행 전에 호출해 클론을 초기화
    public void InitAsClone(Vector2 velocity, float stateTimer)
    {
        _isClone = true;
        _state = BallState.Launched;
        _velocity = velocity;
        _stateTimer = stateTimer;
        _returnTimeElapsed = 0f;
        _spawnTime = Time.time;
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

    // 업그레이드: 사방미인=4, 팔방미인=8 (기본 2 = 원본+반대방향 1개)
    public static int SplitDirections = 2;

    private void SpawnClones()
    {
        int clonesCount = SplitDirections - 1;
        float angleStep = 360f / SplitDirections;

        for (int i = 1; i <= clonesCount; i++)
        {
            float angle = angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            Vector2 cloneVel = new Vector2(
                _velocity.x * cos - _velocity.y * sin,
                _velocity.x * sin + _velocity.y * cos
            );
            GameObject cloneObj = Instantiate(gameObject, transform.position, transform.rotation);
            SplitOrbitalWeapon clone = cloneObj.GetComponent<SplitOrbitalWeapon>();
            clone.InitAsClone(cloneVel, _launchDuration);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_isClone && _spawnTime >= 0f && Time.time - _spawnTime < _spawnGrace)
            return;

        base.OnTriggerEnter2D(other);

        if (_state != BallState.Orbit
            && (_enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            SpawnClones();
        }
    }
}
