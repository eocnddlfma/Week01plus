using UnityEngine;

public class GameScene : MonoBehaviour
{

    // TODO (참조: 이것도 근데 원래 이렇게 안 하긴 하는데 일단 이런 식으로) 
    // 카메라
    // UI


    [SerializeField] private WaveManager _enemySpawner;

    private void Start()
    {
        if (_enemySpawner == null)
        {
            var spawner = FindAnyObjectByType<WaveManager>();
            if (spawner != null)
                _enemySpawner = spawner;
        }
        Init();
    }

    void Init()
    {
        Debug.Log("@>> GameScene Init()");

        GameEvents.OnGameStateChanged += HandleStateChanged;  // Phase 6: GameEvents로 변경

    }

    private void OnDestroy()
    {
        GameEvents.OnGameStateChanged -= HandleStateChanged;  // Phase 6: GameEvents로 변경
    }

    private void HandleStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.Playing:
                OnPlaying();
                break;
            case GameManager.GameState.GameOver:
                OnGameOver();
                break;
            case GameManager.GameState.GameClear:
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
