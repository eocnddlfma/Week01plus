using DG.Tweening;
using UnityEngine;

/// <summary>
/// 튜토리얼 전용 더미 적.
/// 트리거에 닿으면 즉사 → DOFade → Destroy.
/// 죽으면 지정한 공을 플레이어 근처에 스폰합니다.
/// </summary>
public class TutorialEnemy : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private float _spawnRadius = 1.5f;

    [Header("Death Effect")]
    [SerializeField] private float _fadeDuration = 0.3f;

    private bool _isDead;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead) return;
        Die();
    }

    public void Hit()
    {
        if (_isDead) return;
        Die();
    }

    private void Die()
    {
        _isDead = true;

        SpawnRewardBall();

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0 || _fadeDuration <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        int remaining = renderers.Length;
        foreach (var sr in renderers)
        {
            sr.DOFade(0f, _fadeDuration).OnComplete(() =>
            {
                remaining--;
                if (remaining <= 0)
                    Destroy(gameObject);
            });
        }
    }

    private void SpawnRewardBall()
    {
        if (_ballPrefab == null) return;

        var player = FindAnyObjectByType<TutorialPlayerController>();
        Vector3 spawnPos = player != null
            ? player.transform.position + (Vector3)(Random.insideUnitCircle.normalized * _spawnRadius)
            : transform.position;

        Instantiate(_ballPrefab, spawnPos, Quaternion.identity);
    }
}
