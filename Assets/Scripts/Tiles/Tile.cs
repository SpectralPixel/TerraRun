using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Tile")]
public class Tile : ScriptableObject
{

    public TileType Type;
    [Space]
    public string TileID = "Tile";
    [HideIfEnumValue("Type", HideIf.Equal, (int)TileType.Gas)] public Sprite Sprite;
    
    [HideInInspector] public GameObject gameObject;
    [HideInInspector] public float Value = 1f;

}

public enum TileType
{
    Solid,
    Liquid,
    Gas,
    PerlinNoise
}