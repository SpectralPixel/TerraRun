using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    public static float GenerateHeight(int seed, int x, int baseHeight, List<OctaveSetting> octaves, float[] variation)
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
            float _offset1 = variation[7];
            float _offsetE = variation[8];
            float _offsetPI = variation[9];

            newHeight += _multiplier * (_factor1 * Mathf.Sin((1 / _scale1) * x + _offset1) + _factorE * Mathf.Sin((1 / _scaleE) * (float)System.Math.E * x + _offsetE) + _factorPI * Mathf.Sin((1 / _scalePI) * Mathf.PI * x + _offsetPI));
        }

        return (float)newHeight;
    }
}