using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class splef_controller : MonoBehaviour
{
    public GameObject tile;
    public List<GameObject> alltiles;
    public int mapSize = 15;
    private Vector3 newpos;
    private GameObject temp;
    void Start()
    {
        newpos = new Vector3(0, 20, 00);
        mapSize = 15;
        alltiles = new List<GameObject>();
        SpawnTiles();
    }

    private void SpawnTiles()
    {
        for (int x = 0; x <= mapSize; x++)
        {
            for (int y = 0; y <= mapSize; y++)
            {
                newpos = new Vector3(x * 4, newpos.y, y * 4);
                temp = Instantiate(tile, newpos, Quaternion.identity);
                alltiles.Add(temp);
            }
        }
    }
}
