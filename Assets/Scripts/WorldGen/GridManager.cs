using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    public static GridManager Instance;

    private string GridName = "Grid"; // Constant variables are automatically static
    private string GridPreviewName = "Grid Preview Plane";

    public Dictionary<Vector2Int, GameObject> ActiveTiles;

    public GameObject AirTile;
    [SerializeField] private GameObject TilePrefab;
    [SerializeField] private PhysicsMaterial2D TilePhysicsMaterial;

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

    public async Task InitializeGrid()
    {
        ActiveTiles = new Dictionary<Vector2Int, GameObject>();

        GameObject oldGrid = GameObject.Find(GridName);
        if (oldGrid != null) DestroyImmediate(oldGrid);
        new GameObject(GridName);

        GameObject oldGridPreview = GameObject.Find(GridPreviewName);
        if (oldGridPreview != null) DestroyImmediate(oldGridPreview);

        await Task.Yield();
    }

    public void CreateWorldPreview()
    {
        Vector2Int _gridStart = WorldGenerator.Instance.PreviewStartCoordinate;
        Vector2Int _gridEnd = WorldGenerator.Instance.PreviewEndCoordinate;
        Vector2Int _gridSize = WorldGenerator.Instance.PreviewMapSize;
        int _yBias = WorldGenerator.Instance.PreviewYBias;
        int _resolution = WorldGenerator.Instance.PreviewMapResolution;
        bool _conserveXSize = WorldGenerator.Instance.ConservePreviewXSize;
        bool _conserveYSize = WorldGenerator.Instance.ConservePreviewYSize;

        GameObject oldGridPreview = GameObject.Find(GridPreviewName);
        if (oldGridPreview != null) DestroyImmediate(oldGridPreview);

        GameObject _gridPreview = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _gridPreview.name = GridPreviewName;

        _gridPreview.transform.eulerAngles = new Vector3(90f, 180f, 0f);
        _gridPreview.transform.position = new Vector2(_gridSize.x / 2f, WorldGenerator.Instance.FloorHeight);
        _gridPreview.transform.localScale = new Vector3(_gridSize.x / 10f / (!_conserveXSize ? _resolution : 1), 0f, _gridSize.y / 10f / (!_conserveYSize ? _resolution : 1));

        Texture2D _previewTexture = new Texture2D(16 * _gridSize.x / (!_conserveXSize ? _resolution : 1), 16 * _gridSize.y / (!_conserveYSize ? _resolution : 1));
        for (int x = 0; x < _gridSize.x * _resolution; x++)
        {
            for (int y = 0; y < _gridSize.y * _resolution; y++)
            {
                WorldGenerator.Instance.WorldTiles.TryGetValue(new Vector2Int(_gridStart.x + x, _gridStart.y + y + _yBias), out Tile _tile);
                if (_tile != null && _tile.Type != TileType.Gas)
                {
                    Sprite _tileSprite = GameUtilities.GetSprite(_tile.TileID);
                    for (int a = 16; a >= 0; a -= _resolution)
                    {
                        for (int b = 16; b >= 0; b -= _resolution)
                        {
                            _previewTexture.SetPixel(x * 16 / _resolution + a / _resolution, y * 16 / _resolution + b / _resolution, _tileSprite.texture.GetPixel(a, b));
                        }
                    }
                }
            }
        }
        _previewTexture.Apply();

        Material _previewMaterial = Resources.Load<Material>("Unlit Material");
        _previewMaterial.mainTexture = _previewTexture;
        _gridPreview.GetComponent<MeshRenderer>().material = _previewMaterial;
    }

    public void CreateTileObject(Vector2Int _pos, Tile _tile)
    {
        if (_tile.Type != TileType.Gas)
        {
            GameObject _newTile = Instantiate(TilePrefab);
            _newTile.transform.parent = GameObject.Find("/Grid").transform;

            _newTile.name = $"Tile {_pos.x} {_pos.y}";
            _newTile.transform.position = new Vector3(_pos.x + 0.5f, _pos.y + 0.5f);

            _newTile.GetComponent<SpriteRenderer>().sprite = GameUtilities.GetSprite(_tile.TileID);

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

    public void RecheckCollider(Vector2Int _pos, GameObject _tile) // if the tile is bordering air, add a collider, otherwise remove it
    {
        bool _borderingEmptyTile = false;
        foreach (Vector2Int _direction in GameUtilities.CheckDirections)
        {
            WorldGenerator.Instance.WorldTiles.TryGetValue(_pos + _direction, out Tile _checkedTile);
            if (_checkedTile == null || _checkedTile.Type != TileType.Solid) _borderingEmptyTile = true;
        }

        BoxCollider2D _col = _tile.GetComponent<BoxCollider2D>();

        if (!_borderingEmptyTile) Destroy(_col);
        else if (_borderingEmptyTile && _col == null && WorldGenerator.Instance.WorldTiles[_pos].Type == TileType.Solid) // if we are bordering air and we don't have a collider
        {
            BoxCollider2D _newCol = _tile.AddComponent<BoxCollider2D>();
            _newCol.sharedMaterial = TilePhysicsMaterial;
        }
    }

}
