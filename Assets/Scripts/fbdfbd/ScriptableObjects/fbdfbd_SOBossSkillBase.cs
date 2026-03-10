using UnityEngine;

public enum BossSkillType
{
    Melee,
    Range,
}

[CreateAssetMenu(fileName = "BossSkill", menuName = "ScriptableObjects/BossSkill")]
public abstract class fbdfbd_SOBossSkillBase : ScriptableObject
{
    [SerializeField] private string _skillName;
    [SerializeField] private float _skillNumber;
    [SerializeField] private float _skillCooldown;
    [SerializeField] private float _skillCastTime;
    [SerializeField] private int _skillRepeatCount;
    [SerializeField] private BossSkillType _bossSkillType;
    [SerializeField] private GameObject _bossSkillParticle;


    public string SkillName => _skillName;
    public float SkillNumber => _skillNumber;
    public float SkillCooldown => _skillCooldown;
    public float SkillCastTime => _skillCastTime;
    public int SkillRepeatCount => _skillRepeatCount;
    public BossSkillType BossSkillType => _bossSkillType;
    public GameObject BossSkillParticle => _bossSkillParticle;
}
