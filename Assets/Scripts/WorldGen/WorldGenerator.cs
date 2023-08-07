using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldGenerator : MonoBehaviour
{

    public static WorldGenerator Instance;

    public bool UseRandomSeed;
    public int WorldSeed;

    [Min(1)] public int MapWidth, MapHeight, FloorHeight;
    [Range(0, 10)] public int FloorSmoothing;

    public List<OctaveSetting> Octaves;

    public GameObject TilePrefab;
    public PhysicsMaterial2D TilePhysicsMaterial;

    [HideInInspector] public int SeedXOffset;
    [HideInInspector] public float[] SeedVariationMultipliers;

    [Space]
    [SerializeField] private Transform trackedObject;
    [SerializeField] [Min(1)] private int renderDistance;
    [Tooltip("This value will be added to the render distance")] [SerializeField] [Min(1)] private int tileGenerationDistance;
    [Tooltip("This value will be added to the tile generation distance")] [SerializeField] [Min(1)] private int floorGenerationDistance;

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

        // signed 32-bit integer limits are -2,147,483,647 and 2,147,483,647 (you can't get any higher numbers with Int32, which should always be used for whole numbers)
        if (UseRandomSeed) WorldSeed = UnityEngine.Random.Range(-9999, 10000); // floating point errors happen with high seed values

        if (!Application.isPlaying) GridManager.InitializeWorld(WorldSeed, Octaves, FloorSmoothing, TilePrefab, TilePhysicsMaterial, true, MapWidth, MapHeight, FloorHeight);
        else (SeedXOffset, SeedVariationMultipliers) = GridManager.InitializeWorld(WorldSeed, Octaves, FloorSmoothing, TilePrefab, TilePhysicsMaterial);

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

        // Generate floor heights within render distance + tile generation distance + floor generation distance
        for (int x = -renderDistance - tileGenerationDistance - floorGenerationDistance + (int)trackedObject.position.x; x < renderDistance + tileGenerationDistance + floorGenerationDistance + (int)trackedObject.position.x; x++)
        {
            GridManager.RawFloorHeights.TryGetValue(x, out int _height);
            if (_height == 0)
            {
                GridManager.GenerateFloorHeight(x);
            }
        }

        // Smooth out floor tiles and generate trees within render distance + tile generation distance + floor generation distance - floor smoothing radius
        for (int x = -renderDistance - tileGenerationDistance - floorGenerationDistance + FloorSmoothing + (int)trackedObject.position.x; x < renderDistance + tileGenerationDistance + floorGenerationDistance - FloorSmoothing + (int)trackedObject.position.x; x++)
        {
            GridManager.FloorHeights.TryGetValue(x, out int _height);
            if (_height == 0)
            {
                GridManager.SmoothFloorHeight(x);
            }

            Vector2Int _newTreeBasePos = new Vector2Int(x, GridManager.FloorHeights[x] + 1);
            if (Tree.CanGenerateTree(_newTreeBasePos))
            {
                GridManager.Trees[_newTreeBasePos] = new Tree(_newTreeBasePos);
            }
        }

        // Generate tiles within render distance + tile generation distance
        for (int x = -renderDistance - tileGenerationDistance + (int)trackedObject.position.x; x < renderDistance + tileGenerationDistance + (int)trackedObject.position.x; x++)
        {
            for (int y = -renderDistance - tileGenerationDistance + (int)trackedObject.position.y; y < renderDistance + tileGenerationDistance + (int)trackedObject.position.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GridManager.WorldTiles.TryGetValue(pos, out Tile _tile);
                if (_tile == null && pos.y <= GridManager.FloorHeights[x])
                {
                    GridManager.GenerateTile(pos);
                }
            }
        }

        // Render tiles within render distance
        for (int x = -renderDistance + (int)trackedObject.position.x; x < renderDistance + (int)trackedObject.position.x; x++)
        {
            for (int y = -renderDistance + (int)trackedObject.position.y; y < renderDistance + (int)trackedObject.position.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GridManager.ActiveTiles.TryGetValue(pos, out GameObject _obj);
                if (_obj == null)
                {
                    GridManager.WorldTiles.TryGetValue(pos, out Tile _tile);
                    if (_tile != null) GridManager.CreateTileObject(pos, _tile);
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