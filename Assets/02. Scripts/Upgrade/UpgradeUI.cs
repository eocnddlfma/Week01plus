using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 업그레이드 선택 UI 패널.
///
/// 프리팹 구조:
///   UpgradeUI (CanvasGroup + UpgradeUI)
///   ├── Overlay (Image, 검정 반투명)
///   └── Panel (RectTransform)
///       ├── TitleText (TextMeshProUGUI) - "업그레이드 선택" (선택사항)
///       └── CardContainer
///           ├── Card0 (UpgradeCard)
///           ├── Card1 (UpgradeCard)
///           └── Card2 (UpgradeCard)
///
/// Canvas에 부착하고 CanvasGroup 컴포넌트가 필요합니다.
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private UpgradeCard[] _cards;

    [Header("애니메이션 설정")]
    [SerializeField] private float _fadeInDuration = 0.3f;
    [SerializeField] private float _cardAppearDelay = 0.1f;
    [SerializeField] private float _fadeOutDuration = 0.2f;

    private Action<UpgradeData> _onEachSelected;
    private Action _onAllDone;
    private int _remainingPicks;

    // pickCount: 카드 중 몇 개를 고를지 (기본 1)
    public void Show(List<UpgradeData> choices, Action<UpgradeData> onEachSelected, Action onAllDone = null, int pickCount = 1)
    {
        _onEachSelected = onEachSelected;
        _onAllDone = onAllDone;
        _remainingPicks = pickCount;

        gameObject.SetActive(true);

        // 카드 설정
        for (int i = 0; i < _cards.Length; i++)
        {
            if (i < choices.Count)
            {
                var card = _cards[i];
                _cards[i].gameObject.SetActive(true);
                _cards[i].Setup(choices[i], (data) => OnCardClicked(data, card));
            }
            else
            {
                _cards[i].gameObject.SetActive(false);
            }
        }

        // 페이드 인
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, _fadeInDuration).SetUpdate(true);
        }

        // 카드 등장 애니메이션
        for (int i = 0; i < _cards.Length && i < choices.Count; i++)
        {
            RectTransform cardRect = _cards[i].GetComponent<RectTransform>();
            cardRect.localScale = Vector3.zero;
            cardRect.DOScale(Vector3.one, 0.35f)
                .SetDelay(_fadeInDuration + i * _cardAppearDelay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }

    // 하위 호환: 기존 1개 선택 방식
    public void Show(List<UpgradeData> choices, Action<UpgradeData> onSelected)
        => Show(choices, onSelected, null, 1);

    public void Hide()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOFade(0f, _fadeOutDuration)
                .SetUpdate(true)
                .OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCardClicked(UpgradeData data, UpgradeCard clickedCard)
    {
        // 클릭한 카드만 숨김
        clickedCard.gameObject.SetActive(false);

        _onEachSelected?.Invoke(data);
        _remainingPicks--;

        if (_remainingPicks <= 0)
        {
            _onAllDone?.Invoke();
            Hide();
        }
    }
}
