using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FALLANDKILL : MonoBehaviour
{
    public GameObject player;
    public GameObject terrain;
    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.y < 5)
        {
            terrain.GetComponent<killonhit>().kill();
        }
    }
}
