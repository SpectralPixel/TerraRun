using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    private const string gridName = "Grid";

    public static Dictionary<string, Tile> AllTiles;
    public static Dictionary<Vector2Int, Tile> WorldTiles;
    public static Dictionary<Vector2Int, GameObject> ActiveTiles;
    public static Dictionary<int, int> FloorHeights;
    public static PhysicsMaterial2D TileMaterial;

    public static (int, float[]) InitializeWorld(int seed, int width, int height, int worldFloorHeight, List<Tile> allTiles, List<OctaveSetting> octaves, PhysicsMaterial2D tileMaterial)
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

        AllTiles = new Dictionary<string, Tile>();
        foreach (Tile tile in allTiles)
        {
            AllTiles.Add(tile.TileID, tile);
        }

        GameObject oldGrid = GameObject.Find(gridName);
        if (oldGrid != null) DestroyImmediate(oldGrid);

        new GameObject(gridName);

        WorldTiles = new Dictionary<Vector2Int, Tile>();
        ActiveTiles = new Dictionary<Vector2Int, GameObject>();
        FloorHeights = new Dictionary<int, int>();
        TileMaterial = tileMaterial;
        for (int x = 0; x < width; x++)
        {
            GenerateFloorHeight(x, seed, _seedXOffset, worldFloorHeight, octaves, _seedVariationMultipliers);

            for (int y = 0; y < height; y++)
            {
                GenerateTile(new Vector2Int(x, y));
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

    public static void GenerateTile(Vector2Int pos)
    {
        WorldTiles.TryGetValue(pos, out Tile tile);
        if (tile != null) CreateTileObject(pos, tile);
        else
        {
            FloorHeights.TryGetValue(pos.x, out int height);
            if (height == 0) GenerateFloorHeight(pos.x, WorldGenerator.Instance.WorldSeed, WorldGenerator.Instance.SeedXOffset, WorldGenerator.Instance.FloorHeight, WorldGenerator.Instance.Octaves, WorldGenerator.Instance.SeedVariationMultipliers);

            if (pos.y <= FloorHeights[pos.x])
            {
                if (pos.y == FloorHeights[pos.x]) CreateTileObject(pos, AllTiles["GrassTile"]);
                else
                {
                    float randomNum = Random.Range(0f, 1f);
                    float condition = 0.5f - 0.25f * (FloorHeights[pos.x] - pos.y - 4);
                    if (randomNum > condition) CreateTileObject(pos, AllTiles["StoneTile"]);
                    else CreateTileObject(pos, AllTiles["DirtTile"]);
                }
            }
        }
    }

    public static void UpdateGrid(Vector2Int pos, Tile newTile = null)
    {
        WorldTiles.TryGetValue(pos, out Tile tile);
        if (tile != null) // if the tile exists in the dictionary
        {
            GameObject oldTile = GameObject.Find($"/Grid/Tile {pos.x} {pos.y}");
            if (oldTile != null) Destroy(oldTile); // destroy the old copy
        }

        if (newTile != null) CreateTileObject(pos, newTile); // and replace it with a new copy (if you want to)
    }

    public static void CreateTileObject(Vector2Int pos, Tile tile)
    {
        GameObject newTile = new GameObject();
        newTile.transform.parent = GameObject.Find("/Grid").transform;
        
        newTile.name = $"Tile {pos.x} {pos.y}";
        newTile.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f);
        newTile.layer = 10;

        newTile.AddComponent<SpriteRenderer>().sprite = tile.Sprite;
        newTile.AddComponent<BoxCollider2D>().sharedMaterial = TileMaterial;

        tile.gameObject = newTile;

        WorldTiles[pos] = tile;
        ActiveTiles[pos] = newTile;
    }

    public static Tile GetTileAtPos(Vector2Int pos)
    {
        if (WorldTiles.TryGetValue(pos, out Tile tile)) return tile;
        return null;
    }

}
