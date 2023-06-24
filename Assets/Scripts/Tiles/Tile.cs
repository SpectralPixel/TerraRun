using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Tile")]
public class Tile : ScriptableObject
{

    public string TileID = "Tile";
    public Sprite Sprite;


    [HideInInspector] public GameObject gameObject;

}
