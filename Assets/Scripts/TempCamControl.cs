using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCamControl : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float speed;

    void Update()
    {
        if (Input.GetKey(KeyCode.W)) cam.transform.Translate(new Vector3(0f, speed * Time.deltaTime));
        if (Input.GetKey(KeyCode.S)) cam.transform.Translate(new Vector3(0f, -speed * Time.deltaTime));
        if (Input.GetKey(KeyCode.A)) cam.transform.Translate(new Vector3(-speed * Time.deltaTime, 0f));
        if (Input.GetKey(KeyCode.D)) cam.transform.Translate(new Vector3(speed * Time.deltaTime, 0f));
    }
}
