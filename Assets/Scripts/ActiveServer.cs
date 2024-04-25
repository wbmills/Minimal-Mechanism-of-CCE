using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using UnityEngine.Networking;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Runtime.CompilerServices;
using TMPro;
using System.Net;

public class ActiveServer : MonoBehaviour
{

    // all vars for update
    private string[] x;
    private string[] y;
    private string[] z;
    private string[] a;
    private string[] ids;
    private int newnr;
    private int state;
    private int gens;
    private string newMyTurn;

    // all URLs
    private string rootURL;
    private string defaultSessionURL;
    private string initialiseURL;
    private string diffusionURL;
    private string pairedURL;
    private string singleURL;
    private string getSessionURL;
    private string GetWaitURL;
    private bool isTimerActive;
    private bool isWaiting;
    private string initialiseURLTest;
    private float hasBeenWaitingFor = 0;
    private float maxTimeToWait = 25 * 60;
    private int initNumgens;
    private int totalNumRuns;
    private int maxTimeToWaitForOthers;

    // public vars
    public bool useController;
    public float updateRate;
    public GameObject localPlayer;
    public ServerData playerData;
    public int activeCoroutines;
    public GameObject countInCanvas;
    public bool debug;
    public GameObject waitingRoomCanvas;
    public GameObject currentStateCanvas;
    public TMP_Text nrCanvas;
    public GameObject emptyPlayer;
    public bool isTesting;
    public TMP_Text sessionInfoText;
    public float speedOfTransition;

    // private vars
    private Dictionary<int, string> conditions;
    private float timer;
    private SessionData session = null;
    private List<GameObject> currentPlayers;
    private List<GameObject> sparePlayers;
    private int curnr = 0;
    private int maxGens;
    private int maxRunsPerGen;
    private float timeToWait;
    private Dictionary<int, int> genDict;
    private bool hasTeleported;
    private int numBugs;
    

    void Start()
    {
        // ensure ScriptableObject playerData is maintained between scenes
        DontDestroyOnLoad(playerData);
        SetDefaultValues();
        // begin game loop
        if (SceneManager.GetActiveScene().name != "Debrief")
        {
            StartCoroutine("Operator");
        }
    }

    private string GetUniqueID()
    {
        // Generate a new UUID
        Guid uuid = Guid.NewGuid();

        // Convert the UUID to a string
        return uuid.ToString().Replace("-","");
    }

    private string NewID()
    {
        return Guid.NewGuid().ToString();
    }

    private IEnumerator CountIn()
    {
        yield return new WaitForSeconds(3);
        StartCoroutine("Operator");
    }

    private IEnumerator increaseWaitingTime()
    {
        if (hasBeenWaitingFor == maxTimeToWait && playerData.curSession == null)
        {
            ClearSession();
            playerData.curSession = null;
            playerData.SetScene("Debrief");
            yield break;
        }
        else if (hasBeenWaitingFor == maxTimeToWaitForOthers && playerData.curSession == null)
        {
            PostInfo($"{defaultSessionURL}&participant_id={playerData.participant_id}&hash={GetHash()}");
        }

        yield return new WaitForSeconds(1);
        hasBeenWaitingFor += 1;
        StartCoroutine("increaseWaitingTime");
    }

    // all default values for server to function
    private void SetDefaultValues()
    {
        maxTimeToWaitForOthers = 7 * 60;
        totalNumRuns = 0;
        initNumgens = 0;
        numBugs = 0;
        speedOfTransition = 400;
        hasTeleported = false;
        maxTimeToWait = 25 * 60;
        hasBeenWaitingFor = 0;
        timer = -1;
        isWaiting = false;
        currentPlayers = new List<GameObject>();
        isTimerActive = false;
        localPlayer = GameObject.FindGameObjectWithTag("Player");
        playerData.curScene = SceneManager.GetActiveScene().name;
        sparePlayers = new List<GameObject>();
        sparePlayers = GameObject.FindGameObjectsWithTag("npc").ToList();
        genDict = new Dictionary<int, int>()
        {
            {0,2},
            {1,2},
            {2,5},
        };


        debug = false; // don't check for sever updates
        isTesting = false; // send straight to specified condition


        try
        {
            emptyPlayer = GameObject.FindGameObjectWithTag("npc");
        }
        catch   (Exception) {
            emptyPlayer = null;
        }

        updateRate = 0.01f;

        // make sure cursor is visable by default
        UnityEngine.Cursor.visible = true;

        // all URLs to access database
        rootURL = "https://willbmills.com/cce/";
        initialiseURL = $"{rootURL}initialise.php?";
        singleURL = $"{rootURL}singleRun.php?";
        pairedURL = $"{rootURL}pairedRun.php?";
        diffusionURL = $"{rootURL}diffusionRun.php?";
        getSessionURL = $"{rootURL}getSession.php?";
        initialiseURLTest = $"{rootURL}initialise_control.php?";
        defaultSessionURL = $"{rootURL}createDefaultSession.php?";

        

        // dictionary to access condition URL via integer recieved from database
        conditions = new Dictionary<int, string>() {
        {0, singleURL},
        {1, pairedURL},
        {2, diffusionURL},
        {99, getSessionURL },
        };
    }

