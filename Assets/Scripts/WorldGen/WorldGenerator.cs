using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    public static WorldGenerator Instance;

    public GenerationType GenerationType;

    public int WorldSeed;

    [Min(1)]
    public int MapWidth, MapHeight;
    public int FloorHeight;

    public List<Tile> Tiles;
    public List<OctaveSetting> Octaves;

    public GameObject TilePrefab;

    [HideInInspector] public int SeedXOffset;
    [HideInInspector] public float[] SeedVariations;

    [Space]
    [SerializeField] private Transform trackedObject;
    [SerializeField] private int renderDistance;

    private List<Vector2Int> tilesToRemove;

    private void Awake()
    {
        Instance = this;
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.GeneratingTerrain) GenerateWorld();
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Start()
    {
        tilesToRemove = new List<Vector2Int>();
    }

    public void GenerateWorld()
    {
        CancelInvoke("UpdateWorld");

        if (!Application.isPlaying) GridManager.InitializeWorld(GenerationType, WorldSeed, Tiles, Octaves, TilePrefab, true, MapWidth, MapHeight, FloorHeight);
        else (SeedXOffset, SeedVariations) = GridManager.InitializeWorld(GenerationType, WorldSeed, Tiles, Octaves, TilePrefab);

        if (Application.isPlaying)
        {
            InvokeRepeating("UpdateWorld", 0f, 0.1f);
            GameManager.instance.UpdateGameState(GameManager.GameState.GameStart);
        }
    }

    private void UpdateWorld()
    {
        tilesToRemove.Clear();
        foreach (Vector2Int pos in GridManager.ActiveTiles.Keys)
        {
            tilesToRemove.Add(pos);
        }

        for (int x = -renderDistance + (int)trackedObject.position.x - 2; x < renderDistance + (int)trackedObject.position.x + 2; x++)
        {
            for (int y = -renderDistance + (int)trackedObject.position.y - 2; y < renderDistance + (int)trackedObject.position.y + 2; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GridManager.RawWorldTiles.TryGetValue(pos, out Tile tile);
                if (tile == null)
                {
                    GridManager.GenerateTile(pos); // generate new Tile
                }
            }
        }

        for (int x = -renderDistance + (int)trackedObject.position.x; x < renderDistance + (int)trackedObject.position.x; x++)
        {
            for (int y = -renderDistance + (int)trackedObject.position.y; y < renderDistance + (int)trackedObject.position.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GridManager.WorldTiles.TryGetValue(pos, out Tile tile);
                if (tile == null)
                {
                    Tile smoothedTile = GridManager.SmoothTile(pos);
                    if (smoothedTile != null) GridManager.CreateTileObject(pos, smoothedTile);
                }
                else GridManager.CreateTileObject(pos, tile);

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

public enum GenerationType
{
    Terrain,
    PerlinNoise
}