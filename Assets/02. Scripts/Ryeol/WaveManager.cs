using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private List<WaveData> _waveDatas;

    [SerializeField] private float spawnRadius = 10f; // 플레이어에게서 해당 수치만큼 떨어진 곳에서 스폰.
    
    [SerializeField] private GameObject _player;

    [SerializeField] private float _changeWaveThreshold = 0.8f; // 웨이브 바뀌는 정도 (해당 Wave에 스폰된 적의 몇 퍼를 죽여야 넘어가는지)

    private Transform _enemyContainer;
    private int _currentWaveIndex = 0;
    private WaveData _currentWaveData;
    private List<WaveData.EnemySpawnInfo> _remainingEnemies;
    private bool _isChangingWave = false;
    private int _currentWaveKilledCount = 0;

    private void Start()
    {
        _waveDatas.Sort((a, b) => a.id.CompareTo(b.id)); // 혹시 모르니 정렬.
        _currentWaveData = _waveDatas[0];
        _enemyContainer = new GameObject("Enemies").transform;

        if (_player == null)
        {
            var playerObj = FindAnyObjectByType<PlayerBase>();
            if (playerObj != null)
                _player = playerObj.gameObject;
        }

        // 남은 적 리스트 초기화
        _remainingEnemies = new List<WaveData.EnemySpawnInfo>();
        foreach (var info in _currentWaveData.enemyList)
            _remainingEnemies.Add(new WaveData.EnemySpawnInfo { enemyPrefab = info.enemyPrefab, spawnCount = info.spawnCount });

        GameEvents.OnEnemyKilled += CheckNextWave;  // Phase 6: GameManager 이벤트에서 GameEvents로 변경

        StartCoroutine(CoSpawnEnemy());
    }

    private void OnDestroy()
    {
        GameEvents.OnEnemyKilled -= CheckNextWave;  // Phase 6: GameManager 이벤트에서 GameEvents로 변경
    }

    private IEnumerator CoSpawnEnemy()
    {
        while (true)
        {
            float randomInterval = UnityEngine.Random.Range(_currentWaveData.minSpawnInterval, _currentWaveData.maxSpawnInterval);
            yield return new WaitForSeconds(randomInterval);

            if (GameManager.Instance.CurrentState == GameManager.GameState.Playing
            && !_isChangingWave
            && ShouldSpawnEnemy())
            {
                SpawnEnemy();
            }
        }
    }

    private bool ShouldSpawnEnemy()
    {
        return _remainingEnemies != null && _remainingEnemies.Exists(e => e.spawnCount > 0);
    }

    
    private void CheckNextWave()
    {
        if (_isChangingWave) return; // 이미 넘어가는 중이면 무시

        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        _currentWaveKilledCount++; 

        if (ShouldNextWave())
            NextWave();
    }

    private bool ShouldNextWave()
    {
        int totalCount = 0;
        if (_currentWaveData.enemyList != null)
            foreach (var info in _currentWaveData.enemyList)
                totalCount += info.spawnCount;
        float threshold = _currentWaveData.isBoss ? 1f : _changeWaveThreshold;
        return _currentWaveKilledCount >= totalCount * threshold;
    }


    private void NextWave()
    {
        _isChangingWave = true;

        _currentWaveIndex++;

        if (_currentWaveIndex >= _waveDatas.Count)
        {
            // 게임 클리어
            Debug.Log("All waves cleared!");
            GameManager.Instance.GameClear();

            return;
        }

        _currentWaveData = _waveDatas[_currentWaveIndex];

        // 다음 웨이브가 보스 웨이브일 때만 잡몹 제거
        if (_currentWaveData.isBoss)
        {
            ClearEnemies();
            GameEvents.RaiseWaveCleared(true);  // Phase 6: GameEvents로 변경
        }
        else
        {
            GameEvents.RaiseWaveCleared(false);  // Phase 6: GameEvents로 변경
        }

        // 남은 적 리스트 초기화
        _remainingEnemies = new List<WaveData.EnemySpawnInfo>();
        foreach (var info in _currentWaveData.enemyList)
            _remainingEnemies.Add(new WaveData.EnemySpawnInfo { enemyPrefab = info.enemyPrefab, spawnCount = info.spawnCount });
        _currentWaveKilledCount = 0;
        _isChangingWave = false;
    }


    void SpawnEnemy()
    {
        if (_player == null) return;

        // 남은 적 중 spawnCount > 0인 것만 후보로
        var candidates = _remainingEnemies.FindAll(e => e.spawnCount > 0);
        if (candidates.Count == 0) return;

        var selected = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        selected.spawnCount--;

        // 플레이어 주변 랜덤 위치 계산
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = _player.transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

        // 적 생성
        GameObject enemy = Instantiate(selected.enemyPrefab, spawnPosition, Quaternion.identity);
        var enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
            enemyBase.SetTarget(_player.transform); // 적 타겟 주입
        enemy.transform.SetParent(_enemyContainer);
    }

    private void ClearEnemies()
    {
        foreach (Transform enemy in _enemyContainer)
        {
            Destroy(enemy.gameObject);
        }
    }

    public void Clear()
    {
        _currentWaveIndex = 0;
        _isChangingWave = false;
        _currentWaveData = _waveDatas[0];
        _currentWaveKilledCount = 0;
        // 남은 적 리스트 초기화
        _remainingEnemies = new List<WaveData.EnemySpawnInfo>();
        foreach (var info in _currentWaveData.enemyList)
            _remainingEnemies.Add(new WaveData.EnemySpawnInfo { enemyPrefab = info.enemyPrefab, spawnCount = info.spawnCount });

        ClearEnemies();

    }
}



// 밸런스 아이디어 (개인적으로 생각하는 핵심 재미는 한 방에 쫙 잡는 쾌감을 제공하는 것으로 생각)
// 근접 공격이 있나? 개인적으로 일부러 답답함을 주고 싶은 생각 너무 공을 치는 것에만 의존하지 않게
// 근접을 한 대 때리고 공을 쳐서 맞추면 잡는 그런 식으로 하고 싶음
// 차징도 써먹어야 하니 풀 차징일 때는 한 방으로 하고
// 밀쳐서 모아놓는다.

