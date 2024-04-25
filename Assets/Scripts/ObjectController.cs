using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActiveServer;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;

public class ObjectController : MonoBehaviour
{
    public SessionData session = null;
    public ActiveServer activeServer;
    public GameObject testObject;
    public GameObject localPlayer;
    private string id;
    private bool postPos;
    private bool isFinished;
    public ServerData playerData;

    private void Start()
    {
        localPlayer = GameObject.FindGameObjectWithTag("Player");
        id = playerData.participant_id;
        postPos = false;
    }

    public void SetLevel(string name)
    {
        SceneManager.LoadScene(name);
        if (name == "CCE Experiment")
        {
            isFinished = true;
        }
    }

    public IEnumerator SetPlayerPosition()
    {
        if (postPos)
        {
            session.GetInstance(id).SetPos(localPlayer.transform.position, localPlayer.transform.eulerAngles.y);
        }
        yield return new WaitForEndOfFrame();
    }

    private void Update()
    {
        // if run is finished, go to waiting room
        if (isFinished)
        {
            SetLevel("Waiting Room");
        }

        if (session != null)
        {
            foreach (Instance i in session.GetInstances())
            {
                if (i.GetInstanceObject() != null && i.GetInstanceID() != id)
                {
                    i.GetInstanceObject().transform.position = i.GetPos();
                }
                else if (i.GetInstanceObject() == null && i.GetInstanceID() != id)
                {
                    i.SetInstanceObject(Instantiate(testObject, Vector3.zero, Quaternion.identity));
                    break;
                }
                else if (i.GetInstanceObject() == null && i.GetInstanceID() == id)
                {
                    i.SetInstanceObject(localPlayer);
                    postPos = true;
                    StartCoroutine("SetPlayerPosition");
                }
            }
        }
        else
        {
            try
            {
                session = activeServer.GetSession();
            }
            catch (System.Exception)
            {
                print("lol");
            }
        }
    }
}
