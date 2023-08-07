using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{

    public ItemType Type;
    [Space]
    public string ItemID = "Item";
    public Sprite Icon;

    [HideIfEnumValue("Type", HideIf.NotEqual, (int)ItemType.Weapon)] public float Damage;
    [HideIfEnumValue("Type", HideIf.NotEqual, (int)ItemType.Tool)] public float Power;

    [HideIfEnumValue("Type", HideIf.NotEqual, (int)ItemType.Tile)] public Tile tile;

}

public enum ItemType
{
    Weapon,
    Tool,
    Consumable,
    Tile
}