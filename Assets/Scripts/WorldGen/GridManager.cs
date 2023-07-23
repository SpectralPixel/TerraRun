using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    private const string gridName = "Grid";

    public static Dictionary<Vector2Int, Tile> WorldTiles;
    public static Dictionary<Vector2Int, GameObject> ActiveTiles;
    public static Dictionary<int, int> FloorHeights;
    public static GameObject TilePrefab;
    public static PhysicsMaterial2D TilePhysicsMaterial;
    public static GameObject AirTile;

    public static (int, float[]) InitializeWorld(int seed, List<Tile> allTiles, List<OctaveSetting> octaves, GameObject tilePrefab, PhysicsMaterial2D _tilePhysicsMaterial, bool generateWorld = false, int mapWidth = 0, int mapHeight = 0, int worldFloorHeight = 0)
    {
        Random.InitState(seed);
        float _seedVariation = 0.03999f;
        float[] _seedVariationMultipliers = new float[10];
        int _seedXOffset = Mathf.RoundToInt(Random.Range(1 - _seedVariation, 1 + _seedVariation) * 100000);

        for (int m = 0; m < 7; m++) _seedVariationMultipliers[m] = Random.Range(1 - _seedVariation, 1 + _seedVariation);
        for (int m = 7; m < 10; m++) _seedVariationMultipliers[m] = Random.Range(-10000, 10000);

        GameUtilities.AllTiles = new Dictionary<string, Tile>();
        foreach (Tile tile in allTiles)
        {
            GameUtilities.AllTiles.Add(tile.TileID, tile);
        }

        GameObject oldGrid = GameObject.Find(gridName);
        if (oldGrid != null) DestroyImmediate(oldGrid);
        new GameObject(gridName);

        WorldTiles = new Dictionary<Vector2Int, Tile>();
        ActiveTiles = new Dictionary<Vector2Int, GameObject>();
        FloorHeights = new Dictionary<int, int>();

        TilePrefab = tilePrefab;
        TilePhysicsMaterial = _tilePhysicsMaterial;

        GameObject _airTile = GameObject.Find("Air Tile");
        if (_airTile == null) AirTile = new GameObject("Air Tile");
        else AirTile = _airTile;

        if (generateWorld)
        {
            int generationStart = worldFloorHeight - mapHeight / 2;
            int generationEnd = worldFloorHeight + mapHeight / 2;

            for (int x = 0; x < mapWidth; x++)
            {
                GenerateFloorHeight(x, seed, _seedXOffset, worldFloorHeight, octaves, _seedVariationMultipliers);

                for (int y = generationStart; y < generationEnd; y++)
                {
                    GenerateTile(new Vector2Int(x, y));
                }
            }

            foreach (KeyValuePair<Vector2Int, Tile> tile in WorldTiles)
            {
                CreateTileObject(tile.Key, tile.Value);
            }
        }

        return (_seedXOffset, _seedVariationMultipliers);
    }

    public static void GenerateFloorHeight(int x)
    {
        GenerateFloorHeight(x, WorldGenerator.Instance.WorldSeed, WorldGenerator.Instance.SeedXOffset, WorldGenerator.Instance.FloorHeight, WorldGenerator.Instance.Octaves, WorldGenerator.Instance.SeedVariationMultipliers);
    }

    public static void GenerateFloorHeight(int _x, int _seed, int _seedXOffset, int _worldFloorHeight, List<OctaveSetting> _octaves, float[] _seedVariationMultipliers)
    {
        int _neighborTilesToAverage = 1;

        float _floorHeightsAverage = 0;
        for (int i = -_neighborTilesToAverage; i <= _neighborTilesToAverage; i++)
        {
            _floorHeightsAverage += NoiseGenerator.GetHeight(_seed, _x + i + _seedXOffset, _worldFloorHeight, _octaves, _seedVariationMultipliers);
        }

        int _floorHeight = Mathf.RoundToInt(_floorHeightsAverage / ((_neighborTilesToAverage * 2) + 1));
        FloorHeights[_x] = _floorHeight;
    }

    public static void GenerateTile(Vector2Int pos)
    {
        WorldTiles.TryGetValue(pos, out Tile tile);
        if (tile == null) 
        {
            FloorHeights.TryGetValue(pos.x, out int height);
            if (height == 0) GenerateFloorHeight(pos.x, WorldGenerator.Instance.WorldSeed, WorldGenerator.Instance.SeedXOffset, WorldGenerator.Instance.FloorHeight, WorldGenerator.Instance.Octaves, WorldGenerator.Instance.SeedVariationMultipliers);

            if (pos.y <= FloorHeights[pos.x])
            {
                if (pos.y == FloorHeights[pos.x]) WorldTiles[pos] = GameUtilities.AllTiles["GrassTile"];// CreateTileObject(pos, GameUtilities.AllTiles["GrassTile"]);
                else
                {
                    float randomNum = Random.Range(0f, 1f);
                    float condition = 0.5f - 0.25f * (FloorHeights[pos.x] - pos.y - 4);
                    if (randomNum > condition) WorldTiles[pos] = GameUtilities.AllTiles["StoneTile"];//CreateTileObject(pos, GameUtilities.AllTiles["StoneTile"]);
                    else WorldTiles[pos] = GameUtilities.AllTiles["DirtTile"];//CreateTileObject(pos, GameUtilities.AllTiles["DirtTile"]);
                }
            }
        }
    }

    public static void UpdateGrid(Vector2Int _pos, Tile _newTile)
    {
        WorldTiles.TryGetValue(_pos, out Tile tile);
        if (tile != null) // if the tile exists in the dictionary
        {
            GameObject _oldTile = GameObject.Find($"/Grid/Tile {_pos.x} {_pos.y}");
            if (_oldTile != null) Destroy(_oldTile); // destroy the old copy
        }

        WorldTiles[_pos] = _newTile;
        CreateTileObject(_pos, _newTile);

        // check if the neighboring tiles should be/not be a border and change them respectively
        foreach (Vector2Int _direction in GameUtilities.CheckDirections)
        {
            WorldTiles.TryGetValue(_pos + _direction, out Tile _checkedTile);
            ActiveTiles.TryGetValue(_pos + _direction, out GameObject _obj);
            if (_checkedTile != null && _checkedTile.Type == TileType.Solid && _obj != null && _obj != AirTile) // check if the tile exists, is solid, is instantiated and IS NOT THE AIRTILE GODDAMNIT
            {
                RecheckCollider(_pos + _direction, _obj);
            }
        }
    }

    public static void CreateTileObject(Vector2Int _pos, Tile _tile)
    {
        if (_tile.Type != TileType.Gas)
        {
            GameObject _newTile = Instantiate(TilePrefab);
            _newTile.transform.parent = GameObject.Find("/Grid").transform;

            _newTile.name = $"Tile {_pos.x} {_pos.y}";
            _newTile.transform.position = new Vector3(_pos.x + 0.5f, _pos.y + 0.5f);

            _newTile.GetComponent<SpriteRenderer>().sprite = _tile.Sprite;

            if (Application.isPlaying) RecheckCollider(_pos, _newTile);
            else DestroyImmediate(_newTile.GetComponent<BoxCollider2D>());

            _tile.gameObject = _newTile;

            ActiveTiles[_pos] = _newTile;
        }
        else
        {
            ActiveTiles[_pos] = AirTile;
        }
    }

    public static void RecheckCollider(Vector2Int _pos, GameObject _tile) // if the tile is bordering air, add a collider, otherwise remove it
    {
        bool _borderingEmptyTile = false;
        foreach (Vector2Int _direction in GameUtilities.CheckDirections)
        {
            WorldTiles.TryGetValue(_pos + _direction, out Tile _checkedTile);
            if (_checkedTile == null || _checkedTile.Type != TileType.Solid) _borderingEmptyTile = true;
        }

        BoxCollider2D _col = _tile.GetComponent<BoxCollider2D>();

        if (!_borderingEmptyTile) Destroy(_col);
        else if (_borderingEmptyTile && _col == null) // if we are bordering air and we don't have a collider
        {
            BoxCollider2D _newCol = _tile.AddComponent<BoxCollider2D>();
            _newCol.sharedMaterial = TilePhysicsMaterial;
        }
    }

}
