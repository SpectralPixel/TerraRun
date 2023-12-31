using System.Collections.Generic; 
using UnityEngine;

public class Tree
{
    public Dictionary<Vector2Int, Tile> Tiles;

    public int TreeHeight;

    public Vector2Int BasePosition;

    // get tiles in a function containing the structure of the treetop

    public Tree(Vector2Int _basePosition)
    {
        BasePosition = _basePosition;

        if (TreeHeight == 0) TreeHeight = Mathf.RoundToInt((Mathf.Sin(BasePosition.x * 69f) + 1) / 2f * 3f) + 3; // Gets a number in the range 3 - 6

        Vector2Int _nearbyTreePos = CheckForNearbyTrees();

        if (_nearbyTreePos != Vector2Int.zero)
        {
            WorldGenerator.Instance.UpdateGrid(BasePosition, GameUtilities.AllTiles["Air"]);
            return;
        }

        Tiles = new Dictionary<Vector2Int, Tile>()
        {
            { new Vector2Int(-2, TreeHeight), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(-1, TreeHeight), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(0, TreeHeight), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(1, TreeHeight), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(2, TreeHeight), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(-2, TreeHeight + 1), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(-1, TreeHeight + 1), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(0, TreeHeight + 1), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(1, TreeHeight + 1), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(2, TreeHeight + 1), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(-1, TreeHeight + 2), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(0, TreeHeight + 2), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(1, TreeHeight + 2), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(-1, TreeHeight + 3), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(0, TreeHeight + 3), GameUtilities.AllTiles["Leaves"] },
            { new Vector2Int(1, TreeHeight + 3), GameUtilities.AllTiles["Leaves"] }
        };

        for (int i = 0; i < TreeHeight; i++)
        {
            Tiles[new Vector2Int(0, i)] = GameUtilities.AllTiles["Wood"];
        }

        foreach (KeyValuePair<Vector2Int, Tile> _currentTile in Tiles)
        {
            Tile _tile = _currentTile.Value;
            WorldGenerator.Instance.WorldTiles[BasePosition + _currentTile.Key] = _tile;
        }
    }

    public void DestroyTree()
    {
        foreach (KeyValuePair<Vector2Int, Tile> tile in Tiles)
        {
            WorldGenerator.Instance.UpdateGrid(tile.Key + BasePosition, GameUtilities.AllTiles["Air"]);
        }

        WorldGenerator.Instance.Trees.Remove(BasePosition);
    }

    public Vector2Int CheckForNearbyTrees()
    {
        for (int x = -4; x <= 4; x++)
        {
            for (int y = -TreeHeight - 2; y <= TreeHeight + 2; y++)
            {
                Vector2Int _pos = new Vector2Int(BasePosition.x + x, BasePosition.y + y);

                WorldGenerator.Instance.Trees.TryGetValue(_pos, out Tree _tree);
                if (_tree != null) return _pos;
            }
        }

        return Vector2Int.zero;
    }

    public static bool CanGenerateTree(Vector2Int _pos)
    {
        float value = Mathf.PerlinNoise(_pos.x / 2f + 100, _pos.y / 2f + 100) - (Mathf.Cos(_pos.x / 2f + _pos.y / 3f) + Mathf.Sin(_pos.x / 5f + _pos.y / 3f)) / 6f;
        float condition = 0.6f;

        WorldGenerator.Instance.WorldTiles.TryGetValue(_pos, out Tile _tile);
        WorldGenerator.Instance.WorldTiles.TryGetValue(_pos - new Vector2Int(0, -1), out Tile _tileBelow);

        return value > condition && _tile == null && !(_tileBelow != null && _tileBelow.Type == TileType.Gas);
    }

}