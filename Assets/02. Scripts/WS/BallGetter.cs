using System.Collections.Generic;
using UnityEngine;

public class WS_BallGetter : MonoBehaviour
{
    [Header("Ball")]
    [SerializeField] private List<BallData> _ballPool;
    [SerializeField] private Transform _container;

    [Header("Spawn")]
    [SerializeField] private Transform _player;
    [SerializeField] private float _spawnRadiusMin = 1.0f;
    [SerializeField] private float _spawnRadiusMax = 2.5f;

    [Header("Animation")]
    [SerializeField] private WS_BallGetterAnimation _anim;

    [Header("UI")]
    [SerializeField] private BallSelectionUI _ballSelectionUI;
    [SerializeField] private int _selectionChoiceCount = 3;

    private void Start()
    {
        // Phase 6: WaveManager 참조 제거 및 GameEvents로 변경
        GameEvents.OnWaveCleared += OnWaveCleared;

        if (_player == null)
        {
            var playerObj = FindAnyObjectByType<PlayerController>();
            if (playerObj != null)
                _player = playerObj.transform;
        }
        if (_container == null)
            _container = transform;
        if (_anim == null)
            _anim = GetComponentInChildren<WS_BallGetterAnimation>();

        if (_ballSelectionUI != null)
            _ballSelectionUI.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Phase 6: GameEvents로 변경
        GameEvents.OnWaveCleared -= OnWaveCleared;
    }

    private void OnWaveCleared(bool isBoss)
    {
        // UpgradeManager에서 호출되므로 여기서는 아무것도 안 함
    }

    public void ShowBallSelection()
    {
        if (_ballPool == null || _ballPool.Count == 0)
        {
            Debug.LogWarning("BallGetter: _ballPool is empty.");
            Time.timeScale = 1f;
            return;
        }

        if (_player == null)
        {
            Debug.LogWarning("BallGetter: _player is not assigned.");
            Time.timeScale = 1f;
            return;
        }

        if (_ballSelectionUI == null)
        {
            Debug.LogWarning("BallGetter: _ballSelectionUI is not assigned.");
        }

        if (_ballSelectionUI != null)
        {
            // UI로 선택
            Time.timeScale = 0f;
            List<BallData> choices = GetRandomBalls(_selectionChoiceCount);
            _ballSelectionUI.Show(choices, OnBallSelected);
        }
        else
        {
            // UI 없으면 랜덤 선택
            BallData randomBall = _ballPool[Random.Range(0, _ballPool.Count)];
            SpawnBall(randomBall);
            Time.timeScale = 1f;
        }
    }

    private void OnBallSelected(BallData selected)
    {
        _ballSelectionUI.Hide();
        Time.timeScale = 1f;
        SpawnBall(selected);
    }

    private void SpawnBall(BallData ballData)
    {
        if (ballData.BallPrefab == null)
        {
            Debug.LogWarning($"BallGetter: BallPrefab for {ballData.BallName} is null.");
            return;
        }

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        if (randomDir == Vector2.zero)
            randomDir = Vector2.right;

        float randomDistance = Random.Range(_spawnRadiusMin, _spawnRadiusMax);
        Vector3 spawnPosition = _player.position + (Vector3)(randomDir * randomDistance);

        GameObject spawnedBall = Instantiate(ballData.BallPrefab, spawnPosition, Quaternion.identity, _container);
        SatelliteData satelliteData = spawnedBall.GetComponent<SatelliteData>();

        if (_anim != null && satelliteData != null)
            _anim.Play(satelliteData.SatelliteName, spawnPosition);

        Debug.Log($"[BallGetter] 공 선택: {ballData.BallName}");
    }

    private List<BallData> GetRandomBalls(int count)
    {
        List<BallData> result = new List<BallData>();
        List<BallData> pool = new List<BallData>(_ballPool);

        count = Mathf.Min(count, pool.Count);

        // Fisher-Yates 셔플로 중복 없이 선택
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}