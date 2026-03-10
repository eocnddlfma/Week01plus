using UnityEngine;

public enum Rarity { 일반, 희귀, 영웅, 전설 }

public class SatelliteData : MonoBehaviour
{
    [Header("기본 정보")]
    [Tooltip("위성의 이름")]
    [SerializeField] private string satelliteName;
    [Tooltip("설명")]
    [SerializeField, TextArea(2, 5)] private string description;
    [Tooltip("위성의 희귀도 등급 (일반 < 희귀 < 영웅 < 전설)")]
    [SerializeField] private Rarity rarity;

    [Header("스탯")]
    [Tooltip("적에게 가하는 공격 데미지 수치")]
    [Range(1, 5)] [SerializeField] private int attackPower;
    [Tooltip("위성이 자전축을 기준으로 회전하는 속도")]
    [Range(1, 5)] [SerializeField] private int rotationPower;
    [Tooltip("위성이 궤도를 따라 공전하는 속도")]
    [Range(1, 5)] [SerializeField] private int orbitalPower;
    [Tooltip("위성을 발사할 때의 투사 속도")]
    [Range(1, 5)] [SerializeField] private int launchPower;
    [Tooltip("발사 후 원래 궤도로 복귀하는 속도")]
    [Range(1, 5)] [SerializeField] private int returnPower;

    public string SatelliteName => satelliteName;
    public string Description => description;
    public Rarity Rarity => rarity;
    public int AttackPower => attackPower;
    public int RotationPower => rotationPower;
    public int OrbitalPower => orbitalPower;
    public int LaunchPower => launchPower;
    public int ReturnPower => returnPower;
}
