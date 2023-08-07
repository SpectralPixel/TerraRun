using UnityEngine;

public class FirewallMovement : MonoBehaviour
{

    [SerializeField] private Transform player;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float timeToMax;

    private float startX;
    private float timePassed;
    private float wallSpeed;

    private void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        wallSpeed = Mathf.Lerp(minSpeed, maxSpeed, timePassed / timeToMax);

        transform.position = new Vector2(timePassed * wallSpeed + startX, player.position.y + 10);

        timePassed += Time.deltaTime;
    }

}
