using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ryeol_EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<Ryeol_WaveData> _waveDatas;

    [SerializeField] private float spawnRadius = 10f; // 플레이어에게서 해당 수치만큼 떨어진 곳에서 스폰.

    [SerializeField] private GameObject _player;

    [SerializeField] private Transform _bossSpawnPoint; // 보스 스폰 위치 (미설정 시 Vector3.zero)

    [SerializeField] private float _changeWaveThreshold = 0.8f; // 웨이브 바뀌는 정도 (해당 Wave에 스폰된 적의 몇 퍼를 죽여야 넘어가는지)

    private Transform _enemyContainer;
    private int _currentWaveIndex = 0;
    private Ryeol_WaveData _currentWaveData;
    private int _spawnIndex = 0; // 현재 스폰된 몹의 인덱스
    private bool _isChangingWave = false;
    private int _currentWaveKilledCount = 0;

    // 보스 웨이브 클리어 관련 이벤트
    public event Action OnBossCleared;

    private void Start()
    {
        _waveDatas.Sort((a, b) => a.id.CompareTo(b.id)); // 혹시 모르니 정렬.
        _currentWaveData = _waveDatas[0];
        _enemyContainer = new GameObject("Enemies").transform;

        Ryeol_GameManager.Instance.OnEnemyUnregistered += CheckNextWave;

        StartCoroutine(CoSpawnEnemy());
    }

    private void OnDestroy()
    {
        if (Ryeol_GameManager.Instance != null)
            Ryeol_GameManager.Instance.OnEnemyUnregistered -= CheckNextWave;
    }

    private IEnumerator CoSpawnEnemy()
    {
        while (true)
        {
            float randomInterval = UnityEngine.Random.Range(_currentWaveData.minSpawnInterval, _currentWaveData.maxSpawnInterval);
            yield return new WaitForSeconds(randomInterval);

            if (Ryeol_GameManager.Instance.CurrentState == Ryeol_GameManager.GameState.Playing
            && !_isChangingWave
            && ShouldSpawnEnemy())
            {
                SpawnEnemy();
            }
        }
    }

    private bool ShouldSpawnEnemy()
    {
        return _spawnIndex < _currentWaveData.enemyPrefabs.Length;
    }

    
    private void CheckNextWave()
    {
        if (_isChangingWave) return; // 이미 넘어가는 중이면 무시

        if (Ryeol_GameManager.Instance.CurrentState != Ryeol_GameManager.GameState.Playing) return;

        _currentWaveKilledCount++; 

        if (ShouldNextWave())
            NextWave();
    }

    private bool ShouldNextWave()
    {
        //if (_spawnIndex < _currentWaveData.enemyPrefabs.Length) return false; // 아직 스폰 중이면 막을까?

        int totalCount = _currentWaveData.enemyPrefabs.Length;
        float threshold = _currentWaveData.isBoss ? 1f : _changeWaveThreshold;
        return _currentWaveKilledCount >= totalCount * threshold;
    }


    private void NextWave()
    {
        _isChangingWave = true;

        // 막 클리어된 Wave가 보스Wave였다면
        if (_currentWaveData.isBoss)
            OnBossCleared?.Invoke();

        _currentWaveIndex++;

        if (_currentWaveIndex >= _waveDatas.Count)
        {
            // 게임 클리어
            Debug.Log("All waves cleared!");
            Ryeol_GameManager.Instance.GameClear();

            return;
        }

        _currentWaveData = _waveDatas[_currentWaveIndex];

        // 다음 웨이브가 보스 웨이브일 때만 잡몹 제거
        if (_currentWaveData.isBoss)
            ClearEnemies();

        _spawnIndex = 0;
        _isChangingWave = false;
    }


    void SpawnEnemy()
    {
        if (_player == null) return;

        GameObject enemyPrefab = _currentWaveData.enemyPrefabs[_spawnIndex];
        _spawnIndex++;

        Vector3 spawnPosition;
        if (_currentWaveData.isBoss)
        {
            spawnPosition = _bossSpawnPoint != null ? _bossSpawnPoint.position : Vector3.zero;
        }
        else
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            spawnPosition = _player.transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);
        }

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.GetComponent<fbdfbd_EnemyBase>().SetTarget(_player.transform);
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
        _spawnIndex = 0;
        _isChangingWave = false;
        _currentWaveData = _waveDatas[0];
        _currentWaveKilledCount = 0;

        ClearEnemies();

    }
}



// 밸런스 아이디어 (개인적으로 생각하는 핵심 재미는 한 방에 쫙 잡는 쾌감을 제공하는 것으로 생각)
// 근접 공격이 있나? 개인적으로 일부러 답답함을 주고 싶은 생각 너무 공을 치는 것에만 의존하지 않게
// 근접을 한 대 때리고 공을 쳐서 맞추면 잡는 그런 식으로 하고 싶음
// 차징도 써먹어야 하니 풀 차징일 때는 한 방으로 하고
// 밀쳐서 모아놓는다.

