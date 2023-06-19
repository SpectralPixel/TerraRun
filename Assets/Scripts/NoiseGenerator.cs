using System;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    public static float GetHeight(int x, int baseHeight, List<OctaveSetting> octaves)
    {
        double newHeight = baseHeight;

        for (int i = 0; i < octaves.Count; i++)
        {
            float _multiplier = octaves[i].multiplier;
            float _factor1 = octaves[i].factor1;
            float _scale1 = octaves[i].scale1;
            float _factorE = octaves[i].factorE;
            float _scaleE = octaves[i].scaleE;
            float _factorPI = octaves[i].factorPI;
            float _scalePI = octaves[i].scalePI;

            newHeight += _multiplier * (_factor1 * Math.Sin(_scale1 * x) + _factorE * Math.Sin(_scaleE * Math.E * x) + _factorPI * Math.Sin(_scalePI * Math.PI * x));
        }

        return (float)newHeight;
    }
}

[Serializable]
public class OctaveSetting
{
    public float multiplier;
    [Space]
    public float factor1;
    public float scale1;
    [Space]
    public float factorE;
    public float scaleE;
    [Space]
    public float factorPI;
    public float scalePI;
}