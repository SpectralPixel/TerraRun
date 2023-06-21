using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    public static WorldGenerator Instance;

    public int WorldSeed;

    [Min(1)]
    public int MapWidth, MapHeight;
    public int FloorHeight;

    public List<OctaveSetting> Octaves;

    [HideInInspector] public int SeedXOffset;
    [HideInInspector] public float[] SeedVariationMultipliers;

    [Space]
    [SerializeField] private Transform trackedObject;
    [SerializeField] private int renderDistance;

    private List<Vector2Int> tilesToRemove;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        tilesToRemove = new List<Vector2Int>();

        GenerateWorld();
    }

    public void GenerateWorld()
    {
        (SeedXOffset, SeedVariationMultipliers) = GridManager.GenerateGrid(WorldSeed, MapWidth, MapHeight, FloorHeight, Octaves);
    }

    private void FixedUpdate()
    {
        tilesToRemove.Clear();
        foreach (Vector2Int pos in GridManager.ActiveTiles.Keys)
        {
            tilesToRemove.Add(pos);
        }

        for (int x = -renderDistance + (int)trackedObject.position.x; x < renderDistance + (int)trackedObject.position.x; x++)
        {
            for (int y = -renderDistance + (int)trackedObject.position.y; y < renderDistance + (int)trackedObject.position.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                
                GridManager.ActiveTiles.TryGetValue(pos, out GameObject obj);
                if (obj == null)
                {
                    GridManager.GenerateTile(x, y); // generate new Tile
                }

                tilesToRemove.Remove(pos);
            }
        }

        for (int i = 0; i < tilesToRemove.Count; i++)
        {
            Vector2Int pos = tilesToRemove[i];
            Destroy(GridManager.ActiveTiles[pos]);
            GridManager.ActiveTiles.Remove(pos);
        }
    }
}