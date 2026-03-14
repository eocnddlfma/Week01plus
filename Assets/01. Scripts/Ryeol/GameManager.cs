using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    #region GameState
    public enum GameState
    {
        Ready, // 게임 시작 전 (메뉴 상태)
        Playing, 
        GameOver,
        GameClear
    }

    public GameState CurrentState { get; private set; } = GameState.Ready;

    // Action
    public event Action<GameState> OnStateChanged;
    
    private void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        GameState oldState = CurrentState;
        CurrentState = newState;

        Debug.Log($"GameState: {oldState} → {newState}");

        OnStateChanged?.Invoke(newState);
    }

    #endregion

    private int _currentEnemyCount = 0;
    public int EnemyCount => _currentEnemyCount;

    public event Action OnEnemyUnregistered;

    #region 점수&레벨
    private int _score = 0;

    // Action
    public event Action<int> OnScoreChanged;
    #endregion

    // Temp: 플레이어 참조 
    private GameObject _player;

    public GameObject GetPlayer()
    {
        return _player;
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #region 게임 상태
    public void ReadyGame()
    {
        if (CurrentState == GameState.Ready)
            return;

        ChangeState(GameState.Ready);
    }

    public void StartGame()
    {
        if (CurrentState == GameState.Playing)
            return;

        _score = 0;
        _currentEnemyCount = 0;

        ChangeState(GameState.Playing);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver)
            return;

        ChangeState(GameState.GameOver);
    }

    public void GameClear()
    {
        if (CurrentState == GameState.GameClear)
            return;

        ChangeState(GameState.GameClear);
    }
    #endregion

    #region 현재 적 수 - Enemy 쪽에서 관리
    public void RegisterEnemy()
    {
        _currentEnemyCount++;
    }

    public void UnregisterEnemy(bool countAsKill = true)
    {
        _currentEnemyCount--;
        if (countAsKill)
            OnEnemyUnregistered?.Invoke();
    }

    #endregion

    #region 점수

    public void AddScore(int points)
    {
        _score += points;

        Debug.Log($"Score: {_score}");
        OnScoreChanged?.Invoke(_score);
    }
    public int GetScore()
    {
        return _score;
    }

    #endregion

}
