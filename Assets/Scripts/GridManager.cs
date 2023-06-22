using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    private const string gridName = "Grid";

    public static List<Tile> AllTiles;
    public static Dictionary<Vector2Int, Tile> WorldTiles;
    public static Dictionary<Vector2Int, GameObject> ActiveTiles;
    public static Dictionary<int, int> FloorHeights;

    public static (int, float[]) InitializeWorld(int seed, int width, int height, int worldFloorHeight, List<OctaveSetting> octaves)
    {
        Random.InitState(seed);
        float _seedVariation = 0.03999f;
        float[] _seedVariationMultipliers = new float[7];
        int _seedXOffset = Mathf.RoundToInt(Random.Range(1 - _seedVariation, 1 + _seedVariation) * 100000);

        for (int m = 0; m < _seedVariationMultipliers.Length; m++)
        {
            float randNum = Random.Range(1 - _seedVariation, 1 + _seedVariation);
            _seedVariationMultipliers[m] = randNum;
        }

        AllTiles = new List<Tile>
        {
            (Tile)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tiles/0 Air Tile.prefab", typeof(Tile)),
            (Tile)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tiles/1 Grass Tile.prefab", typeof(Tile)),
            (Tile)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tiles/2 Dirt Tile.prefab", typeof(Tile)),
            (Tile)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tiles/3 Stone Tile.prefab", typeof(Tile)),
        };

        GameObject oldGrid = GameObject.Find(gridName);
        if (oldGrid != null) DestroyImmediate(oldGrid);

        new GameObject(gridName);

        WorldTiles = new Dictionary<Vector2Int, Tile>();
        ActiveTiles = new Dictionary<Vector2Int, GameObject>();
        FloorHeights = new Dictionary<int, int>();
        for (int x = 0; x < width; x++)
        {
            GenerateFloorHeight(x, seed, _seedXOffset, worldFloorHeight, octaves, _seedVariationMultipliers);

            for (int y = 0; y < height; y++)
            {
                GenerateTile(x, y);
            }
        }

        return (_seedXOffset, _seedVariationMultipliers);
    }

    public static void GenerateFloorHeight(int x, int seed, int seedXOffset, int worldFloorHeight, List<OctaveSetting> octaves, float[] seedVariationMultipliers)
    {
        int _neighborTilesToAverage = 1;

        float _floorHeightsAverage = 0;
        for (int i = -_neighborTilesToAverage; i <= _neighborTilesToAverage; i++)
        {
            _floorHeightsAverage += NoiseGenerator.GetHeight(seed, x + i + seedXOffset, worldFloorHeight, octaves, seedVariationMultipliers);
        }

        int _floorHeight = Mathf.RoundToInt(_floorHeightsAverage / 3);
        FloorHeights[x] = _floorHeight;
    }

    public static void GenerateTile(int x, int y)
    {
        FloorHeights.TryGetValue(x, out int height);
        if (height == 0) GenerateFloorHeight(x, WorldGenerator.Instance.WorldSeed, WorldGenerator.Instance.SeedXOffset, WorldGenerator.Instance.FloorHeight, WorldGenerator.Instance.Octaves, WorldGenerator.Instance.SeedVariationMultipliers);

        if (y <= FloorHeights[x])
        {
            if (y == FloorHeights[x]) CreateTileObject(x, y, AllTiles[1]);
            else
            {
                float randomNum = Random.Range(0f, 1f);
                float condition = 0.5f - 0.25f * (FloorHeights[x] - y - 4);
                if (randomNum > condition) CreateTileObject(x, y, AllTiles[3]);
                else CreateTileObject(x, y, AllTiles[2]);
            }
        }
    }

    public static void UpdateGrid(int x, int y, Tile tile)
    {
        GameObject oldTile = GameObject.Find($"/Grid/Tile {x} {y}");
        if (oldTile != null) Destroy(oldTile);

        CreateTileObject(x, y, tile);
    }

    public static void CreateTileObject(int x, int y, Tile tile)
    {
        Tile newTile = Instantiate(tile);
        newTile.transform.parent = GameObject.Find("/Grid").transform;
        
        newTile.name = $"Tile {x} {y}";
        newTile.transform.position = new Vector3(x + 0.5f, y + 0.5f);

        WorldTiles[new Vector2Int(x, y)] = newTile;
        ActiveTiles[new Vector2Int(x, y)] = newTile.gameObject;

        newTile.Init(new Vector2Int(x, y));
    }

    public static Tile GetTileAtPos(Vector2Int pos)
    {
        if (WorldTiles.TryGetValue(pos, out Tile tile)) return tile;
        return null;
    }

}
