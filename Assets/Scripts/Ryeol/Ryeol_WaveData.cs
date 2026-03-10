using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class Ryeol_WaveData : ScriptableObject
{
    public int id;
    public int targetScore; // 목표 총 점수
    public int minEnemyCount = 5; // 적이 얼마나 스폰이 될 것인지
    public GameObject[] enemyPrefabs; // 적 종류
    //public float speedMultiplier = 1f; // 적 이동 속도 배수

    // 스폰 간격
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2f;

}


