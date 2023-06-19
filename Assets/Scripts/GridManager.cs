using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    private const string gridName = "Grid";

    public static List<Tile> AllTiles;
    public static Dictionary<Vector2Int, Tile> WorldTiles;

    public static void GenerateGrid(int seed, int width, int height, int worldFloorHeight, List<OctaveSetting> octaves)
    {
        Random.InitState(seed);
        float _seedVariation = 0.03999f;
        float[] _seedVariationMultiplier = new float[7];

        for (int m = 0; m < _seedVariationMultiplier.Length; m++)
        {
            _seedVariationMultiplier[m] = Random.Range(1 - _seedVariation, 1 + _seedVariation);
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
        for (int x = 0; x < width; x++)
        {
            int _neighborTilesToAverage = 1;

            float _floorHeightsAverage = 0;
            for (int i = -_neighborTilesToAverage; i <= _neighborTilesToAverage; i++)
            {
                _floorHeightsAverage += NoiseGenerator.GetHeight(seed, x + i, worldFloorHeight, octaves, _seedVariationMultiplier);
            }

            int _floorHeight = Mathf.RoundToInt(_floorHeightsAverage / 3);
            for (int y = 0; y < height; y++)
            {
                if (y <= _floorHeight)
                {
                    if (y == _floorHeight) CreateTileObject(x, y, AllTiles[1]);
                    //else if (y > floorHeight - 3) CreateTileObject(x, y, AllTiles[2]);
                    else
                    {
                        float randomNum = Random.Range(0f, 1f);
                        float condition = 0.5f - 0.25f * (_floorHeight - y - 4);
                        if (randomNum > condition) CreateTileObject(x, y, AllTiles[3]);
                        else CreateTileObject(x, y, AllTiles[2]);
                    }
                }
            }
        }
    }

    public static void UpdateGrid(Vector2Int pos, Tile tile)
    {
        Destroy(GameObject.Find($"/Grid/Tile {pos.x} {pos.y}"));
        CreateTileObject(pos.x, pos.y, tile);
    }

    public static void CreateTileObject(int x, int y, Tile tile)
    {
        Tile newTile = Instantiate(tile);
        newTile.transform.parent = GameObject.Find("/Grid").transform;
        
        newTile.name = $"Tile {x} {y}";
        newTile.transform.position = new Vector3(x + 0.5f, y + 0.5f);

        WorldTiles[new Vector2Int(x, y)] = newTile;

        newTile.Init(new Vector2Int(x, y));
    }

    public static Tile GetTileAtPos(Vector2Int pos)
    {
        if (WorldTiles.TryGetValue(pos, out Tile tile)) return tile;
        return null;
    }

}
