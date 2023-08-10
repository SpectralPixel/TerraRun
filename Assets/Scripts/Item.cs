using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{

    public ItemType Type;
    [Space]
    public string ItemID = "Item";

    [HideIfEnumValue("Type", HideIf.NotEqual, (int)ItemType.Weapon)] public float Damage;
    [HideIfEnumValue("Type", HideIf.NotEqual, (int)ItemType.Tool)] public float Power;

}

public enum ItemType
{
    Weapon,
    Tool,
    Accessory,
    Material,
    Tile
}