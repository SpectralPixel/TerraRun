using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCamControl : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float speed;

    void Update()
    {
        if (Input.GetKey(KeyCode.W)) player.Translate(new Vector3(0f, speed * Time.deltaTime));
        if (Input.GetKey(KeyCode.S)) player.Translate(new Vector3(0f, -speed * Time.deltaTime));
        if (Input.GetKey(KeyCode.A)) player.Translate(new Vector3(-speed * Time.deltaTime, 0f));
        if (Input.GetKey(KeyCode.D)) player.Translate(new Vector3(speed * Time.deltaTime, 0f));
    }
}
