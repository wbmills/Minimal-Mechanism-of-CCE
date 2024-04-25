using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class addTimeToScore : MonoBehaviour
{
    public TMP_Text t;
    private float increment = 0.2f;
    public bool stop;
    // Update is called once per frame

    private void Start()
    {
        stop = false;
        StartCoroutine("addTime", 0);
    }

    public void SetStop()
    {
        stop = true;
    }

    public IEnumerator addTime(float time)
    {
        if (!stop)
        {
            yield return new WaitForSeconds(increment);
            time = time + increment;
            t.text = $"Time Alive: {Math.Round(time, 3)} Seconds";
            StartCoroutine("addTime", time);
        }
        yield break;
    }
}
