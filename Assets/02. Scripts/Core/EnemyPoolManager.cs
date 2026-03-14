using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [SerializeField] private int _defaultInitialSize = 5;

    // 프리팹 경로(또는 인스턴스ID) → 풀 매핑
    private Dictionary<int, List<GameObject>> _pools = new Dictionary<int, List<GameObject>>();

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
    /// 프리팹에 대한 풀에서 Enemy를 획득합니다.
    /// </summary>
    public GameObject Get(GameObject prefab)
    {
        if (prefab == null)
            return null;

        int prefabId = prefab.GetInstanceID();

        if (!_pools.ContainsKey(prefabId))
        {
            CreatePool(prefab, _defaultInitialSize);
        }

        List<GameObject> pool = _pools[prefabId];

        // 비활성 오브젝트 찾기
        foreach (var obj in pool)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // 비활성 오브젝트 없으면 새로 생성
        GameObject newInstance = Instantiate(prefab);
        newInstance.SetActive(true);
        pool.Add(newInstance);
        return newInstance;
    }

    private void CreatePool(GameObject prefab, int initialSize)
    {
        int prefabId = prefab.GetInstanceID();
        List<GameObject> pool = new List<GameObject>();

        // 초기 풀 채우기
        for (int i = 0; i < initialSize; i++)
        {
            GameObject instance = Instantiate(prefab);
            instance.SetActive(false);
            pool.Add(instance);
        }

        _pools[prefabId] = pool;
    }

    private void OnDestroy()
    {
        _pools.Clear();
    }
}
