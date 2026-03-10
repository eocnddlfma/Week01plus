using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ryeol_EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<Ryeol_WaveData> _waveDatas;

    [SerializeField] private float spawnRadius = 10f; // 플레이어에게서 해당 수치만큼 떨어진 곳에서 스폰.
    
    [SerializeField] private GameObject _player;
    
    private Transform _enemyContainer;
    private int _currentWaveIndex = 0;
    private Ryeol_WaveData _currentWaveData;
    private int _spawnIndex = 0; // 현재 스폰된 몹의 인덱스
    private bool _isChangingWave = false;

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
            float randomInterval = Random.Range(_currentWaveData.minSpawnInterval, _currentWaveData.maxSpawnInterval);
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

        if (Ryeol_GameManager.Instance.CurrentState == Ryeol_GameManager.GameState.Playing
            && ShouldNextWave())
        {
            NextWave();
        }
    }

    private bool ShouldNextWave()
    {
        int totalCount = _currentWaveData.enemyPrefabs.Length;
        int killed = totalCount - Ryeol_GameManager.Instance.EnemyCount;
        float threshold = _currentWaveData.isBoss ? 1f : 0.8f;
        return killed >= totalCount * threshold;
    }


    private void NextWave()
    {
        _isChangingWave = true;
        ClearEnemies();

        _currentWaveIndex++;
        if (_currentWaveIndex >= _waveDatas.Count)
        {
            // 게임 클리어
            Debug.Log("All waves cleared!");
            return;
        }
        _currentWaveData = _waveDatas[_currentWaveIndex];
        _spawnIndex = 0;
        _isChangingWave = false;
    }


    void SpawnEnemy()
    {
        if (_player == null) return;

        GameObject enemyPrefab = _currentWaveData.enemyPrefabs[_spawnIndex];
        _spawnIndex++;

        // 플레이어 주변 랜덤 위치 계산
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = _player.transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

        // 적 생성
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.GetComponent<fbdfbd_EnemyBase>().SetTarget(_player.transform); // 적 타겟 주입
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

        ClearEnemies();

    }
}
