using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Ryeol_GameManager;

public class Ryeol_UI_GameScene : MonoBehaviour
{

    [SerializeField] private GameObject _readyPanel;
    [SerializeField] private GameObject _gameEndPanel;

    [SerializeField] private Button _startButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private TMP_Text _gameEndText;


    private void Start()
    {
        // 패널 초기화
        if (_readyPanel != null) _readyPanel.SetActive(true);
        if (_gameEndPanel != null) _gameEndPanel.SetActive(false);


        // 이벤트 구독
        Ryeol_GameManager.Instance.OnStateChanged += HandleStateChanged;

        #region 버튼 리스너 등록

        if (_startButton != null)
        {
            _startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (_menuButton != null)
        {
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        if (_quitButton != null)
            _quitButton.onClick.AddListener(OnQuitButtonClicked);

        #endregion


    }

    private void OnDestroy()
    {
        #region 버튼 리스너 해제
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (_menuButton != null)
        {
            _menuButton.onClick.RemoveListener(OnMenuButtonClicked);
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
                _readyPanel.SetActive(true);
                _gameEndPanel.SetActive(false);
                break;
            case GameState.Playing:
                _readyPanel.SetActive(false);
                _gameEndPanel.SetActive(false);
                break;
            case GameState.GameOver:
                _readyPanel.SetActive(false);
                _gameEndPanel.SetActive(true);
                _gameEndText.text = "Game Over";
                break;
            case GameState.GameClear:
                _readyPanel.SetActive(false);
                _gameEndPanel.SetActive(true);
                _gameEndText.text = "Game Clear";
                break;
        }
    }

    // Restart도 그냥 이거 쓰면 될듯.
    public void OnStartButtonClicked()
    {
        Debug.Log("Start Button Clicked");
        Ryeol_GameManager.Instance.StartGame();
    }

    public void OnMenuButtonClicked()
    {
        Debug.Log("Menu Button Clicked");
        Ryeol_GameManager.Instance.ReadyGame();
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
