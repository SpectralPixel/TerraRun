using UnityEngine;

public class PerlinNoisePreview : MonoBehaviour
{

    [Min(2)] public int TextureSize;
    [Min(0.06f)] public float Scale;
    public Vector2 Offset;
    [Range(0f, 1f)] public float MinimumThreshold;
    [Range(0f, 1f)] public float MaximumThreshold;

    [HideInInspector] public bool AutoUpdate;

    private void Start()
    {
        Destroy(gameObject);
    }

    public void GeneratePerlinNoise()
    {
        transform.position = new Vector3(-TextureSize, -TextureSize);
        transform.localScale = new Vector3(TextureSize / 5f, 1f, TextureSize / 5f);

        Texture2D _noiseTexture = new Texture2D(TextureSize, TextureSize);
        for (int x = 0; x < TextureSize; x++)
        {
            for (int y = 0; y < TextureSize; y++)
            {
                float value = Mathf.PerlinNoise(x / Scale + Offset.x, y / Scale + Offset.y);
                if (value > MinimumThreshold && value < MaximumThreshold) _noiseTexture.SetPixel(x, y, Color.HSVToRGB(0f, 0f, value));
                else _noiseTexture.SetPixel(x, y, Color.HSVToRGB(Mathf.Clamp01(value / 8f), 1f, 1f));
            }
        }
        _noiseTexture.Apply();

        _noiseTexture.filterMode = FilterMode.Point;

        Material _previewMaterial = Resources.Load<Material>("Unlit Perlin");
        _previewMaterial.mainTexture = _noiseTexture;
        GetComponent<MeshRenderer>().material = _previewMaterial;
    }
}
