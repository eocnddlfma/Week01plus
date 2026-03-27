using DG.Tweening;
using UnityEngine;

/// <summary>
/// 튜토리얼 스위치.
/// OrbitalWeapon(공) 또는 트리거로 설정된 콜라이더가 닿으면
/// 연결된 오브젝트를 DOFade로 페이드아웃 후 비활성화합니다.
/// </summary>
public class TutorialSwitch : MonoBehaviour
{
    [Header("연결 오브젝트")]
    [SerializeField] private GameObject _targetObject;
    [SerializeField] private float _fadeDuration = 0.5f;

    [Header("Visuals")]
    [SerializeField] private Color _offColor = Color.gray;
    [SerializeField] private Color _onColor  = Color.yellow;

    private bool _isActivated = false;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            _spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null) break;
        }
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated) return;
        Activate();
    }

    public void OnBallHit()
    {
        if (_isActivated) return;
        Activate();
    }

    private void Activate()
    {
        _isActivated = true;
        UpdateVisual();

        if (_targetObject == null) return;

        var sr = _targetObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.DOFade(0f, _fadeDuration).OnComplete(() => _targetObject.SetActive(false));
            return;
        }

        var cg = _targetObject.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0f, _fadeDuration).OnComplete(() => _targetObject.SetActive(false));
            return;
        }

        _targetObject.SetActive(false);
    }

    private void UpdateVisual()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = _isActivated ? _onColor : _offColor;
    }
}
