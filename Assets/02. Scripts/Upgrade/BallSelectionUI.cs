using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BallSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private BallSelectionCard[] _cards;

    [Header("애니메이션 설정")]
    [SerializeField] private float _fadeInDuration = 0.3f;
    [SerializeField] private float _cardAppearDelay = 0.1f;
    [SerializeField] private float _fadeOutDuration = 0.2f;

    private Action<BallData> _onSelected;

    public void Show(List<BallData> choices, Action<BallData> onSelected)
    {
        _onSelected = onSelected;
        gameObject.SetActive(true);

        // 카드 설정
        for (int i = 0; i < _cards.Length; i++)
        {
            if (i < choices.Count)
            {
                _cards[i].gameObject.SetActive(true);
                _cards[i].Setup(choices[i], OnCardClicked);
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

    private void OnCardClicked(BallData data)
    {
        // 중복 클릭 방지
        foreach (var card in _cards)
            card.SetInteractable(false);

        _onSelected?.Invoke(data);
    }
}
