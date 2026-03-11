using Unity.VisualScripting;
using UnityEngine;

public class WS_BallGetter : MonoBehaviour
{
    [SerializeField] private Ryeol_EnemySpawner _spawner;

    [Header("Ball")]
    [SerializeField] private GameObject[] _ballLists;
    [SerializeField] private Transform _container;

    [Header("Spawn")]
    [SerializeField] private Transform _player;
    [SerializeField] private float _spawnRadiusMin = 1.0f;
    [SerializeField] private float _spawnRadiusMax = 2.5f;

    [Header("Animation")]
    [SerializeField] private WS_BallGetterAnimation _anim;

    private void Start()
    {
        if (_spawner != null)
            _spawner.OnWaveClear += GetBall;
    }

    private void OnDestroy()
    {
        if (_spawner != null)
            _spawner.OnWaveClear -= GetBall;
    }

    private void GetBall(bool isBoss)
    {
        if (_ballLists == null || _ballLists.Length == 0)
        {
            Debug.LogWarning("BallGetter: _ballLists is empty.");
            return;
        }

        if (_player == null)
        {
            Debug.LogWarning("BallGetter: _player is not assigned.");
            return;
        }

        int randomIndex = Random.Range(0, _ballLists.Length);
        GameObject selectedBall = _ballLists[randomIndex];

        if (selectedBall == null)
        {
            Debug.LogWarning($"BallGetter: _ballLists[{randomIndex}] is null.");
            return;
        }

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        if (randomDir == Vector2.zero)
            randomDir = Vector2.right;

        float randomDistance = Random.Range(_spawnRadiusMin, _spawnRadiusMax);
        Vector3 spawnPosition = _player.position + (Vector3)(randomDir * randomDistance);

        Instantiate(selectedBall, spawnPosition, Quaternion.identity, _container);

        _anim.Play(selectedBall.GetComponent<SatelliteData>().SatelliteName, spawnPosition);
    }
}