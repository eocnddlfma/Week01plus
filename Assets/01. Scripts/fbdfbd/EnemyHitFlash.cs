using UnityEngine;
using System.Collections;

public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField] private EnemyBase _enemy;
    [SerializeField] private SpriteRenderer[] _renderers;
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField, Range(0f, 1f)] private float _flashBlend = 1f;
    [SerializeField, Min(0f)] private float _flashDuration = 0.08f;

    private Color[] _baseColors;
    private Coroutine _flashRoutine;
    public bool IsFlashing => _flashRoutine != null;

    private void Awake()
    {
        if (_enemy == null) _enemy = GetComponent<EnemyBase>();
        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);

        _baseColors = new Color[_renderers.Length];
        CacheBaseColors();
    }

    private void OnEnable()
    {
        if (_enemy != null) _enemy.OnDamaged += HandleDamaged;
        RestoreColors(); 
    }

    private void OnDisable()
    {
        if (_enemy != null) _enemy.OnDamaged -= HandleDamaged;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = null;
        RestoreColors(); 
    }

    private void HandleDamaged(int _)
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < _flashDuration)
        {
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].color = Color.Lerp(_baseColors[i], _flashColor, _flashBlend);

            elapsed += Time.deltaTime;
            yield return null;
        }

        RestoreColors();
        _flashRoutine = null;
    }

    private void CacheBaseColors()
    {
        for (int i = 0; i < _renderers.Length; i++)
            _baseColors[i] = _renderers[i].color;
    }

    private void RestoreColors()
    {
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].color = _baseColors[i];
    }
}
