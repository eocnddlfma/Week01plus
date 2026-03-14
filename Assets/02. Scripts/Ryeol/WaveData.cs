using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    public int id;

    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public int spawnCount;
    }

    public List<EnemySpawnInfo> enemyList;

    // 스폰 간격
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2f;

    // 보스 Wave인가
    public bool isBoss = false;

}




