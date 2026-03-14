using System.Collections.Generic;
using UnityEngine;

public class DamageTextPool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private TextFloating _textPrefab;
    [SerializeField] private int _initialCount = 20;
    [SerializeField] private bool _canExpand = true;

    private readonly List<TextFloating> _pool = new List<TextFloating>();

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_textPrefab == null)
        {
            Debug.LogError($"{name}: TextFloating 프리팹이 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < _initialCount; i++)
        {
            CreateItem();
        }
    }

    private TextFloating CreateItem()
    {
        TextFloating item = Instantiate(_textPrefab, transform);
        item.gameObject.SetActive(false);
        _pool.Add(item);
        return item;
    }

    private TextFloating GetAvailableItem()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (_pool[i].gameObject.activeSelf == false)
                return _pool[i];
        }

        if (_canExpand)
            return CreateItem();

        return null;
    }

    public void Show(int damage, Vector2 anchoredPosition, bool isCharge)
    {
        TextFloating item = GetAvailableItem();

        if (item == null)
        {
            Debug.LogWarning($"{name}: 사용 가능한 데미지 텍스트가 없습니다.");
            return;
        }

        item.Play(damage, anchoredPosition, isCharge);
    }
}