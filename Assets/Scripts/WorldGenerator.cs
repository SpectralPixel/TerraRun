using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    public int WorldSeed;

    [Min(1)]
    public int MapWidth, MapHeight;
    public int FloorHeight;

    public List<OctaveSetting> Octaves;

    public void GenerateWorld()
    {
        GridManager.GenerateGrid(WorldSeed, MapWidth, MapHeight, FloorHeight, Octaves);
    }
}