using UnityEngine;

/// <summary>
/// 적 투사체 풀 관리자 (Phase 7)
/// 5개 스포너에서 생성하는 EnemyProjectile을 풀링합니다.
/// </summary>
public class EnemyProjectilePool : MonoBehaviour
{
    public static EnemyProjectilePool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private EnemyProjectile _prefab;
    [SerializeField] private int _initialSize = 20;

    private ObjectPool<EnemyProjectile> _pool;

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
            Debug.LogError("EnemyProjectilePool: Prefab not assigned!");
            return;
        }

        _pool = new ObjectPool<EnemyProjectile>(_prefab, transform, _initialSize, canExpand: true);
        Debug.Log($"EnemyProjectilePool initialized with {_initialSize} projectiles");
    }

    /// <summary>
    /// 풀에서 투사체 획득
    /// </summary>
    public EnemyProjectile Get()
    {
        return _pool?.Get();
    }

    /// <summary>
    /// 투사체를 풀에 반납
    /// </summary>
    public void Return(EnemyProjectile projectile)
    {
        _pool?.Return(projectile);
    }
}
