using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class FormController : MonoBehaviour
{
    private Dictionary<int, GameObject> canvases;
    public ActiveServer server;
    private int curFormIndex;
    public string sceneToLoad;
    public ServerData playerData; 
    void Start()
    {
        DontDestroyOnLoad(playerData);
        sceneToLoad = "Tutorial";
        curFormIndex = 0;
        canvases = InitCanvases();
        SetActiveForm();
    }

    private static Dictionary<int, GameObject> InitCanvases()
    {
        Dictionary<int, GameObject> final = new Dictionary<int, GameObject>();
        GameObject[] all = GameObject.FindGameObjectsWithTag("Form");
        int i = 0;
        foreach(GameObject ob in all)
        {
            final.Add(System.Int32.Parse(ob.name), ob);
            i++;
        }
        return final;
    }

    private void EndForm()
    {
        if (sceneToLoad != null)
        {
            if (SetInfo() == true)
            {
                playerData.SetScene("Tutorial");
            }
            else
            {
                SceneManager.LoadScene("Forms");
            }
        }
        else
        {
            Application.Quit();
        }
    }

    public void IncrementIndex()
    {
        curFormIndex += 1;
        SetActiveForm();
    }

    public bool SetInfo()
    {
        Dictionary<string, object> info = CollectInfo();
        try
        {

            if ((int)info["consent"] == 0 | info["age"].ToString() == "abc")
            {
                return false;
            }
            else
            {
                playerData.consent = info["consent"].ToString();
                playerData.age = Int32.Parse((string)info["age"]);
                if (info.ContainsKey("gender"))
                {
                    playerData.gender = info["gender"].ToString();
                }
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public void RedirectToForm()
    {
        Application.OpenURL("PROLIFIC REDIRECT HERE");
        Application.Quit();
    }

    private void SetActiveForm()
    {
        foreach(GameObject ob in canvases.Values)
        {
            ob.SetActive(false);
        }

        if (canvases.Count <= curFormIndex)
        {
            EndForm();
        }
        else
        {
            canvases[curFormIndex].SetActive(true);
        }
    }

    // desgusting code I'm sorry :'(
    private Dictionary<string, object> CollectInfo()
    {
        // get all info from canvases
        Dictionary<string, object> info = new Dictionary<string, object>();
        for (int i = 0; i < canvases.Count; i++)
        {
            foreach(Transform child in canvases[i].transform)
            {
                if (child.TryGetComponent<TMP_InputField>(out TMP_InputField inputField))
                {
                    if (inputField.text != null)
                    {
                        info.Add(child.name, inputField.text);
                    }
                    else
                    {
                        info.Add(child.name, "abc");
                    }
                }
                else if (child.TryGetComponent<Toggle>(out Toggle toggle))
                {
                    if (toggle.isOn)
                    {
                        info.Add(child.name, 1);
                    }
                    else
                    {
                        info.Add(child.name, 0);
                    }
                }
            }
        }

        return info;
    }
}