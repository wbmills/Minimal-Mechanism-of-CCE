using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class aiCon : MonoBehaviour
{

    public int speed = 10;
    public Terrain t;


    private GameObject[] waypointObjects;
    private int curIndex = 0;
    private float curTimer = 0;
    private float randomPausingRate = 6;
    private bool isPausing;
    void Start()
    {
        t = GameObject.Find("Terrain").GetComponent<Terrain>();
        isPausing = false;
        curTimer = 0;
        waypointObjects = GameObject.FindGameObjectsWithTag("waypoint");
        StartCoroutine("timeSinceChange");
    }


    private IEnumerator timeSinceChange()
    {
        var pauseChange = Random.Range(0, randomPausingRate);

        if (pauseChange == 1 && !isPausing)
        {
            isPausing = true;
        }

        if (curTimer > 0)
        {
            yield return new WaitForSeconds(1);
            curTimer -= 1;
        }
        else
        {
            curTimer = Random.Range(4, 10);
        }

        StartCoroutine("timeSinceChange");
    }

    // Update is called once per frame
    void Update()
    {
        Transform wp = waypointObjects[curIndex].transform;

        if (!isPausing && (Vector3.Distance(transform.position, wp.position) < 0.01f | curTimer == 1))
        {
            // change waypoint
            curIndex = Random.Range(0, waypointObjects.Length - 1);
        }
        else if (!isPausing)
        {
            var targetPosition = new Vector3(wp.position.x, t.SampleHeight(transform.position), wp.position.z);
            transform.position = Vector3.MoveTowards(
                transform.position, targetPosition, speed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, t.SampleHeight(transform.position), transform.position.z);

            transform.LookAt(targetPosition);
        }
        else if (isPausing && curTimer <= 1)
        {
            isPausing = false;
        }
    }
}
