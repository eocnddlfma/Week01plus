using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_Phase2EnemySpawn", menuName = "Scriptable Objects/BossSkill/Phase2/EnemySpawn")]
public class SSH_SOBossSkillPhase2EnemySpawn : fbdfbd_SOBossSkillBase
{
    [Header("Spawn")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private int          _spawnCount  = 3;
    [SerializeField] private float        _spawnRadius = 5f;
    [SerializeField] private float        _spawnDelay  = 0.3f;

    public GameObject[] EnemyPrefabs => _enemyPrefabs;
    public int          SpawnCount   => _spawnCount;
    public float        SpawnRadius  => _spawnRadius;
    public float        SpawnDelay   => _spawnDelay;
}
