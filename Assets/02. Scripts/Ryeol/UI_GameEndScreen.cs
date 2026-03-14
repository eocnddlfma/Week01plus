using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class UI_GameEndScreen : MonoBehaviour
{


    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _quitButton;
    
    [SerializeField] private TMP_Text _gameEndText;

    // 연출
    [SerializeField] private CanvasGroup _gameEndPanel;
    [SerializeField] private RectTransform _panelRect;
    [SerializeField] private CanvasGroup _buttonsGroup;
    [SerializeField] private RectTransform[] _buttonRects;  // Restart, Quit 순서대로 연결


    private void Start()
    {
        // 패널 초기화
        HidePanel();

        // 이벤트 구독
        GameManager.Instance.OnStateChanged += HandleStateChanged;

        #region 버튼 리스너 등록

        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        if (_quitButton != null)
            _quitButton.onClick.AddListener(OnQuitButtonClicked);

        #endregion


    }

    private void OnDestroy()
    {
        #region 버튼 리스너 해제
        
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (_quitButton != null)
            _quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        #endregion
    }

    private void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Ready:
            case GameState.Playing:
                HidePanel();
                break;
            case GameState.GameOver:
                ShowGameOver();
                break;
            case GameState.GameClear:
                ShowGameClear();
                break;
        }
    }

    #region 연출
    private void HidePanel()
    {
        _gameEndPanel.alpha = 0f;
        _gameEndPanel.interactable = false;
        _gameEndPanel.blocksRaycasts = false;

        _panelRect.anchoredPosition = Vector2.zero;
        _buttonsGroup.alpha = 0f;

    }

    private void ShowGameOver()
    {
        _gameEndPanel.interactable = true;
        _gameEndPanel.blocksRaycasts = true;

        _gameEndText.text = "Game Over";

        // 시작 위치 초기화
        _panelRect.anchoredPosition = new Vector2(0f, 80f);  // 위에서 시작
        _gameEndText.transform.localScale = Vector3.one;

        var seq = DOTween.Sequence();

        // 1) 패널 페이드인 + 위에서 아래로
        seq.Append(_gameEndPanel.DOFade(1f, 0.4f).SetEase(Ease.OutCubic));
        seq.Join(_panelRect.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutCubic));

        float originalY = _gameEndText.transform.localPosition.y;  // 원래 위치 저장

        // 2) 텍스트 오버슈트 후 쿵 — 원래 위치로 착지
        seq.Append(_gameEndText.transform.DOLocalMoveY(originalY, 0.3f)
                                         .From(originalY + 40f)  // 원래 위치보다 20 위에서 시작
                                         .SetEase(Ease.OutBounce));

        // 3) 착지 후 패널 흔들림 (무게감)
        seq.Append(_panelRect.DOShakePosition(0.3f, new Vector3(0f, 6f, 0f), vibrato: 8, randomness: 0f));

        seq.AppendCallback(ShowButtons);
    }

    private void ShowGameClear()
    {
        _gameEndPanel.interactable = true;
        _gameEndPanel.blocksRaycasts = true;

        _gameEndText.text = "Game Clear";

        _panelRect.anchoredPosition = Vector2.zero;
        _gameEndText.transform.localScale = Vector3.one;
        _gameEndText.transform.localRotation = Quaternion.identity;

        var seq = DOTween.Sequence();

        // 1) 패널 페이드인
        seq.Append(_gameEndPanel.DOFade(1f, 0.2f).SetEase(Ease.OutCubic));

        // 2) 도장 꽝 — 크게 시작해서 빡 제자리로 + 살짝 회전
        seq.Append(_gameEndText.transform.DOScale(1f, 0.2f).From(2.5f).SetEase(Ease.OutExpo));
        seq.Join(_gameEndText.transform.DOLocalRotate(Vector3.zero, 0.2f).From(new Vector3(0f, 0f, -8f)).SetEase(Ease.OutExpo));

        // 3) 도장 찍힌 후 미세하게 흔들리며 안착
        seq.Append(_gameEndText.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 4, 0.3f));

        seq.AppendCallback(ShowButtons);
    }

    private void ShowButtons()
    {
        _buttonsGroup.alpha = 0f;

        var seq = DOTween.Sequence();
        seq.Append(_buttonsGroup.DOFade(1f, 0.2f));

        for (int i = 0; i < _buttonRects.Length; i++)
        {
            int index = i;
            float originalY = _buttonRects[index].localPosition.y;
            float originalX = _buttonRects[index].localPosition.x;  // X 저장

            seq.Append(
                _buttonRects[index].DOLocalMoveY(originalY, 0.25f)
                                   .From(originalY - 20f)
                                   .SetEase(Ease.OutCubic)
                                   .OnUpdate(() => {
                                       // X는 항상 고정
                                       var pos = _buttonRects[index].localPosition;
                                       pos.x = originalX;
                                       _buttonRects[index].localPosition = pos;
                                   })
            );
        }
    }

    #endregion

    public void OnRestartButtonClicked()
    {
        Debug.Log("Restart Button Clicked");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("Quit Button Clicked");
        QuitGame();
    }


    public void QuitGame()
    {
#if UNITY_EDITOR
        // 에디터에서는 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // 빌드된 게임에서는 애플리케이션 종료
    Application.Quit();
#endif
    }


}
