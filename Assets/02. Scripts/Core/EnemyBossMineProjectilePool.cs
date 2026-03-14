using UnityEngine;

/// <summary>
/// 적 지뢰 투사체 풀 관리자 (Phase 7)
/// 2개 스포너에서 생성하는 EnemyBossMineProjectile을 풀링합니다.
/// </summary>
public class EnemyBossMineProjectilePool : MonoBehaviour
{
    public static EnemyBossMineProjectilePool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private EnemyBossMineProjectile _prefab;
    [SerializeField] private int _initialSize = 10;

    private ObjectPool<EnemyBossMineProjectile> _pool;

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (_prefab == null)
        {
            Debug.LogError("EnemyBossMineProjectilePool: Prefab not assigned!");
            return;
        }

        _pool = new ObjectPool<EnemyBossMineProjectile>(_prefab, transform, _initialSize, canExpand: true);
        Debug.Log($"EnemyBossMineProjectilePool initialized with {_initialSize} mines");
    }

    /// <summary>
    /// 풀에서 지뢰 투사체 획득
    /// </summary>
    public EnemyBossMineProjectile Get()
    {
        return _pool?.Get();
    }

    /// <summary>
    /// 지뢰 투사체를 풀에 반납
    /// </summary>
    public void Return(EnemyBossMineProjectile mine)
    {
        _pool?.Return(mine);
    }
}
