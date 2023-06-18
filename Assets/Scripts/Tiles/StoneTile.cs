using UnityEngine;

public class StoneTile : Tile
{

    public override void Init(Vector2Int pos)
    {
        //TileID = 3;

        // NEW BEHAVIOUR ON INIT????
        SetTileColor(pos.x, pos.y);
    }

}