using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{

    public static float GetHeight(int x, int baseHeight, List<OctaveSetting> octaves, float[] variation)
    {
        double newHeight = baseHeight;

        x += (int)variation[0];

        for (int i = 0; i < octaves.Count; i++)
        {
            float _multiplier = octaves[i].multiplier * variation[1];
            float _factor1 = octaves[i].factor1 * variation[2];
            float _scale1 = octaves[i].scale1 * variation[3];
            float _factorE = octaves[i].factorE * variation[4];
            float _scaleE = octaves[i].scaleE * variation[5];
            float _factorPI = octaves[i].factorPI * variation[6];
            float _scalePI = octaves[i].scalePI * variation[7];
            float _pos1 = octaves[i].pos1 + variation[8];
            float _posE = octaves[i].posE + variation[9];
            float _posPI = octaves[i].posPI + variation[10];

            newHeight += _multiplier * (_factor1 * Mathf.Sin((1 / _scale1) * x + _pos1) + _factorE * Mathf.Sin((1 / _scaleE) * (float)System.Math.E * x + _posE) + _factorPI * Mathf.Sin((1 / _scalePI) * Mathf.PI * x + _posPI));
        }

        return (float)newHeight;
    }

    public static Tile DetermineTile(string _tile1, string _tile2, Vector2Int _pos, float _condition)
    {
        float _value = Mathf.PerlinNoise(_pos.x / 2f + 200, _pos.y / 2f + 200) - ((Mathf.Cos(_pos.x / 2f + _pos.y / 3f) + Mathf.Sin(_pos.x / 5f + _pos.y / 3f)) / 6f);
        if (_value > _condition) return GameUtilities.AllTiles[_tile1];
        else return GameUtilities.AllTiles[_tile2];
    }

}