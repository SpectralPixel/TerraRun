using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class WorldLoader : MonoBehaviour
{

    public static WorldLoader Instance;

    [SerializeField] [Min(1)] private int renderDistance;
    [Tooltip("This value will be added to the render distance")] [SerializeField] [Min(1)] private int tileGenerationDistance;
    [Tooltip("This value will be added to the tile generation distance")] [SerializeField] [Min(1)] private int floorGenerationDistance;

    private List<Vector2Int> tilesToRemove;

    #region Singleton + GameManager subscription
    private void Awake()
    {
        EnsureSingleton();

        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    public void EnsureSingleton()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void OnGameStateChanged(GameState newState)
    {
        CancelInvoke("UpdateWorld");
        if (newState == GameState.GameStart && Application.isPlaying) InvokeRepeating("UpdateWorld", 0f, 0.1f);
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    #endregion

    private void Start()
    {
        tilesToRemove = new List<Vector2Int>();
    }

    public void UpdateWorld()
    {
        tilesToRemove.Clear();
        foreach (Vector2Int pos in GridManager.Instance.ActiveTiles.Keys)
        {
            tilesToRemove.Add(pos);
        }

        // Smooth out floor tiles and generate trees within render distance + tile generation distance + floor generation distance
        for (int x = -renderDistance - tileGenerationDistance - floorGenerationDistance + (int)transform.position.x; x < renderDistance + tileGenerationDistance + floorGenerationDistance + (int)transform.position.x; x++)
        {
            WorldGenerator.Instance.FloorHeights.TryGetValue(x, out int _height);
            if (_height == 0)
            {
                WorldGenerator.Instance.GenerateFloorHeight(x);
            }

            /*
            for (int y = -renderDistance - tileGenerationDistance - floorGenerationDistance + (int)transform.position.y; y < renderDistance + tileGenerationDistance + floorGenerationDistance + (int)transform.position.y; y++)
            {
                Vector2Int _pos = new Vector2Int(x, y);
                WorldGenerator.Instance.WorldTiles.TryGetValue(_pos, out Tile _tile);
                if (_tile == null && !WorldGenerator.Instance.CaveTiles.Contains(_pos)) WorldGenerator.Instance.GenerateCave(_pos);
            }
            */

            Vector2Int _newTreeBasePos = new Vector2Int(x, WorldGenerator.Instance.FloorHeights[x] + 1);
            if (Tree.CanGenerateTree(_newTreeBasePos))
            {
                WorldGenerator.Instance.Trees[_newTreeBasePos] = new Tree(_newTreeBasePos);
            }
        }

        // Generate tiles within render distance + tile generation distance
        for (int x = -renderDistance - tileGenerationDistance + (int)transform.position.x; x < renderDistance + tileGenerationDistance + (int)transform.position.x; x++)
        {
            for (int y = -renderDistance - tileGenerationDistance + (int)transform.position.y; y < renderDistance + tileGenerationDistance + (int)transform.position.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                WorldGenerator.Instance.WorldTiles.TryGetValue(pos, out Tile _tile);
                if (_tile == null && pos.y <= WorldGenerator.Instance.FloorHeights[x])
                {
                    WorldGenerator.Instance.GenerateTile(pos);
                }
            }
        }

        // Render tiles within render distance
        for (int x = -renderDistance + (int)transform.position.x; x < renderDistance + (int)transform.position.x; x++)
        {
            for (int y = -renderDistance + (int)transform.position.y; y < renderDistance + (int)transform.position.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GridManager.Instance.ActiveTiles.TryGetValue(pos, out GameObject _obj);
                if (_obj == null)
                {
                    WorldGenerator.Instance.WorldTiles.TryGetValue(pos, out Tile _tile);
                    if (_tile != null) GridManager.Instance.CreateTileObject(pos, _tile);
                }

                tilesToRemove.Remove(pos);
            }
        }

        for (int i = 0; i < tilesToRemove.Count; i++)
        {
            Vector2Int pos = tilesToRemove[i];
            Destroy(GridManager.Instance.ActiveTiles[pos]);
            GridManager.Instance.ActiveTiles.Remove(pos);
        }
    }

}