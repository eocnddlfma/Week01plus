using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill_Dash", menuName = "Scriptable Objects/BossSkill/Dash")]
public class fbdfbd_SOBossSkillDash : fbdfbd_SOBossSkillBase
{
    [Header("Dash Move")]
    [Min(0.1f)][SerializeField] private float _dashSpeed = 12f;
    [Min(0.05f)][SerializeField] private float _dashDuration = 0.25f;

    [Header("Dash FX")]
    [SerializeField] private GameObject _dashStartFxPrefab;

    public float DashSpeed => _dashSpeed;
    public float DashDuration => _dashDuration;
    public GameObject DashStartFxPrefab => _dashStartFxPrefab;
}
