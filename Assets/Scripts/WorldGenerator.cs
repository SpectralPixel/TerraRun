using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    [Min(1)]
    public int MapWidth, MapHeight;
    public int floorHeight;

    public List<OctaveSetting> octaves;

    public void GenerateWorld()
    {
        GridManager.GenerateGrid(MapWidth, MapHeight, floorHeight, octaves);
    }
}