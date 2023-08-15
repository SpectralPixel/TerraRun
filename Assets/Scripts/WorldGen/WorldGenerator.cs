using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldGenerator : MonoBehaviour
{

    public static WorldGenerator Instance;

    [HideInInspector] public Dictionary<Vector2Int, Tile> WorldTiles;
    [HideInInspector] public Dictionary<int, int> RawFloorHeights;
    [HideInInspector] public Dictionary<int, int> FloorHeights;
    [HideInInspector] public Dictionary<Vector2Int, Tree> Trees;
    [HideInInspector] public List<Vector2Int> CaveTiles;

    [HideInInspector] public int SeedXOffset;
    [HideInInspector] public float[] SeedVariationMultipliers;

    [HideInInspector] public Vector2Int PreviewStartCoordinate;
    [HideInInspector] public Vector2Int PreviewEndCoordinate;

    [SerializeField] private bool UseRandomSeed;
    public int WorldSeed;
    [Space]
    public List<OctaveSetting> Octaves;
    [Range(0, 10)] public int FloorSmoothing;
    public int FloorHeight;
    public int DirtHeight;
    [Min(0.06f)] public float CaveSize;
    [Range(0f, 1f)] public float CaveSpawnThreshold;
    [Range(0f, 1f)] public float CaveExpansionThreshold;

    [Space]

    public int PreviewYBias;
    [HideInInspector] public Vector2Int PreviewMapSize;
    [HideInInspector] public int PreviewMapResolution;
    [HideInInspector] public bool ConservePreviewXSize;
    [HideInInspector] public bool ConservePreviewYSize;

    #region Singleton
    private void Awake()
    {
        EnsureSingleton();
    }

    public void EnsureSingleton()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
    #endregion

    public async Task InitializeWorld()
    {
        // signed 32-bit integer limits are -2,147,483,647 and 2,147,483,647 (you can't get any higher numbers with Int32, which should always be used for whole numbers)
        if (UseRandomSeed) WorldSeed = Random.Range(-9999, 10000); // floating point errors happen with high seed values

        Random.InitState(WorldSeed);
        float _seedVariation = 0.03999f;

        SeedVariationMultipliers = new float[11];
        SeedVariationMultipliers[0] = Mathf.RoundToInt(Random.Range(1 - _seedVariation, 1 + _seedVariation) * 1000); // formerly known as seedXOffset
        for (int m = 1; m < 8; m++) SeedVariationMultipliers[m] = Random.Range(1 - _seedVariation, 1 + _seedVariation);
        for (int m = 8; m < 11; m++) SeedVariationMultipliers[m] = Random.Range(-1000, 1000);

        WorldTiles = new Dictionary<Vector2Int, Tile>();
        RawFloorHeights = new Dictionary<int, int>();
        FloorHeights = new Dictionary<int, int>();
        Trees = new Dictionary<Vector2Int, Tree>();

        PreviewStartCoordinate = Vector2Int.zero;
        PreviewEndCoordinate = Vector2Int.zero;

        await Task.Yield();
    }

    public void GeneratePreviewWorld()
    {
        int _width = PreviewMapSize.x * (ConservePreviewXSize ? PreviewMapResolution : 1);
        int _height = PreviewMapSize.y * (ConservePreviewYSize ? PreviewMapResolution : 1);

        int _generationStartY = FloorHeight - _height / 2;
        int _generationEndY = FloorHeight + _height / 2;

        PreviewStartCoordinate = new Vector2Int(0, _generationStartY);
        PreviewEndCoordinate = new Vector2Int(_width, _generationEndY);

        for (int x = 0; x < _width; x++)
        {
            GenerateFloorHeight(x);

            for (int y = _generationStartY; y < _generationEndY; y++)
            {
                GenerateCave(new Vector2Int(x, y));
            }

            Vector2Int _newTreeBasePos = new Vector2Int(x, FloorHeights[x] + 1);
            if (Tree.CanGenerateTree(_newTreeBasePos))
            {
                Trees[_newTreeBasePos] = new Tree(_newTreeBasePos);
            }

            for (int y = _generationStartY; y < _generationEndY; y++)
            {
                GenerateTile(new Vector2Int(x, y));
            }
        }
    }

    private void GenerateRawFloorHeight(int x)
    {
        GenerateRawFloorHeight(x, FloorHeight, Octaves, SeedVariationMultipliers);
    }

    private void GenerateRawFloorHeight(int _x, int _worldFloorHeight, List<OctaveSetting> _octaves, float[] _seedVariationMultipliers)
    {
        RawFloorHeights[_x] = Mathf.RoundToInt(NoiseGenerator.GetHeight(_x, _worldFloorHeight, _octaves, _seedVariationMultipliers));
    }

    public void GenerateFloorHeight(int _x)
    {
        if (FloorSmoothing > 0)
        {
            float _floorHeightsAverage = 0;
            for (int i = -FloorSmoothing; i <= FloorSmoothing; i++)
            {
                RawFloorHeights.TryGetValue(_x + i, out int _currentHeight);
                if (_currentHeight == 0) GenerateRawFloorHeight(_x + i);

                _floorHeightsAverage += RawFloorHeights[_x + i];
            }

            int _floorHeight = Mathf.RoundToInt(_floorHeightsAverage / ((FloorSmoothing * 2) + 1));
            FloorHeights[_x] = _floorHeight;
        }
        else FloorHeights[_x] = RawFloorHeights[_x];
    }

    public void GenerateTile(Vector2Int _pos)
    {
        WorldTiles.TryGetValue(_pos, out Tile tile);
        if (tile == null)
        {
            if (_pos.y <= FloorHeights[_pos.x])
            {
                if (_pos.y == FloorHeights[_pos.x]) WorldTiles[_pos] = GameUtilities.AllTiles["Grass"];
                else
                {
                    WorldTiles[_pos] = NoiseGenerator.DetermineTile("Stone", "Dirt", _pos, 0.5f - 0.25f * (FloorHeights[_pos.x] - _pos.y - DirtHeight));
                }
            }
        }
    }

    public void GenerateCave(Vector2Int _pos)
    {
        WorldTiles.TryGetValue(_pos, out Tile tile);
        if (tile == null && !CaveTiles.Contains(_pos))
        {
            if (_pos.y <= FloorHeights[_pos.x])
            {
                float _caveValue = Mathf.PerlinNoise(_pos.x / CaveSize + 300f, _pos.y / CaveSize + 300f);
                if (_caveValue < CaveSpawnThreshold)
                {
                    
                    for (int x = -5; x < 5; x++)
                    {
                        for (int y = -5; y < 5; y++)
                        {
                            if (Mathf.PerlinNoise((_pos.x + x) / CaveSize + 300f, (_pos.y + y) / CaveSize + 300f) < CaveExpansionThreshold)
                            {
                                WorldTiles[_pos + new Vector2Int(x, y)] = GameUtilities.AllTiles["Air"];
                                CaveTiles.Add(_pos + new Vector2Int(x, y));
                            }
                        }
                    }
                }
            }
        }
    }

    public void UpdateGrid(Vector2Int _pos, Tile _newTile)
    {
        WorldTiles.TryGetValue(_pos, out Tile tile);
        if (tile != null) // if the tile exists in the dictionary
        {
            GridManager.Instance.ActiveTiles.TryGetValue(_pos, out GameObject _oldTile);
            if (_oldTile == null) _oldTile = GameObject.Find($"/Grid/Tile {_pos.x} {_pos.y}");
            if (_oldTile != null) Destroy(_oldTile); // destroy the old copys
        }

        WorldTiles[_pos] = _newTile;
        GridManager.Instance.CreateTileObject(_pos, _newTile);

        // check if the neighboring tiles should be/not be a border and change them respectively
        foreach (Vector2Int _direction in GameUtilities.CheckDirections)
        {
            WorldTiles.TryGetValue(_pos + _direction, out Tile _checkedTile);
            GridManager.Instance.ActiveTiles.TryGetValue(_pos + _direction, out GameObject _obj);
            if (_checkedTile != null && _checkedTile.Type == TileType.Solid && _obj != null && _obj != GridManager.Instance.AirTile) // check if the tile exists, is solid, is instantiated and IS NOT THE AIRTILE GODDAMNIT
            {
                GridManager.Instance.RecheckCollider(_pos + _direction, _obj);
            }
        }
    }

}