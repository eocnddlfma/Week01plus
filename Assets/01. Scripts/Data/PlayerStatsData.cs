using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerStats")]
public class PlayerStatsData : ScriptableObject
{
    [Header("체력")]
    public int maxHp = 5;

    [Header("이동")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("대시")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.75f;

    [Header("피격")]
    public float invincibleDuration = 1.0f;
    public float flashInterval = 0.1f;
}
