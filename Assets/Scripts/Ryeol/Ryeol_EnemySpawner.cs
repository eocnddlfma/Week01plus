using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ryeol_EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<Ryeol_WaveData> _waveDatas;

    [SerializeField] private float spawnRadius = 10f; // 플레이어에게서 해당 수치만큼 떨어진 곳에서 스폰.
    

    private GameObject _player;
    private Transform _enemyContainer;

    private int _currentWaveIndex = 0; // 최적화를 위함
    private Ryeol_WaveData _currentWaveData;

    private void Start()
    {
        // TODO: 플레이어 참조 가져오기
        _player = Ryeol_GameManager.Instance.GetPlayer();

        _waveDatas.Sort((a, b) => a.id.CompareTo(b.id)); // 혹시 모르니 정렬.

        Ryeol_GameManager.Instance.OnScoreChanged += HandleScoreChanged;

        // 처음에 무조건 한번 가져오기
        _currentWaveData = GetCurrentWaveData();

        StartCoroutine(CoSpawnEnemy());
    }

    private void OnDestroy()
    {
        if (Ryeol_GameManager.Instance != null)
        {
            Ryeol_GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        _currentWaveData = GetCurrentWaveData();
    }

    private IEnumerator CoSpawnEnemy()
    {
        while (true)
        {
            float randomInterval = Random.Range(_currentWaveData.minSpawnInterval, _currentWaveData.maxSpawnInterval);
            yield return new WaitForSeconds(randomInterval);

            if (Ryeol_GameManager.Instance.CurrentState == Ryeol_GameManager.GameState.Playing
                && ShouldSpawnEnemy())
            {
                SpawnEnemy();
            }
        }
    }

    private bool ShouldSpawnEnemy()
    {
        if (_currentWaveData == null) return false;

        return Ryeol_GameManager.Instance.EnemyCount < _currentWaveData.minEnemyCount;
        
    }

    void SpawnEnemy()
    {
        if (_player == null) return;


        // 랜덤으로 적 종류 선택
        GameObject randomEnemyPrefab = _currentWaveData.enemyPrefabs[Random.Range(0, _currentWaveData.enemyPrefabs.Length)];

        // 플레이어 주변 랜덤 위치 계산
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = _player.transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

        // 적 생성
        GameObject enemy = Instantiate(randomEnemyPrefab, spawnPosition, Quaternion.identity);
        enemy.GetComponent<fbdfbd_EnemyBase>().SetTarget(_player.transform); // 적 타겟 주입
        enemy.transform.SetParent(_enemyContainer);
    }


    private Ryeol_WaveData GetCurrentWaveData()
    {
        int currentScore = Ryeol_GameManager.Instance.GetScore();

        for (int i = _currentWaveIndex; i < _waveDatas.Count; i++)
        {
            // 다음 Wave의 목표 점수에 아직 못 미치면 현재 Wave 유지
            if (currentScore < _waveDatas[i].targetScore)
                break;

            _currentWaveIndex = i;
        }

        return _waveDatas[_currentWaveIndex];

    }

    public void Clear()
    {
        _currentWaveIndex = 0;
        _currentWaveData = null;

        if (_enemyContainer != null)
            Destroy(_enemyContainer.gameObject);

        // 컨테이너 재생성
        _enemyContainer = new GameObject("Enemies").transform;
    }
}