    // post information using given URL to server
    private IEnumerator PostInfo(string url, Action<string> onFinish=null)
    {
        print(url);
        if (debug)
        {
            yield break;
        }
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                yield break;
            }
            else
            {
                onFinish?.Invoke(www.downloadHandler.text);
            }
        }
    }

    private string GetURLProlific()
    {
        string thisPageURL = Application.absoluteURL;
        string participantID;
        if (thisPageURL.ToLower().Contains("prolific_pid"))

        {
            Uri thisPageUri = new Uri(thisPageURL.ToLower());

            participantID = System.Web.HttpUtility.ParseQueryString(

                thisPageUri.Query).Get("prolific_pid"); 
        }

        else{
            participantID = NewID().Replace("-", "");
        }

        // If there isn't an ID in the URL, we need to prompt the participant.
        return participantID;
    }

    // post consent and age to database when form is complete
    public void PostFormData()
    {
        NewSession();
        StartCoroutine("increaseWaitingTime");
        session.sessionID = null;
        playerData.myTurn = false;
        playerData.col_1 = 99;
        var age = playerData.age;
        if (playerData.curSession != null | playerData.curSession != "")
        {
            playerData.curSession = null;
        }
        var consent = playerData.consent;
        var gender = playerData.gender;
        if (gender == null)
        {
            gender = "Not Specified";
        }
        playerData.participant_id = GetURLProlific();  //NewID().Replace("-", "");
        string url;
        if (isTesting)
        {
            url = $"{initialiseURLTest}&age={age}&gender={gender}&consent={consent}&hash={GetHash()}&participant_id={playerData.participant_id}&is_human=1&given_con=0";
        }
        else
        {
            url = $"{initialiseURL}&age={age}&gender={gender}&consent={consent}&hash={GetHash()}&participant_id={playerData.participant_id}&is_human=1";
        }
        
        StartCoroutine("PostNoCallback", url);
    }

    // post to database with no info required in return
    private IEnumerator PostNoCallback(string url)
    {
        print(url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                yield break;
            }
            else
            {
                yield return www.downloadHandler.text;
            }
        }
    }

    private string GetHash()
    {
        byte[] hash;
        string hashString;
        var secretKey = "WillMills3451";

        // Convert Time.time to bytes
        byte[] timeBytes = BitConverter.GetBytes(Time.time);

        // Combine sessionID, secretKey, and timeBytes
        string dataToHash = playerData.participant_id.ToLower() +  secretKey;

        // Compute the hash
        using (SHA512 shaMan = new SHA512Managed())
        {
            hash = shaMan.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
            hashString = BitConverter.ToString(hash).Replace("-", "");
        }
        return hashString;
    }

    public void FixedUpdate()
    {
        if (session != null && playerData != null && playerData.curSession != null)
        {
            if (session.instances == null)
            {
                session.instances = new List<Instance>();
            }
            else if (session.GetInstances().Length > 0)
            {
                foreach (var instance in session.GetInstances())
                {
                    if (instance.GetSelf() != null && instance.GetInstanceID() != playerData.participant_id)
                    {
                        var ins = instance.GetSelf();
                        ins.transform.position = Vector3.MoveTowards(instance.GetLastPos(), instance.GetPos(), speedOfTransition * Time.deltaTime);
                        //ins.transform.position = instance.GetPos();
                        ins.transform.rotation = Quaternion.LookRotation(GetHeadingNormal(instance.GetAlpha()));
                    }
                    else if (instance.GetSelf() == null && instance.GetInstanceID() != playerData.participant_id && sparePlayers.Count > 0)
                    {
                        GameObject s = sparePlayers[0];
                        sparePlayers.Remove(s);
                        currentPlayers.Add(s);
                        instance.SetSelf(s);
                    }
                }
            }
        }
    }

    private Vector3 GetHeadingNormal(float normalizedAngle)
    {
        // Convert the normalized angle back to degrees
        float angle = normalizedAngle * 180f;

        // Convert the angle to be between 0 and 360 degrees
        if (angle < 0)
        {
            angle += 360f;
        }

        // Convert angle to radians
        float angleRad = angle * Mathf.Deg2Rad;

        // Calculate the direction vector
        Vector3 headingDirection = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad));

        // Return the heading direction
        return headingDirection;
    }

    private void ClearSession()
    {
        session.instances = new List<Instance>();
        foreach(GameObject g in currentPlayers)
        {
            Destroy(g);
        }
        currentPlayers = new List<GameObject>();
    }

    // decide whether to try to get new session or update current session
    public IEnumerator Operator()
    {
        yield return new WaitForSeconds(updateRate);

        if (debug)
        {
            yield break;
        }

        if (playerData.consent == null && playerData.curScene != "Forms")
        {
            playerData.SetScene("Forms");
            StartCoroutine("Operator");
        }
        else if (playerData.curScene == "Forms" | playerData.curScene == "Tutorial")
        {
            StartCoroutine("Operator");
        }
        else if ((playerData.curSession == "" | playerData.curSession == null) && (playerData.participant_id != null | playerData.participant_id != ""))
        {
            StartCoroutine("WaitForSession");
        }
        else if ((playerData.curSession == "" | playerData.curSession == null) && (playerData.participant_id == null | playerData.participant_id == ""))
        {
            StartCoroutine("Operator");
        }
        else if (playerData.curSession != null && playerData.curScene != "CCE Experiment" && playerData.myTurn)
        {
            playerData.SetScene("CCE Experiment");
            StartCoroutine("UpdateSession");
        }
        else if (playerData.curScene == "CCE Experiment" && playerData.myTurn)
        {
            StartCoroutine("UpdateSession");
        }
        else if (!playerData.myTurn && playerData.curSession != null)
        {
            isWaiting = true;
            StartCoroutine("WaitForTurn");
            StartCoroutine("WaitForSession");
        }
    }

    // while waiting for turn in diffusion chain, wait and get if your turn
    public IEnumerator WaitForTurn()
    {
        var callTo = true;
        Dictionary<string, bool> string2bool = new Dictionary<string, bool>() {
            {"true", true },
            {"false", false}
        };
        yield return StartCoroutine(PostInfo($"{conditions[playerData.col_1]}&session_id={playerData.curSession}&x=0&y=0&z=0&a=0&participant_id={playerData.participant_id}&hash={GetHash()}", result =>
        {
            var p = SplitParameter(result);
            if (p == null)
            {
                StartCoroutine("Operator");
            }
            else if (p.ContainsKey("nr") && p["nr"] == "1000")
            {
                callTo = false;
                playerData.SetScene("Debrief");
            }
            else
            {
                playerData.myTurn = string2bool[p["myTurn"]];
            }
            
        }));
        if (callTo)
        {
            StartCoroutine("Operator");
        }
        
    }

    // while in waiting room, try to get session.
    public IEnumerator WaitForSession()
    {
        if (session == null)
        {
            NewSession();
        }
        //print("Waiting....");
        Vector3 pos = localPlayer.transform.position;
        // Start the asynchronous operation to retrieve the session ID
        yield return StartCoroutine(PostInfo($"{getSessionURL}&participant_id={playerData.participant_id}&hash={GetHash()}&x={pos.x}&y={pos.y}&z={pos.z}&a={GetHeading()}", result =>
        {
            if (result != null)
            {
                var r = SplitParameter(result);
                if (r == null)
                {
                    return;
                }
                else if (r.ContainsKey("con1") && r["session"] == "null" && Int32.Parse(r["con1"]) != playerData.col_1)
                {
                    playerData.col_1 = Int32.Parse(r["con1"]);
                }
                else if ((r["session"] != null && r["session"] != "null") && playerData.curSession == null)
                {
                    playerData.curSession = r["session"];
                    playerData.col_1 = Int32.Parse(r["con1"]);
                    maxGens = genDict[playerData.col_1];
                    if (maxTimeToWait < 10 * 60)
                    {
                        maxTimeToWait = 15 * 60;
                    }

                    if (sessionInfoText != null)
                    {
                        sessionInfoText.text = "Your next task starts\nin about 10 minutes...";
                    }
                }
                else if (r.ContainsKey("x") && !playerData.myTurn)
                {
                    var x = SplitStringToArray(r["x"]);
                    var y = SplitStringToArray(r["y"]);
                    var z = SplitStringToArray(r["z"]);
                    var a = SplitStringToArray(r["alpha"]);
                    var ids = SplitStringToArray(r["id"]);
                    if (ids.Length > 0 && ids[0] != "")
                    {
                        // for each player, create or append current instance with updated position
                        for (int i = 0; i < x.Length; i++)
                        {
                            var tm = session.GetInstance(ids[i]);
                            if (tm == null)
                            {
                                tm = session.NewInstance(ids[i]);
                            }
                            tm.SetPosString(x[i], y[i], z[i], a[i]);
                        }
                    }
                }
            }
        }));
        if (!isWaiting)
        {
            StartCoroutine("Operator");
        }
    }

    private float GetHeading()
    {
        Vector3 headingDirection = localPlayer.transform.forward;

        // Calculate the angle in degrees relative to north
        float angle = Mathf.Atan2(headingDirection.x, headingDirection.z) * Mathf.Rad2Deg;

        // Convert angle to be between -180 and 180 degrees
        if (angle > 180f)
        {
            angle -= 360f;
        }

        // Normalize the angle to be between -1 and 1
        float normalizedAngle = angle / 180f;

        // Display the normalized heading value
        return angle;
    }

    private IEnumerator CheckSessionDuringRun()
    {
        yield return StartCoroutine(PostInfo($"{getSessionURL}&participant_id={playerData.participant_id}&hash={GetHash()}&session={playerData.curSession}", result =>
        {
            if (result != null) {
                var r = SplitParameter(result);
                if (r != null && r.ContainsKey("session") && r["session"] != playerData.curSession)
                {
                    playerData.curSession = r["session"];
                    playerData.col_1 = Int32.Parse(r["con1"]);
                }
            }
        }));
    }

    // while in game, communicate with server to get game state
    public IEnumerator UpdateSession()
    {
        maxRunsPerGen = 6;
        maxGens = 2;

        if (session == null)
        {
            NewSession().init(playerData.curSession);
        }

        var pos = localPlayer.transform.position;
        var url = GetUrl(pos);

        yield return StartCoroutine(PostInfo(url, result =>
        {
            var p = SplitParameter(result);
            if (IsErrorCondition(p))
            {
                HandleErrorCondition(url, result);
            }
            else
            {
                UpdatePlayerData(p);

                if (ShouldDebrief(p))
                {
                    Debrief();
                }
                else if (ShouldStartNewRun(p))
                {
                    StartNewRun(p);
                }
                else if (ShouldWarn(p))
                {
                    Warn();
                }
                else if (ShouldReachGoal(p))
                {
                    ReachGoal();
                }
                else
                {
                    KeepGoing();
                }

                UpdateRunNumber(p);
                UpdateOtherPlayerData(p);
            }
        }));
        StartCoroutine("Operator");
    }

    private string GetUrl(Vector3 pos)
    {
        return $"{conditions[playerData.col_1]}&session_id={playerData.curSession}&hash={GetHash()}&participant_id={playerData.participant_id}&x={pos.x}&y={pos.y}&z={pos.z}&a={GetHeading()}";
    }

    private bool IsErrorCondition(Dictionary<string, string> p)
    {
        return p == null || !p.ContainsKey("x") || totalNumRuns > 12;
    }

    private void HandleErrorCondition(string url, string result)
    {
        numBugs++;
        if (numBugs > 30 || totalNumRuns > 12)
        {
            Debrief();
        }
    }

    private void UpdatePlayerData(Dictionary<string, string> p)
    {
        x = SplitStringToArray(p["x"]);
        y = SplitStringToArray(p["y"]);
        z = SplitStringToArray(p["z"]);
        a = SplitStringToArray(p["alpha"]);
        ids = SplitStringToArray(p["id"]);
        newnr = Int32.Parse(p["nr"]);
        state = Int32.Parse(p["state"].Replace("'", ""));
        gens = Int32.Parse(p["gens"]);
        newMyTurn = p["myTurn"];

        if (newnr == 0 && totalNumRuns == 0)
        {
            initNumgens = gens;
        }

        if (playerData.col_1 == 2)
        {
            gens = gens - initNumgens;
        }
    }

    private bool ShouldDebrief(Dictionary<string, string> p)
    {
        return gens == maxGens || newMyTurn == "false" || newnr == 1000;
    }

    private void Debrief()
    {
        totalNumRuns += 1;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        ClearSession();
        playerData.curSession = null;
        playerData.SetScene("Debrief");
    }

    private bool ShouldStartNewRun(Dictionary<string, string> p)
    {
        return curnr != newnr || timer == 0 || newnr == 1000;
    }

    private void StartNewRun(Dictionary<string, string> p)
    {
        StartCoroutine("CheckSessionDuringRun");
        totalNumRuns += 1;
        ClearSession();
        hasTeleported = false;
        timer = -1;
        TransportPlayer(new Vector3(float.Parse(p["start_x"].Replace("'", "")), 10, float.Parse(p["start_z"].Replace("'", ""))));
        countInCanvas.GetComponent<countdownToStart>().CallStart();
    }

    private bool ShouldWarn(Dictionary<string, string> p)
    {
        return curnr == newnr && state == 1;
    }

    private void Warn()
    {
        if (!isTimerActive)
        {
            currentStateCanvas.SetActive(true);
            StartCoroutine("reduceTimer", timer);
        }
    }

    private bool ShouldReachGoal(Dictionary<string, string> p)
    {
        return curnr == newnr && state == 2;
    }

    private void ReachGoal()
    {
        if (!isTimerActive)
        {
            StartCoroutine("reduceTimer", timer);
        }

        if (!hasTeleported)
        {
            TransportPlayer(new Vector3(993.13f, -281.51f, -277.7f));
            hasTeleported = true;
        }
    }

    private void KeepGoing()
    {
        if (currentStateCanvas != null)
        {
            currentStateCanvas.SetActive(false);
        }
    }

    private void UpdateRunNumber(Dictionary<string, string> p)
    {
        curnr = newnr;
        if (nrCanvas != null && gens == 0)
        {
            nrCanvas.text = $"Runs: {curnr + 1}/12";
        }
        else if (nrCanvas != null && gens == 1)
        {
            nrCanvas.text = $"Runs: {curnr + 7}/12";
        }
    }

    private void UpdateOtherPlayerData(Dictionary<string, string> p)
    {
        if (ids.Length >= 1 && ids[0] != "")
        {
            for (int i = 0; i < ids.Length; i++)
            {
                var tempInstance = session.GetInstance(ids[i]);
                if (tempInstance != null)
                {
                    tempInstance.SetLastPos(tempInstance.GetPos());
                    tempInstance.SetPosString(x[i], y[i], z[i], a[i]);
                }
                else
                {
                    Instance s = session.NewInstance(ids[i]);
                    s.SetLastPosString(x[i], y[i], z[i]);
                    s.SetPosString(x[i], y[i], z[i], a[i]);
                }
            }
        }
    }

    private IEnumerator reduceTimer(float timer){
        if (timer == -1 && playerData.col_1 == 0)
        {
            timer = 10;
        }
        else if (timer == -1 && playerData.col_1 != 0)
        {
            timer = 20;
        }

        yield return new WaitForSeconds(1);
        if (timer > 0)
        {
            isTimerActive = true;
            timer -= 1;
            StartCoroutine("reduceTimer", timer);
        }
        else
        {
            isTimerActive = false;
        }

        // set timer canvases 
        if (currentStateCanvas != null && timer >= 0)
        {
            currentStateCanvas.GetComponentInChildren<TMP_Text>().text = $"Time Remaining: {timer} Seconds";
        }

        if (waitingRoomCanvas != null && timer >= 0)
        {
            waitingRoomCanvas.GetComponentInChildren<TMP_Text>().text = $"Next Run\nStarts in\n{timer}\nSeconds";
        }
    }

    private void TransportPlayer(Vector3 pos)
    {
        CharacterController c = localPlayer.GetComponent<CharacterController>();
        c.enabled = false;
        localPlayer.transform.position = pos;
        c.enabled = true;
    }

    private string[] SplitStringToArray(string s)
    {
        string final = s;
        string[] finalS;
        try
        {
            final = final.Replace("[", "").Replace("]", "");
            finalS = final.Split(",");
        }
        catch
        {
            finalS = null;
        }

        return finalS;
    }
    public Dictionary<string, string> SplitParameter(string inputString)
    {
        print(inputString);
        Dictionary<string, string> keyValueDictionary = new Dictionary<string, string>();
        try
        {
            string[] keyValuePairs = inputString.Split('&');

            // Iterate over each key-value pair
            foreach (string pair in keyValuePairs)
            {
                if (pair != "")
                {
                    // Split the pair by '=' to separate key and value
                    string[] parts = pair.Split('=');

                    // Add key-value pair to the dictionary
                    keyValueDictionary[parts[0]] = parts[1];
                }
            }
        }
        catch (Exception)
        {
            keyValueDictionary = null;
        }
        return keyValueDictionary;
    }

    // methods for setting and getting session
    public SessionData GetSession()
    {
        return session;
    }

    public SessionData NewSession()
    {
        session = new SessionData();
        return session;
    }

    public class Instance
    {
        private Vector3 lastPos;
        private Vector3 pos;
        private float alpha;
        private string mapID;
        private string sessionID;
        private string instanceID;
        private GameObject self;

        public void init(Vector3 newPos, string newMapID, string newSessionID, GameObject newObject, string newInstanceID)
        {
            pos = newPos;
            mapID = newMapID;
            sessionID = newSessionID;
            instanceID = newInstanceID;
            self = newObject;
        }

        public Vector3 GetLastPos()
        {
            return lastPos;
        }

        public GameObject GetInstanceObject()
        {
            return self;
        }

        public void SetInstanceObject(GameObject ob)
        {
            self = ob;
        }

        public void SetSelf(GameObject ob)
        {
            self = ob;
        }

        public GameObject GetSelf()
        {
            return self;
        }

        public void SetPosString(string x, string y, string z, string a)
        {
            pos = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
            alpha = float.Parse(a);
        }

        public void SetLastPos(Vector3 p)
        {
            lastPos = p;
        }
        public void SetLastPosString(string x, string y, string z)
        {
            lastPos = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        }
        public void SetPos(Vector3 newPos, float newAlpha)
        {
            pos = newPos;
            alpha = newAlpha;
        }

        public void SetInstance(string x)
        {
            instanceID = x;
        }

        public string GetInstanceID()
        {
            return instanceID;
        }

        public void SetSessionID(string x)
        {
            sessionID = x;
        }
        public void SetMapID(string x)
        {
            mapID = x;
        }

        public Vector3 GetPos()
        {
            return pos;
        }

        public float GetAlpha()
        {
            return alpha;
        }

        public string GetSession()
        {
            return sessionID;
        }

        public string GetMap()
        {
            return mapID;
        }
    }

    public class SessionData
    {
        public List<Instance> instances = new List<Instance>();
        public string sessionID;
        private string mapID;
        public bool isActive = true;

        public void init(string id)
        {
            instances = new List<Instance>();
            sessionID = id;
        }

        public Instance FindInstance(string i)
        {
            foreach (Instance x in instances)
            {
                if (x.GetInstanceID() == i)
                {
                    return x;
                }
            }
            return null;
        }

        public Instance NewInstance(string instanceID, GameObject ob = null, Vector3 pos = new Vector3())
        {
            Instance i = new Instance();
            i.init(newPos: pos, newMapID: mapID, newSessionID: sessionID, newObject: ob, newInstanceID: instanceID);
            instances.Add(i);

            return i;
        }

        public int GetNumInstances()
        {
            return instances.Count;
        }

        public void Init(string id)
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

        public Instance GetInstance(string id)
        {
            foreach (Instance i in instances)
            {
                if (i.GetInstanceID() == id)
                {
                    return i;
                }
            }
            return null;
        }
    }
}
