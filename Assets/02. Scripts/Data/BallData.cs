using UnityEngine;

[CreateAssetMenu(menuName = "Game/BallData")]
public class BallData : ScriptableObject, ISelectable
{
    [SerializeField] private string ballName;
    [SerializeField, TextArea(2, 5)] private string description;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Rarity rarity;
    [SerializeField] private Sprite icon;

    public string BallName => ballName;
    public string Description => description;
    public GameObject BallPrefab => ballPrefab;
    public Rarity Rarity => rarity;
    public Sprite Icon => icon;

    // ISelectable 구현
    string ISelectable.Name => ballName;
    string ISelectable.Description => description;
    Rarity ISelectable.Rarity => rarity;
    Sprite ISelectable.Icon => icon;
}
