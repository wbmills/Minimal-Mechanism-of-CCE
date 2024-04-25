using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class countdownToStart : MonoBehaviour
{
    // Start is called before the first frame update
    private List<string> words;
    public TMP_Text t;
    public string finalMessage = "Find the Goal"!;
    public string initialMessage = "Starting in...";
    public void Start()
    {
        CallStart();
    }
    public void CallStart()
    {
        t.transform.parent.gameObject.SetActive(true);
        words = new List<string>() {initialMessage, "3", "2", "1", finalMessage};
        StartCoroutine("countdown", 0);
    }

    private IEnumerator countdown(int i)
    {
        if (i < words.Count)
        {
            t.text = words[i];
            yield return new WaitForSeconds(1);
            StartCoroutine("countdown", ++i);
        }
        else
        {
            t.transform.parent.gameObject.SetActive(false);
            yield break;
        }
    }
}
