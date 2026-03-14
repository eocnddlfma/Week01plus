using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    public int id;
    public GameObject[] enemyPrefabs; // 적 순서까지 고려해서 쫙 넣을 것임

    // 스폰 간격
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2f;

    // 보스 Wave인가
    public bool isBoss = false;

}




