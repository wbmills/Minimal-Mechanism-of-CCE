using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class returnToTop : MonoBehaviour
{

    public GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void TransportPlayer(Vector3 pos)
    {
        CharacterController c = player.GetComponent<CharacterController>();
        c.enabled = false;
        player.transform.position = pos;
        c.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.y < -40)
        {
            TransportPlayer(new Vector3(16.82f, 1.25f, 16.89f));
        }
    }
}
