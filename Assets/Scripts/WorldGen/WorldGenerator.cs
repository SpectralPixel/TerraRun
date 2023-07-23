using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    public static WorldGenerator Instance;

    public int WorldSeed;

    [Min(1)]
    public int MapWidth, MapHeight;
    public int FloorHeight;

    public List<Tile> Tiles;
    public List<OctaveSetting> Octaves;

    public GameObject TilePrefab;
    public PhysicsMaterial2D TilePhysicsMaterial;

    [HideInInspector] public int SeedXOffset;
    [HideInInspector] public float[] SeedVariationMultipliers;

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

        if (!Application.isPlaying) GridManager.InitializeWorld(WorldSeed, Tiles, Octaves, TilePrefab, TilePhysicsMaterial, true, MapWidth, MapHeight, FloorHeight);
        else (SeedXOffset, SeedVariationMultipliers) = GridManager.InitializeWorld(WorldSeed, Tiles, Octaves, TilePrefab, TilePhysicsMaterial);

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

        for (int x = -renderDistance - 1 + (int)trackedObject.position.x; x < renderDistance + 1 + (int)trackedObject.position.x; x++)
        {
            for (int y = -renderDistance - 1 + (int)trackedObject.position.y; y < renderDistance + 1 + (int)trackedObject.position.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GridManager.WorldTiles.TryGetValue(pos, out Tile _tile);
                if (_tile == null)
                {
                    GridManager.GenerateTile(pos); // generate new Tile 1 block ahead of render distance
                }
            }
        }

        for (int x = -renderDistance + (int)trackedObject.position.x; x < renderDistance + (int)trackedObject.position.x; x++)
        {
            for (int y = -renderDistance + (int)trackedObject.position.y; y < renderDistance + (int)trackedObject.position.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GridManager.ActiveTiles.TryGetValue(pos, out GameObject _obj);
                if (_obj == null)
                {
                    GridManager.WorldTiles.TryGetValue(pos, out Tile _tile);
                    if (_tile != null) GridManager.CreateTileObject(pos, _tile); // create tile object
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