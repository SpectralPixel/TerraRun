using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtilities
{

    public static Dictionary<string, Sprite> AllSprites;
    public static Dictionary<string, Tile> AllTiles;
    public static Dictionary<string, Item> AllItems;
    public static Dictionary<string, string> ItemDropConversions; // when an item is dropped, the player gets another item instead (e.g. player breaks generated wood, gets placeable wood)
    public static List<Vector2Int> CheckDirections;

    public static void InitializeUtilities()
    {
        Sprite[] _allSprites = Resources.LoadAll<Sprite>("Sprites");
        AllSprites = new Dictionary<string, Sprite>();
        foreach (Sprite _sprite in _allSprites)
        {
            AllSprites.Add(_sprite.name, _sprite);
        }

        DropConversion[] _allConversions = Resources.LoadAll<DropConversion>("Item Drop Conversions");
        ItemDropConversions = new Dictionary<string, string>();
        List<string> _unobtainibleItems = new List<string>(); // stores all of the itemIDs that can't be obtained due to cenversions
        foreach (DropConversion _conversion in _allConversions)
        {
            ItemDropConversions.Add(_conversion.brokenTileID, _conversion.droppedItemID);
            _unobtainibleItems.Add(_conversion.brokenTileID);
        }

        Tile[] _allTiles = Resources.LoadAll<Tile>("Tiles"); // load all the tiles directly from the files
        AllTiles = new Dictionary<string, Tile>();
        foreach (Tile _tile in _allTiles)
        {
            AllTiles.Add(_tile.TileID, _tile);
        }

        Item[] _allItems = Resources.LoadAll<Item>("Items"); // load all the tiles directly from the files
        AllItems = new Dictionary<string, Item>();
        foreach (Item _item in _allItems)
        {
            AllItems.Add(_item.ItemID, _item);
        }
        foreach (Tile _tile in _allTiles) // create items for all of the tiles, since tile items should not be created in the editor
        {
            if (!_unobtainibleItems.Contains(_tile.TileID) && _tile.Type == TileType.Solid)
            {
                Item _item = (Item)ScriptableObject.CreateInstance("Item");

                _item.Type = ItemType.Tile;
                _item.ItemID = _tile.TileID;

                AllItems.Add(_item.ItemID, _item);
            }
        }

        CheckDirections = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    }

    public static Item TileToItem(Tile _tile)
    {
        ItemDropConversions.TryGetValue(_tile.TileID, out string _newID);
        if (_newID != null) return AllItems[_newID];
        return AllItems[_tile.TileID];
    }

    public static Tile ItemToTile(Item _item)
    {
        return AllTiles[_item.ItemID];
    }

    public static Sprite GetSprite(string ID)
    {
        return AllSprites[ID];
    }

}
