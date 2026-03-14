using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [SerializeField] private int _defaultInitialSize = 5;

    private Dictionary<GameObject, ObjectPool<GameObject>> _pools =
        new Dictionary<GameObject, ObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 프리팹에 대한 풀을 가져옵니다. 없으면 자동 생성합니다.
    /// </summary>
    public GameObject Get(GameObject prefab)
    {
        if (prefab == null)
            return null;

        if (!_pools.ContainsKey(prefab))
        {
            CreatePool(prefab, _defaultInitialSize);
        }

        ObjectPool<GameObject> pool = _pools[prefab];
        GameObject instance = pool.Get();

        if (instance != null)
        {
            instance.SetActive(true);
        }

        return instance;
    }

    /// <summary>
    /// Enemy를 풀에 반환합니다.
    /// </summary>
    public void Return(GameObject instance, GameObject prefab)
    {
        if (instance == null || prefab == null)
            return;

        if (!_pools.ContainsKey(prefab))
        {
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        _pools[prefab].Return(instance);
    }

    private void CreatePool(GameObject prefab, int initialSize)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            onGetFunc: (obj) => { },
            onReturnFunc: (obj) => obj.SetActive(false),
            canExpand: true
        );

        // 초기 풀 채우기
        for (int i = 0; i < initialSize; i++)
        {
            GameObject instance = pool.Get();
            instance.SetActive(false);
            pool.Return(instance);
        }

        _pools[prefab] = pool;
    }

    private void OnDestroy()
    {
        _pools.Clear();
    }
}
