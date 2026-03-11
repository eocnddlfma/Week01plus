using UnityEngine;

// 프리팹 인스펙터 설정:
//   _orbitRadius       ~0.5   (플레이어 근접 판정 거리)
//   _returnStrength    20~30  (플레이어 방향 끌어당기는 힘)
//   _orbitAssistStrength 0    (수평 보조 불필요)
public class SSH_BossSkillPhase2OrbitalDropProjectile : Jaein_OrbitalWeapon
{
    [Header("Damage")]
    [SerializeField] private int       _damage        = 1;
    [SerializeField] private int       _bossDamage    = 300;
    [SerializeField] private LayerMask _playerMask;

    [Header("Bat Return")]
    [SerializeField] private float     _batReturnSpeed = 125f;

    private Transform _bossTransform;
    private bool      _batHit = false;

    // base.Awake()는 private → Unity가 둘 다 호출, Init()에서 _center 덮어씀
    protected override void Awake() 
    {
        base.Awake();
        _state = BallState.Returning;
    }

    protected override void Start()
    {
        // base.Start()의 궤도 위치 초기화를 막음 (Init에서 직접 설정)
    }

    public void Init(Transform boss, Transform player,
                     int damage, int bossDamage, LayerMask playerMask)
    {
        _bossTransform = boss;
        _damage        = damage;
        _bossDamage    = bossDamage;
        _playerMask    = playerMask;

        // center = 플레이어 → Returning 물리가 플레이어 방향으로 당김
        _center            = player;
        _velocity          = Vector2.zero;
        _returnTimeElapsed = 0f;
        _batHit            = false;

        // 타이머 없이 처음부터 Returning 상태로 플레이어에게 날아감
        _state = BallState.Returning;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // 배트(Weapon 태그) → 보스 방향 직선 발사
        if (other.CompareTag("Weapon") && !_batHit)
        {
            Debug.Log($"[OrbitalDrop] 배트 충돌 | bossTransform={_bossTransform} | state={_state}");
            _batHit = true;
            if (_bossTransform != null)
            {
                Vector2 dir = ((Vector2)_bossTransform.position - (Vector2)transform.position).normalized;
                _velocity   = dir * _batReturnSpeed;
                _state      = BallState.Launched;
                _stateTimer = 10f;
                Debug.Log($"[OrbitalDrop] 발사 완료 | dir={dir} | speed={_batReturnSpeed}");
            }
            else
            {
                Debug.LogWarning("[OrbitalDrop] _bossTransform이 null! Init이 호출됐는지 확인 필요");
            }

            other.GetComponentInChildren<WS_EffectParticle>()?.Play(0f);
            return;
        }

        Debug.Log($"[OrbitalDrop] 충돌 | tag={other.tag} | batHit={_batHit} | layer={other.gameObject.layer}");

        if (!_batHit)
        {
            // 플레이어 피격
            if ((_playerMask.value & (1 << other.gameObject.layer)) != 0)
            {
                other.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
            }
        }
        else
        {
            // 배트로 친 후 보스 피격
            if (_bossTransform != null && other.gameObject == _bossTransform.gameObject)
            {
                _bossTransform.SendMessage("TakeDamage", _bossDamage, SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
            }
        }
    }

    protected override void RejoinOrbit(Vector2 currentPos)
    {
        Destroy(gameObject);
    }
}
