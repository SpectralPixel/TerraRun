using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public void Init(Vector2 pos, List<Color> tileColors)
    {
        for (int i = 0; i < tileColors.Count; i++)
        {
            if ((pos.x + pos.y) % tileColors.Count == i)
            {
                GetComponent<SpriteRenderer>().color = tileColors[i];
            }
        }
    }
}
