using UnityEngine;

public interface ISelectable
{
    string Name { get; }
    string Description { get; }
    Rarity Rarity { get; }
    Sprite Icon { get; }
}
