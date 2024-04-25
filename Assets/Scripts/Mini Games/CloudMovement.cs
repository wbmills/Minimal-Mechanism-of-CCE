using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public float speed = 10f;
    public float limit = 157f;

    private void Start()
    {
        limit = 50f;
    }
    void Update()
    {
        transform.Translate(Vector3.forward * speed* Time.deltaTime);
        if (transform.position.z > limit)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -400f);
        }
    }
}
