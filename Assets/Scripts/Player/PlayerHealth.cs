using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private FirewallHeat wallHeatScript;

    [SerializeField] private float maxHealth;
    [SerializeField] private float bodyTemperature;
    [SerializeField] private float deathTemperature;

    [HideInInspector] public float Health;
    [HideInInspector] public float Temperature;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Health = 100;
        Temperature = bodyTemperature;
    }

    private void Update()
    {
        float _newTemperature = wallHeatScript.GetHeat(transform);
        Temperature = _newTemperature > bodyTemperature ? _newTemperature : bodyTemperature;

        spriteRenderer.color = Color.HSVToRGB(
            Mathf.Lerp(55f / 360f, 0f, _newTemperature / deathTemperature),
            Mathf.Lerp(0f, 1f, _newTemperature / deathTemperature),
            1f
        );

        if (Health <= 0f || Temperature >= deathTemperature)
        {
            spriteRenderer.color = Color.black; // I mean at that point you may as well be a piece of coal
        }
    }

    public void UpdateHealth(float _change)
    {
        Health = Mathf.Clamp(Health + _change, 0f, maxHealth);
    }

    public void SetHealth(float _newHealth)
    {
        Health = _newHealth;
    }

}
