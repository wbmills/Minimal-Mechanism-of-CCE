/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using UnityEngine.Networking;
using System.Web;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

public class SessionData
{
    private List<Instance> instances = new List<Instance>();
    private int sessionID;
    private string mapID;

    public Instance FindInstance(int i)
    {
        foreach(Instance x in instances)
        {
            if (x.GetInstance() == i)
            {
                return x;
            }
        }
        return null;
    }

    public Instance NewInstance(int instanceID, GameObject ob = null, Vector3 pos = new Vector3(), int participantID = 1)
    {
        Instance i = new Instance();
        i.init(newPos: pos, newMapID: mapID, newSessionID: sessionID, newObject: ob, newInstanceID: instanceID, newParticipantID: participantID);
        instances.Add(i);

        return i;
    }

    public int GetNumInstances()
    {
        return instances.Count;
    }

    public void Init(int id)
    {
        sessionID = id;
    }

    public void Add(Instance x)
    {
        if (x.GetSession() == sessionID)
        {
            instances.Add(x);
        }
        else
        {
            Debug.Log("Session ID doesn't match");
        }

    }

    public void Remove(Instance x)
    {
        instances.Remove(x);
    }

    public Instance[] GetInstances()
    {
        return instances.ToArray();
    }

    public void ResetInstances()
    {
        instances = new List<Instance>();
    }

    public Instance GetInstance(int id)
    {
        foreach (Instance i in instances)
        {
            if (i.GetInstance() == id)
            {
                return i;
            }
        }
        return null;
    }
}

public class Instance
{
    private Vector3 pos;
    private string mapID;
    private int sessionID;
    private int participantID;
    private int instanceID;
    private GameObject self;

    public void init(Vector3 newPos, string newMapID, int newSessionID, int newParticipantID, GameObject newObject, int newInstanceID)
    {
        pos = newPos;
        mapID = newMapID;
        sessionID = newSessionID;
        participantID = newParticipantID;
        instanceID = newInstanceID;
        self = newObject;
    }

    public void SetUsingString(string x)
    {

    }

    public GameObject GetSelf()
    {
        return self;
    }

    public void SetSelf(GameObject ob)
    {
        self = ob;
    }

    public void SetPosString(string x, string y, string z)
    {
        pos = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
    }
    public void SetPos(Vector3 newPos)
    {
        pos = newPos;
    }

    public void SetInstance(int x)
    {
        instanceID = x;
    }

    public int GetInstance()
    {
        return instanceID;
    }

    public void SetSession(string x)
    {
        sessionID = Int32.Parse(x);
    }
    public void SetMap(string x)
    {
        mapID = x;
    }
    public void SetParticipant(string x)
    {
        participantID = Int32.Parse(x);
    }

    public Vector3 GetPos()
    {
        return pos;
    }

    public int GetSession()
    {
        return sessionID;
    }
    public int GetParticipant()
    {
        return participantID;
    }

    public string GetMap()
    {
        return mapID;
    }
}

// SeverConnection
public class ServerCon
{
    float tempX;
    float tempY;
    float tempZ;
    float tempID;

    private string url;
    private List<string> param = new List<string> { "x", "y", "z", "id" };
    private SessionData session = null;
    private UnityWebRequest request;

    public string SetURL(string newURL)
    {
        url = newURL;
        return url;
    }

    public void UpdateSession()
    {
        request = UnityWebRequest.Get("http://localhost/tutorial/reciever.php?x=4");
        session.GetInstance(1).SetMap(request.downloadHandler.text);
    }

    // methods for setting and getting session
    public SessionData GetSession()
    {
        return session;
    }

    public void SetSession(SessionData s)
    {
        session = s;
    }

    public SessionData NewSession()
    {
        session = new SessionData();
        return session;
    }
}
*/