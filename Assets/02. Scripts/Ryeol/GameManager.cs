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

    private void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        GameState oldState = CurrentState;
        CurrentState = newState;

        Debug.Log($"GameState: {oldState} → {newState}");

        GameEvents.RaiseGameStateChanged(newState);  // Phase 6: 글로벌 이벤트 버스
    }

    #endregion

    private int _currentEnemyCount = 0;
    public int EnemyCount => _currentEnemyCount;

    #region 점수&레벨
    private int _score = 0;
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
        {
            GameEvents.RaiseEnemyKilled();  // Phase 6: 글로벌 이벤트 버스
        }
    }

    #endregion

    #region 점수

    public void AddScore(int points)
    {
        _score += points;

        Debug.Log($"Score: {_score}");
        GameEvents.RaiseScoreChanged(_score);  // Phase 6: 글로벌 이벤트 버스
    }
    public int GetScore()
    {
        return _score;
    }

    #endregion

}
