using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    public static float GetHeight(int seed, int x, int baseHeight, List<OctaveSetting> octaves, float[] variation)
    {
        double newHeight = baseHeight;

        x += seed;

        for (int i = 0; i < octaves.Count; i++)
        {
            float _multiplier = octaves[i].multiplier * variation[0];
            float _factor1 = octaves[i].factor1 * variation[1];
            float _scale1 = octaves[i].scale1 * variation[2];
            float _factorE = octaves[i].factorE * variation[3];
            float _scaleE = octaves[i].scaleE * variation[4];
            float _factorPI = octaves[i].factorPI * variation[5];
            float _scalePI = octaves[i].scalePI * variation[6];

            newHeight += _multiplier * (_factor1 * Mathf.Sin(_scale1 * x) + _factorE * Mathf.Sin(_scaleE * (float)System.Math.E * x) + _factorPI * Mathf.Sin(_scalePI * Mathf.PI * x));
        }

        return (float)newHeight;
    }
}