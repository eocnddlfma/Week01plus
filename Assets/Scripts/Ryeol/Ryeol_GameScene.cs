using UnityEngine;

public class Ryeol_GameScene : MonoBehaviour
{

    // TODO (참조: 이것도 근데 원래 이렇게 안 하긴 하는데 일단 이런 식으로) 
    // 카메라
    // UI

    [SerializeField] private Ryeol_EnemySpawner _enemySpawner;
    [SerializeField] private GameObject _player;

    private void Awake()
    {
        Ryeol_GameManager.Instance.SetPlayer(_player);
        Ryeol_GameManager.Instance.StartGame();
    }
    private void Start()
    {
        Init();
    }

    void Init()
    {
        Debug.Log("@>> GameScene Init()");

        Ryeol_GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (Ryeol_GameManager.Instance != null)
            Ryeol_GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(Ryeol_GameManager.GameState newState)
    {
        switch (newState)
        {
            case Ryeol_GameManager.GameState.Playing:
                OnPlaying();
                break;
            case Ryeol_GameManager.GameState.GameOver:
                OnGameOver();
                break;
            case Ryeol_GameManager.GameState.GameClear:
                OnGameClear();
                break;
        }


    }

    private void OnPlaying()
    {
        _enemySpawner.Clear();
        //player.ResetPlayer();
    }

    private void OnGameOver()
    {
        _enemySpawner.Clear();
    }

    private void OnGameClear()
    {
        _enemySpawner.Clear();
    }

}
