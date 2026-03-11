using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSH_ProjectileSpawner : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private LayerMask _targetMask;

    [Header("Spawn Settings")]
    [SerializeField] private int _count = 5;
    [SerializeField] private float _rangeWidth = 10f;
    [SerializeField] private float _rangeHeight = 5f;

    [Header("Warning Settings")]
    [SerializeField] private GameObject _warningPrefab;
    [SerializeField] private float _warningDuration = 2f;
    [SerializeField] private float _blinkInterval = 0.2f;

    public void SetCount(int count)   { _count = count; }
    public void SetSpeed(float speed) { _speed = speed; }

    private void Start()
    {
        StartCoroutine(WarningThenSpawn());
        Destroy(gameObject, 5f);
    }

    private IEnumerator WarningThenSpawn()
    {
        List<Vector3> spawnPositions = new List<Vector3>();

        for (int i = 0; i < _count; i++)
        {
            Vector3 localOffset = new Vector3(
                Random.Range(-_rangeWidth * 0.5f, _rangeWidth * 0.5f),
                Random.Range(0, _rangeHeight),
                0f
            );
            spawnPositions.Add(transform.position + transform.rotation * localOffset);
        }

        GameObject warningBox = null;
        if (_warningPrefab != null)
        {
            warningBox = Instantiate(_warningPrefab, transform.position, transform.rotation, transform);
            //warningBox.transform.localScale = new Vector3(_rangeWidth, _rangeHeight, 1f);
        }

        float elapsed = 0f;
        while (elapsed < _warningDuration)
        {
            if (warningBox != null) warningBox.SetActive(false);
            yield return new WaitForSeconds(_blinkInterval * 0.5f);
            if (warningBox != null) warningBox.SetActive(true);
            yield return new WaitForSeconds(_blinkInterval * 0.5f);
            elapsed += _blinkInterval;
        }

        if (warningBox != null) Destroy(warningBox);

        SpawnProjectiles(spawnPositions);
    }

    private void SpawnProjectiles(List<Vector3> positions)
    {
        if (_projectilePrefab == null) return;

        foreach (Vector3 pos in positions)
        {
            GameObject obj = Instantiate(_projectilePrefab, pos, transform.rotation, transform);
            ssh_EnemyProjectile proj = obj.GetComponent<ssh_EnemyProjectile>();
            if (proj != null)
            {
                proj.Init(_damage, _speed, _targetMask, gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Vector3 center = Vector3.up * _rangeHeight * 0.5f;
        Vector3 size = new Vector3(_rangeWidth, _rangeHeight, 0f);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireCube(center, size);

        Gizmos.matrix = prev;
    }
}
