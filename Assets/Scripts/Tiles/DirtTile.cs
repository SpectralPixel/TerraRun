using UnityEngine;

public class DirtTile : Tile
{

    public override void Init(Vector2Int pos)
    {
        //TileID = 2;

        // NEW BEHAVIOUR ON INIT????
        SetTileColor(pos.x, pos.y);
    }

}
