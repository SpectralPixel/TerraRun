using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    private const string gridName = "Grid";

    public static Dictionary<string, Tile> AllTiles;
    public static Dictionary<Vector2Int, Tile> RawWorldTiles;
    public static Dictionary<Vector2Int, Tile> WorldTiles;
    public static Dictionary<Vector2Int, GameObject> ActiveTiles;
    public static Dictionary<int, int> FloorHeights;
    public static GameObject TilePrefab;
    public static GameObject AirTile;
    public static GenerationType GenType;

    public static (int, float[]) InitializeWorld(GenerationType generationType, int seed, List<Tile> allTiles, List<OctaveSetting> octaves, GameObject tilePrefab, bool generateWorld = false, int mapWidth = 0, int mapHeight = 0, int worldFloorHeight = 0)
    {
        GenType = generationType;

        Random.InitState(seed);
        float _seedVariation = 0.03999f;
        float[] _seedVariations = new float[10];

        for (int m = 0; m < _seedVariations.Length; m++)
        {
            float randNum = Random.Range(1 - _seedVariation, 1 + _seedVariation);
            _seedVariations[m] = randNum;
        }

        int _seedXOffset = Mathf.RoundToInt(Random.Range(1 - _seedVariation, 1 + _seedVariation) * 100000);
        _seedVariations[7] = Mathf.RoundToInt(Random.Range(1 - _seedVariation, 1 + _seedVariation) * 100000);
        _seedVariations[8] = Mathf.RoundToInt(Random.Range(1 - _seedVariation, 1 + _seedVariation) * 100000);
        _seedVariations[9] = Mathf.RoundToInt(Random.Range(1 - _seedVariation, 1 + _seedVariation) * 100000);

        AllTiles = new Dictionary<string, Tile>();
        foreach (Tile tile in allTiles)
        {
            AllTiles.Add(tile.TileID, tile);
        }

        GameObject oldGrid = GameObject.Find(gridName);
        if (oldGrid != null) DestroyImmediate(oldGrid);
        new GameObject(gridName);

        RawWorldTiles = new Dictionary<Vector2Int, Tile>();
        WorldTiles = new Dictionary<Vector2Int, Tile>();
        ActiveTiles = new Dictionary<Vector2Int, GameObject>();
        FloorHeights = new Dictionary<int, int>();

        TilePrefab = tilePrefab;

        GameObject _airTile = GameObject.Find("Air Tile");
        if (_airTile == null) AirTile = new GameObject("Air Tile");
        else AirTile = _airTile;

        if (generateWorld)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                GenerateFloorHeight(x, seed, _seedXOffset, worldFloorHeight, octaves, _seedVariations);

                for (int y = 0; y < mapHeight; y++)
                {
                    GenerateTile(new Vector2Int(x, y));
                    Debug.Log(RawWorldTiles[new Vector2Int(x, y)].Value);
                }
            }
            Debug.Log(RawWorldTiles[new Vector2Int(0, 0)].Value);
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    //Tile smoothedTile = SmoothTile(new Vector2Int(x, y));
                    //if (smoothedTile != null) CreateTileObject(new Vector2Int(x, y), smoothedTile);

                    // DELETE ME
                    Debug.Log(RawWorldTiles[new Vector2Int(x, y)].Value);
                    CreateTileObject(new Vector2Int(x, y), RawWorldTiles[new Vector2Int(x, y)]);
                }
            }
        }

        return (_seedXOffset, _seedVariations);
    }

    public static void GenerateFloorHeight(int x)
    {
        GenerateFloorHeight(x, WorldGenerator.Instance.WorldSeed, WorldGenerator.Instance.SeedXOffset, WorldGenerator.Instance.FloorHeight, WorldGenerator.Instance.Octaves, WorldGenerator.Instance.SeedVariations);
    }

    public static void GenerateFloorHeight(int x, int seed, int seedXOffset, int worldFloorHeight, List<OctaveSetting> octaves, float[] seedVariationMultipliers)
    {
        int _neighborTilesToAverage = 2;

        float _floorHeightsAverage = 0;
        for (int i = -_neighborTilesToAverage; i <= _neighborTilesToAverage; i++)
        {
            _floorHeightsAverage += NoiseGenerator.GenerateHeight(seed, x + i + seedXOffset, worldFloorHeight, octaves, seedVariationMultipliers);
        }

        int _floorHeight = Mathf.RoundToInt(_floorHeightsAverage / 3);
        FloorHeights[x] = _floorHeight;
    }

    public static void GenerateTile(Vector2Int pos)
    {
        if (GenType == GenerationType.Terrain)
        {
            RawWorldTiles.TryGetValue(pos, out Tile rawTile);
            if (rawTile == null) // otherwise generate a new object
            {
                {
                    FloorHeights.TryGetValue(pos.x, out int height);
                    if (height == 0) GenerateFloorHeight(pos.x, WorldGenerator.Instance.WorldSeed, WorldGenerator.Instance.SeedXOffset, WorldGenerator.Instance.FloorHeight, WorldGenerator.Instance.Octaves, WorldGenerator.Instance.SeedVariations);

                    if (pos.y <= FloorHeights[pos.x])
                    {
                        if (pos.y == FloorHeights[pos.x]) RawWorldTiles[pos] = AllTiles["GrassTile"];
                        else
                        {
                            float randomNum = Random.Range(0f, 1f);
                            float condition = 0.5f - 0.25f * (FloorHeights[pos.x] - pos.y - 4);

                            if (randomNum > condition) RawWorldTiles[pos] = AllTiles["StoneTile"];
                            else RawWorldTiles[pos] = AllTiles["DirtTile"];
                        }
                    }
                }
            }
        }
        else if (GenType == GenerationType.PerlinNoise)
        {
            Tile newTile = AllTiles["EmptyTile"];
                
            RawWorldTiles[pos] = newTile;
            RawWorldTiles[pos].Value = Mathf.PerlinNoise(pos.x / 10f, pos.y / 10f);
        }
    }

    public static Tile SmoothTile(Vector2Int pos)
    {
        Dictionary<string, int> nearbyTileCounts = new Dictionary<string, int>();
        foreach (string tile in AllTiles.Keys)
        {
            nearbyTileCounts[tile] = 0;
        }

        int radius = 2;
        float newValue = 0;
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                RawWorldTiles.TryGetValue(new Vector2Int(pos.x + x, pos.y + y), out Tile tile);
                if (tile == null) return null;

                nearbyTileCounts[tile.TileID]++;

                newValue += tile.Value;
            }
        }

        float _value = newValue / Mathf.Pow((radius * 2) + 1, 2); // average out all of the values by adding them all together and them dividing them the amount of vales that are being added

        string toprint = ""; //////////////////////////////////////////////////////////////////////
        foreach (KeyValuePair<string, int> kvp in nearbyTileCounts)
        {
            toprint += kvp.Key + " " + kvp.Value.ToString() + " | ";
        }

        Dictionary<string, int> mostPrevalentTileIDs = nearbyTileCounts.OrderByDescending(pair => pair.Value).Take(1).ToDictionary(pair => pair.Key, pair => pair.Value);
        string mostPrevalentTileID = "";

        foreach (string ID in mostPrevalentTileIDs.Keys)
        {
            mostPrevalentTileID = ID;
        }

        //Debug.Log(pos.ToString() + " => " + AllTiles[mostPrevalentTileID].TileID + " => " + toprint);

        WorldTiles[pos] = AllTiles[mostPrevalentTileID];
        //WorldTiles[pos].Value = _value;
        return WorldTiles[pos];
    }

    public static void UpdateGrid(Vector2Int pos, Tile newTile)
    {
        WorldTiles.TryGetValue(pos, out Tile tile);
        if (tile != null) // if the tile exists in the dictionary
        {
            GameObject oldTile = GameObject.Find($"/Grid/Tile {pos.x} {pos.y}");
            if (oldTile != null) Destroy(oldTile); // destroy the old copy
        }

        CreateTileObject(pos, newTile);
    }

    public static void CreateTileObject(Vector2Int pos, Tile tile)
    {
        if (tile.Type != TileType.Gas)
        {
            GameObject newTile = Instantiate(TilePrefab);
            newTile.transform.parent = GameObject.Find("/Grid").transform;

            newTile.name = $"Tile {pos.x} {pos.y}";
            newTile.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f);

            Color newCol = Color.HSVToRGB(0f, 0f, tile.Value);
            //Debug.Log(newCol.ToString());
            if (GenType == GenerationType.PerlinNoise) newTile.GetComponent<SpriteRenderer>().color = newCol;
            else newTile.GetComponent<SpriteRenderer>().sprite = tile.Sprite;

            tile.gameObject = newTile;

            ActiveTiles[pos] = newTile;
        }
        else
        {
            ActiveTiles[pos] = AirTile;
        }
    }

    public static Tile GetTileAtPos(Vector2Int pos)
    {
        if (WorldTiles.TryGetValue(pos, out Tile tile)) return tile;
        return null;
    }

}
