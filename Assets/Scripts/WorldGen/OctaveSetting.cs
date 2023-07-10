using UnityEngine;

[CreateAssetMenu(fileName = "Octave", menuName = "ScriptableObjects/Octave", order = 1)]
public class OctaveSetting : ScriptableObject
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