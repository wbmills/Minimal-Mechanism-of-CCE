using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ServerData", order = 1)]
public class ServerData : ScriptableObject
{
    public int age;
    public string consent;
    public string participant_id = null;
    public int debugLastID = 0;
    public string curSession;
    public string curRun;
    public string curScene = "Forms";
    public DateTime startTime;
    public DateTime goalTime;
    public int rn;
    public int col_1;
    public int col_2;
    public int nr;
    public bool myTurn = false;
    public bool formsComplete = false;
    public string gender;

    public void SetScene(string scene)
    {
        curScene = scene;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void SetTimes(DateTime x)
    {
        startTime = x;
        goalTime = x.AddMinutes(3);
    }

    public void ResetVariables()
    {

    }
}
