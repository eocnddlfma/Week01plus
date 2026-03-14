using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범용 오브젝트 풀 클래스 (Phase 7)
/// 빈번한 Instantiate/Destroy를 방지하고 GC Allocation을 감소시킵니다.
/// </summary>
public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly List<T> _pool = new List<T>();
    private readonly bool _canExpand;

    /// <summary>
    /// 오브젝트 풀 초기화
    /// </summary>
    /// <param name="prefab">풀링할 프리팹</param>
    /// <param name="parent">생성된 오브젝트의 부모 Transform</param>
    /// <param name="initialSize">초기 생성 개수</param>
    /// <param name="canExpand">부족 시 자동 확장 여부</param>
    public ObjectPool(T prefab, Transform parent, int initialSize = 10, bool canExpand = true)
    {
        _prefab = prefab;
        _parent = parent;
        _canExpand = canExpand;

        // 초기 풀 생성
        for (int i = 0; i < initialSize; i++)
            _pool.Add(CreateNew());
    }

    /// <summary>
    /// 풀에서 오브젝트 획득
    /// </summary>
    public T Get()
    {
        // 비활성 오브젝트 검색
        for (int i = _pool.Count - 1; i >= 0; i--)
        {
            var obj = _pool[i];

            // Destroy된 오브젝트 제거
            if (obj == null)
            {
                _pool.RemoveAt(i);
                continue;
            }

            if (!obj.gameObject.activeSelf)
            {
                obj.gameObject.SetActive(true);
                return obj;
            }
        }

        // 비활성 오브젝트 없음 → 확장 가능하면 생성
        if (_canExpand)
        {
            var newObj = CreateNew();
            newObj.gameObject.SetActive(true);
            _pool.Add(newObj);
            return newObj;
        }

        Debug.LogWarning($"ObjectPool<{typeof(T).Name}> is full and canExpand is false!");
        return null;
    }

    /// <summary>
    /// 오브젝트를 풀에 반납
    /// </summary>
    public void Return(T obj)
    {
        if (obj != null)
            obj.gameObject.SetActive(false);
    }

    /// <summary>
    /// 새로운 풀 오브젝트 생성
    /// </summary>
    private T CreateNew()
    {
        var obj = Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        return obj;
    }

    /// <summary>
    /// 풀 사이즈 반환 (디버그용)
    /// </summary>
    public int PoolSize => _pool.Count;
}
