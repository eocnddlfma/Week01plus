using UnityEngine;

public class WS_DamageTextManager : MonoBehaviour
{
    public static WS_DamageTextManager I { get; private set; }

    [Tooltip("인스펙터에 할당 해야함")][Header("References")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private WS_DamageTextPool _pool;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    public void Show(int damage, Vector3 worldPosition)
    {
        if (_canvas == null)
        {
            Debug.LogError($"{name}: Canvas 참조가 없습니다.");
            return;
        }

        if (_pool == null)
        {
            Debug.LogError($"{name}: WS_DamageTextPool 참조가 없습니다.");
            return;
        }

        Camera uiCamera = null;

        if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = _canvas.worldCamera != null ? _canvas.worldCamera : _worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_worldCamera, worldPosition);

        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _pool.GetComponent<RectTransform>(),
            screenPoint,
            uiCamera,
            out Vector2 localPoint
        );

        if (success == false)
            return;

        _pool.Show(damage, localPoint);
    }
}