using Unity.VisualScripting;
using UnityEngine;

public class WS_BallGetter : MonoBehaviour
{
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
        // Phase 6: WaveManager 참조 제거 및 GameEvents로 변경
        GameEvents.OnWaveCleared += GetBall;

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
    }

    private void OnDestroy()
    {
        // Phase 6: GameEvents로 변경
        GameEvents.OnWaveCleared -= GetBall;
    }

    private void GetBall(bool isBoss)
    {
        //TODO: 지워야 할 로직
        //_player.GetComponent<PlayerController>().Heal(1);

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