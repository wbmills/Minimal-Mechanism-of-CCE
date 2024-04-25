using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tilebehaviour : MonoBehaviour
{
    private bool drop = false;
    private IEnumerator WaitAndDrop()
    {
        yield return new WaitForSeconds(.8f);
        drop = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        print("collision trigger");
        if (other.transform.name == "hit")
        {
            StartCoroutine("WaitAndDrop");
        }
    }

    private void Update()
    {
        if (transform.position.y < -2)
        {
            Destroy(gameObject);
        }

        if (drop)
        {
            transform.Translate(Vector3.down * Time.deltaTime * 10);
        }
    }
}
