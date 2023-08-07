using UnityEngine;

public class FirewallHeat : MonoBehaviour
{

    [SerializeField] private float initialHeat;
    [SerializeField] private float range = 4; // the range is measured by how many tiles away the temperature of 50*C is reached.

    private ParticleSystem ps;
    private float Heat;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();

        Heat = initialHeat;
    }

    public float GetHeat(Transform _otherTransform)
    {
        float distance = (_otherTransform.position.x - transform.position.x - ps.shape.scale.x / 2f) * 4f / range;

        float heat = Mathf.Pow(distance, -2) * Heat;
        if (distance <= 0f) heat = Heat;

        return heat;
    }

}
