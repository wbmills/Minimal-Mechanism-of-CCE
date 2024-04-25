using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sendToGame : MonoBehaviour
    
{
    public ServerData playerData;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerData.SetScene("Splef");
        }
    }
}
