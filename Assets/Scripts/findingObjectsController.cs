using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class findingObjectsController : MonoBehaviour
{

    public TMP_Text all;
    public TMP_Text collected;
    public GameObject[] obsToFind;
    public List<GameObject> obsListFound;
    // Start is called before the first frame update
    void Start()
    {
        obsListFound = new List<GameObject>();
        obsToFind = GameObject.FindGameObjectsWithTag("collectable");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "collectable")
        {
            obsListFound.Add(other.gameObject);
            Destroy(other.gameObject);
        }

        if (obsListFound.Count == obsToFind.Length+1)
        {
            collected.text = "All Objects Found!";
        }
        else
        {
            collected.text = $"Objects Found: {obsListFound.Count}/{obsToFind.Length+1}";
        }
    }
}
