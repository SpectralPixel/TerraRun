using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    [Min(1)]
    public int MapWidth, MapHeight;

    public void GenerateWorld()
    {
        GridManager.GenerateGrid(MapWidth, MapHeight);
    }
}