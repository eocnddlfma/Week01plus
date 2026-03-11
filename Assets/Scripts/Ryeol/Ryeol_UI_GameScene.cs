using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Ryeol_GameManager;

public class Ryeol_UI_GameScene : MonoBehaviour
{

    [SerializeField] private GameObject _gameEndPanel;

    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private TMP_Text _gameEndText;


    private void Start()
    {
        // 패널 초기화
        if (_gameEndPanel != null) _gameEndPanel.SetActive(false);

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
                _gameEndPanel.SetActive(false);
                break;
            case GameState.Playing:
                _gameEndPanel.SetActive(false);
                break;
            case GameState.GameOver:
                _gameEndPanel.SetActive(true);
                _gameEndText.text = "Game Over";
                break;
            case GameState.GameClear:
                _gameEndPanel.SetActive(true);
                _gameEndText.text = "Game Clear";
                break;
        }
    }

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
