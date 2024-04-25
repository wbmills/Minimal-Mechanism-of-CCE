using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    private bool move = false;
    private GameObject temp;
    private Rigidbody rb;
    public GameObject target;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            move = true;
            temp = other.gameObject;
            rb = temp.GetComponent<Rigidbody>();
        }
    }

    private void Update()
    { 
        if (move && Mathf.Abs(temp.transform.position.y - target.transform.position.y) > 2 &&
            Mathf.Abs(temp.transform.position.x - target.transform.position.x) < 3 &&
            Mathf.Abs(temp.transform.position.z - target.transform.position.z) < 3)
        {
            rb.useGravity = false;
            temp.transform.position = Vector3.MoveTowards(temp.transform.position, target.transform.position,
                Time.deltaTime * 40);
        }
        else if (temp != null)
        {
            move = false;
            rb.useGravity = true;
            temp = null;
        }
    }

}
