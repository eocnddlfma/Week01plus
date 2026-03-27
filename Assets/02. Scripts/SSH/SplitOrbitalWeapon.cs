using UnityEngine;
using System.Collections.Generic;

public class SplitOrbitalWeapon : OrbitalWeapon
{
    [Header("Special Ability")]
    [Tooltip("적으로 판정할 레이어. 발사 중 해당 레이어와 충돌 시 반대 방향 복제구 생성")]
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Clone Limit")]
    [SerializeField] private int _maxClones = 8; // Inspector에서 조정

    private bool _isClone = false;
    private float _spawnTime = -1f;
    private const float _spawnGrace = 0.1f;

    // 업그레이드: 사방미인=4, 팔방미인=8 (기본 2 = 원본+반대방향 1개)
    public static int SplitDirections = 2;

    // 풀 & 카운터 (static으로 모든 인스턴스 공유)
    private static readonly Queue<SplitOrbitalWeapon> _clonePool = new Queue<SplitOrbitalWeapon>();
    private static int _activeCloneCount = 0;

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
            ReturnCloneToPool();
            return;
        }
        base.RejoinOrbit(currentPos);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 씬 종료 등으로 강제 파괴될 때 카운터 보정
        if (_isClone)
            _activeCloneCount = Mathf.Max(0, _activeCloneCount - 1);
    }

    private void SpawnClones()
    {
        int clonesCount = SplitDirections - 1;
        float angleStep = 360f / SplitDirections;

        for (int i = 1; i <= clonesCount; i++)
        {
            if (_activeCloneCount >= _maxClones)
            {
                Debug.Log($"[Split] 클론 한도 초과 ({_activeCloneCount}/{_maxClones}), 생성 건너뜀");
                break;
            }

            float angle = angleStep * i;
            float rad   = angle * Mathf.Deg2Rad;
            float cos   = Mathf.Cos(rad);
            float sin   = Mathf.Sin(rad);
            Vector2 cloneVel = new Vector2(
                _velocity.x * cos - _velocity.y * sin,
                _velocity.x * sin + _velocity.y * cos
            );

            SplitOrbitalWeapon clone = GetCloneFromPool();
            clone.transform.position = transform.position;
            clone.transform.rotation = transform.rotation;
            clone.gameObject.SetActive(true);
            clone.InitAsClone(cloneVel, _launchDuration);
            _activeCloneCount++;
            Debug.Log($"[Split] 클론 생성 ({_activeCloneCount}/{_maxClones})");
        }
    }

    private SplitOrbitalWeapon GetCloneFromPool()
    {
        while (_clonePool.Count > 0)
        {
            var pooled = _clonePool.Dequeue();
            if (pooled != null) return pooled;
        }

        // 풀 비었으면 새로 생성
        GameObject obj = Instantiate(gameObject, transform.position, transform.rotation);
        return obj.GetComponent<SplitOrbitalWeapon>();
    }

    private void ReturnCloneToPool()
    {
        _activeCloneCount = Mathf.Max(0, _activeCloneCount - 1);
        Debug.Log($"[Split] 클론 반환 ({_activeCloneCount}/{_maxClones})");

        // 상태 초기화 후 비활성화
        _isClone  = false;
        _spawnTime = -1f;
        gameObject.SetActive(false);
        _clonePool.Enqueue(this);
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
