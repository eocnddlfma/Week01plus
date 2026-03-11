using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Ryeol_GameManager;

public class Ryeol_UI_GameScene : MonoBehaviour
{


    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _quitButton;
    
    [SerializeField] private TMP_Text _gameEndText;

    // 연출
    [SerializeField] private CanvasGroup _gameEndPanel;
    [SerializeField] private RectTransform _panelRect;


    private void Start()
    {
        // 패널 초기화
        HidePanel();

        // 이벤트 구독
        Ryeol_GameManager.Instance.OnStateChanged += HandleStateChanged;

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
                _gameEndText.text = "Game Over";
                ShowPanel();
                break;
            case GameState.GameClear:
                _gameEndText.text = "Game Clear";
                ShowPanel();
                break;
        }
    }

    #region 연출
    private void HidePanel()
    {
        _gameEndPanel.alpha = 0f;
        _gameEndPanel.interactable = false;
        _gameEndPanel.blocksRaycasts = false;
    }

    private void ShowPanel()
    {
        _gameEndPanel.alpha = 1.0f;
        _gameEndPanel.interactable = true;
        _gameEndPanel.blocksRaycasts = true;

        var seq = DOTween.Sequence();
        seq.Append(_gameEndPanel.DOFade(1f, 0.5f).SetEase(Ease.OutCubic));
        seq.Join(_panelRect.DOLocalMoveY(0f, 0.5f).From(30f).SetEase(Ease.OutCubic));
        //seq.Append(_gameEndText.transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 5, 0.3f));
        seq.Append(_gameEndText.transform.DOScale(1f, 0.15f).From(1.4f).SetEase(Ease.OutExpo));
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
