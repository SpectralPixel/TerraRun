using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{

    private const string gridName = "Grid";

    public static Dictionary<Vector2Int, Tile> WorldTiles;
    public static Dictionary<Vector2Int, GameObject> ActiveTiles;
    public static Dictionary<int, int> RawFloorHeights;
    public static Dictionary<int, int> FloorHeights;
    public static Dictionary<Vector2Int, Tree> Trees;
    public static int FloorSmoothing;
    public static GameObject TilePrefab;
    public static PhysicsMaterial2D TilePhysicsMaterial;
    public static GameObject AirTile;

    public static (int, float[]) InitializeWorld(int seed, List<Tile> allTiles, List<OctaveSetting> octaves, int _floorSmoothing, GameObject tilePrefab, PhysicsMaterial2D _tilePhysicsMaterial, bool generateWorld = false, int mapWidth = 0, int mapHeight = 0, int worldFloorHeight = 0)
    {
        Random.InitState(seed);
        float _seedVariation = 0.03999f;
        float[] _seedVariationMultipliers = new float[10];
        int _seedXOffset = Mathf.RoundToInt(Random.Range(1 - _seedVariation, 1 + _seedVariation) * 1000);

        for (int m = 0; m < 7; m++) _seedVariationMultipliers[m] = Random.Range(1 - _seedVariation, 1 + _seedVariation);
        for (int m = 7; m < 10; m++) _seedVariationMultipliers[m] = Random.Range(-1000, 1000);

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
        RawFloorHeights = new Dictionary<int, int>();
        FloorHeights = new Dictionary<int, int>();
        Trees = new Dictionary<Vector2Int, Tree>();

        FloorSmoothing = _floorSmoothing;

        TilePrefab = tilePrefab;
        TilePhysicsMaterial = _tilePhysicsMaterial;

        GameObject _airTile = GameObject.Find("Air Tile");
        if (_airTile == null) AirTile = new GameObject("Air Tile");
        else AirTile = _airTile;

        if (generateWorld)
        {
            int generationStart = worldFloorHeight - mapHeight / 2;
            int generationEnd = worldFloorHeight + mapHeight / 2;

            for (int x = -FloorSmoothing; x < mapWidth + FloorSmoothing; x++)
            {
                GenerateFloorHeight(x, seed, _seedXOffset, worldFloorHeight, octaves, _seedVariationMultipliers);
            }

            for (int x = 0; x < mapWidth; x++)
            {
                SmoothFloorHeight(x);

                Vector2Int _newTreeBasePos = new Vector2Int(x, FloorHeights[x] + 1);
                if (Tree.CanGenerateTree(_newTreeBasePos))
                {
                    Trees[_newTreeBasePos] = new Tree(_newTreeBasePos);
                }

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
        RawFloorHeights[_x] = Mathf.RoundToInt(NoiseGenerator.GetHeight(_seed, _x + _seedXOffset, _worldFloorHeight, _octaves, _seedVariationMultipliers));
    }

    public static void SmoothFloorHeight(int _x)
    {
        if (FloorSmoothing > 0)
        {
            float _floorHeightsAverage = 0;
            for (int i = -FloorSmoothing; i <= FloorSmoothing; i++)
            {
                RawFloorHeights.TryGetValue(_x + i, out int _currentHeight);
                if (_currentHeight == 0) GenerateFloorHeight(_x + i);
                
                _floorHeightsAverage += RawFloorHeights[_x + i];
            }

            int _floorHeight = Mathf.RoundToInt(_floorHeightsAverage / ((FloorSmoothing * 2) + 1));
            FloorHeights[_x] = _floorHeight;
        }
        else FloorHeights[_x] = RawFloorHeights[_x];
    }

    public static void GenerateTile(Vector2Int _pos)
    {
        WorldTiles.TryGetValue(_pos, out Tile tile);
        if (tile == null) 
        {
            if (_pos.y <= FloorHeights[_pos.x])
            {
                if (_pos.y == FloorHeights[_pos.x]) WorldTiles[_pos] = GameUtilities.AllTiles["GrassTile"];
                else
                {
                    float value = Mathf.PerlinNoise(_pos.x / 2f, _pos.y / 2f) - (Mathf.Cos(_pos.x / 2f + _pos.y / 3f) + Mathf.Sin(_pos.x / 5f + _pos.y / 3f)) / 6f;
                    float condition = 0.5f - 0.25f * (FloorHeights[_pos.x] - _pos.y - 4);
                    if (value > condition) WorldTiles[_pos] = GameUtilities.AllTiles["StoneTile"];
                    else WorldTiles[_pos] = GameUtilities.AllTiles["DirtTile"];
                }
            }
        }
    }

    public static void UpdateGrid(Vector2Int _pos, Tile _newTile)
    {
        WorldTiles.TryGetValue(_pos, out Tile tile);
        if (tile != null) // if the tile exists in the dictionary
        {
            ActiveTiles.TryGetValue(_pos, out GameObject _oldTile);
            if (_oldTile == null) _oldTile = GameObject.Find($"/Grid/Tile {_pos.x} {_pos.y}");
            if (_oldTile != null) Destroy(_oldTile); // destroy the old copys
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
        else if (_borderingEmptyTile && _col == null && WorldTiles[_pos].Type == TileType.Solid) // if we are bordering air and we don't have a collider
        {
            BoxCollider2D _newCol = _tile.AddComponent<BoxCollider2D>();
            _newCol.sharedMaterial = TilePhysicsMaterial;
        }
    }

}
