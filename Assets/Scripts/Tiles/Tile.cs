using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Tile")]
public class Tile : ScriptableObject
{

    public TileType Type;
    [Space]
    public string TileID = "Tile";
    [HideIfEnumValue("Type", HideIf.Equal, (int)TileType.Gas)] public Sprite Sprite;

    [HideIfEnumValue("Type", HideIf.NotEqual, (int)TileType.Solid, (int)TileType.Tree)] public float Hardness;

    [HideInInspector] public GameObject gameObject;

}

public enum TileType
{
    Solid,
    Liquid,
    Gas,
    Tree,
    Background
}