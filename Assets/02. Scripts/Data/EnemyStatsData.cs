using UnityEngine;

[CreateAssetMenu(menuName = "Game/EnemyStats")]
public class EnemyStatsData : ScriptableObject
{
    public int hp = 3;
    public float moveSpeed = 2f;
    public float stopDistance = 1.2f;
    public int scoreReward = 100;
}
