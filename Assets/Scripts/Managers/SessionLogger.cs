using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SessionLogger : MonoBehaviour
{
    [Serializable] public class Session
    {
        public string sessionId;
        public string version;
        public string duration;
        public string[] players;
        public Event[] events;
    }

    // The current active session/round in game
    private Session curSession;

    [Serializable] public class Event
    {
        public string time;
        public string pid;
        public string code;
        public string info;
    }

    // List of events
    private List<Event> events = new List<Event>();

    // Endpoint URL to post session data
    [SerializeField] public string devEndpoint = "http://localhost:3000/session";
    [SerializeField] public string prodEndpoint = "";
    [SerializeField] public bool useDevelopmentEndpoint = true;

    // Permissions
    [HideInInspector] public bool sendPermissions = false; // whether the user chose to send us data or not
    private bool permissionsShown = false; // whether we already shown the popup to the user already this session
    [SerializeField] private GameObject permissionsPopup; // the permissions popup that requets user to send data

    // Debug
    [SerializeField] private bool enableDebugging = false;

    // singleton
    public static SessionLogger _instance;
    public static SessionLogger Singleton { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: RoundManager");
        else _instance = this;
    }

    #region API Handler

    public void SendData()
    {
        Debug.Log("Sending data...");

        string url = devEndpoint;

        // Use production endpoint if not dev
        if (!useDevelopmentEndpoint) url = prodEndpoint;

        StartCoroutine(PostData(url, JsonUtility.ToJson(curSession)));
    }

    IEnumerator PostData(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);
    }

    #endregion

    #region Data Collection

    // Called immediately when round starts (after all players join)
    public void InitializeSession()
    {
        // Create new session
        curSession = new Session();

        // Set session ID
        DateTimeOffset curDate = new DateTimeOffset(DateTime.UtcNow);
        curSession.sessionId = curDate.ToUnixTimeMilliseconds().ToString();

        // Set game version
        curSession.version = Application.version;
    }

    // Called immediately when round ends (before scene is switched)
    public void CollectData(float duration, List<Player> players)
    {
        if (curSession == null) { Debug.LogWarning("Session not initialized!"); return; }

        // Sort events chronologically (just in case)
        events.Sort((e1, e2) => float.Parse(e1.time).CompareTo(float.Parse(e2.time)));

        // Set total duration of the game
        curSession.duration = duration.ToString("F2");

        // Append all players
        curSession.players = new string[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            curSession.players[i] = GetPlayerId(players[i].gameObject);
        }

        // Append all events
        curSession.events = events.ToArray();

        // Clear local events list
        events.Clear();

        // Automatically send data if permitted
        if (sendPermissions) SendData();
    }

    #endregion

    #region Popup Dialog

    public void ShowPermsPopup()
    {
        if (!permissionsShown) permissionsPopup.SetActive(true);
    }

    public void OnClickPermsSendButton()
    {
        sendPermissions = true;
        permissionsShown = true;
        permissionsPopup.SetActive(false);

        // Send data if user permits
        SendData();
    }

    public void OnClickPermsCloseButton()
    {
        permissionsShown = true;
        permissionsPopup.SetActive(false);
    }

    #endregion

    #region Event Creation

    public void CreateEventBOM(GameObject bomb, GameObject placerPlayer, float time)
    {
        Event e = new Event();
        e.time = time.ToString("F2");
        e.pid = GetPlayerId(placerPlayer);
        e.code = "BOM";
        e.info = GetBombCode(bomb);
        events.Add(e);

        if (enableDebugging) LoggerDebug(JsonUtility.ToJson(e));
    }

    public void CreateEventDMG(GameObject bomb, GameObject damagedPlayer, GameObject causingPlayer, float time)
    {
        Event e = new Event();
        e.time = time.ToString("F2");
        e.pid = GetPlayerId(damagedPlayer);
        e.code = "DMG";
        e.info = GetBombCode(bomb) + ',' + GetPlayerId(causingPlayer);
        events.Add(e);

        if (enableDebugging) LoggerDebug(JsonUtility.ToJson(e));
    }

    public void CreateEventSWP(char oldKey, char newKey, bool isCombo, GameObject player, float time)
    {
        Event e = new Event();
        e.time = time.ToString("F2");
        e.pid = GetPlayerId(player);
        e.code = "SWP";
        e.info = newKey.ToString() + "," + oldKey.ToString();
        if (isCombo) e.info = e.info + ",1";
        else e.info = e.info + ",0";
        events.Add(e);

        if (enableDebugging) LoggerDebug(JsonUtility.ToJson(e));
    }

    public void CreateEventSPN(GameObject player, GameObject bomb, float time)
    {
        Event e = new Event();
        e.time = time.ToString("F2");
        e.pid = GetPlayerId(player);
        e.code = "SPN";
        if (bomb != null) e.info = GetBombCode(bomb);
        events.Add(e);

        if (enableDebugging) LoggerDebug(JsonUtility.ToJson(e));
    }

    #endregion

    #region Helper Functions

    /// List of Bomb Codes: 
    /// DEF - default bomb
    /// LAS - laser bomb
    /// PLA - plasma bomb
    /// BLK - blink bomb
    /// GRA - gravity bomb
    private string GetBombCode(GameObject bomb)
    {
        if (bomb.GetComponent<BombObject>() != null)
        {
            return "DEF";
        }
        else if (bomb.GetComponent<LaserObject>() != null)
        {
            return "LAS";
        }
        else if (bomb.GetComponent<PlasmaObject>() != null)
        {
            return "PLA";
        }
        else if (bomb.GetComponent<BlinkObject>() != null)
        {
            return "BLK";
        }
        else if (bomb.GetComponent<GravityObject>() != null)
        {
            return "GRA";
        } else
        {
            Debug.LogError("Could not get bomb type!");
            return "";
        }
    }

    private string GetPlayerId(GameObject player)
    {
        Player p = player.GetComponent<Player>();

        if (p.steamId != 0)
        {
            // Get steam id in string
            string uuid = p.steamId.ToString();

            // Compute hash for steam id (to hide user identifcation) 
            string hashedId = ComputeSHA256Hash(uuid);

            // Truncate first 8 characters of hashed output
            return hashedId.Substring(0, 8);
        } else if (p.playerId != 0)
        {
            // Get player id in string
            string uuid = p.playerId.ToString();

            // Compute hash for player id
            string hashedId = ComputeSHA256Hash(uuid);

            // Truncate first 8 characters of hashed output
            return hashedId.Substring(0, 8);
        } else
        {
            Debug.LogError("Could not get playerID and steamId");
            return "";
        }
    }

    private static string ComputeSHA256Hash(string value)
    {
        StringBuilder Sb = new StringBuilder();

        using (SHA256 hash = SHA256Managed.Create())
        {
            Encoding enc = Encoding.UTF8;
            byte[] result = hash.ComputeHash(enc.GetBytes(value));

            foreach (byte b in result)
                Sb.Append(b.ToString("x2"));
        }

        return Sb.ToString();
    }

    private void LoggerDebug(string s)
    {
        Debug.Log("[SessionLogger]: " + s);
    }

    #endregion
}