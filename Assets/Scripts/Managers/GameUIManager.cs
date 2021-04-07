using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;
using Steamworks;

public class GameUIManager : NetworkBehaviour
{
    [Serializable]
    public class LivesUI
    {
        public GameObject livesObject;
        public RawImage avatar;
        public TMP_Text playerName;
        public TMP_Text livesCounter;
    }

    [SerializeField] private RoundStartEnd roundStartEnd = null;


    [Header("Lives")]
    [SerializeField] private LivesUI[] livesUIs = new LivesUI[4];
    
    [Header("Killfeed")]
    [SerializeField] private GameObject killFeedPrefab;
    [SerializeField] private GameObject killFeedCanvas;
    [SerializeField] private float fadeDelay = 4f;

    private List<GameObject> feedUIs = new List<GameObject>();
    //private List<string> feedTexts = new List<string>(6);

    // singletons
    public static GameUIManager _instance;
    public static GameUIManager Singleton { get { return _instance; } }

    private RoundManager roundManager;
    private EventManager eventManager;

    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: RoundManager");
        else _instance = this;
    }

    private void InitSingletons()
    {
        roundManager = RoundManager.Singleton;
        if (roundManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");
    }

    public override void OnStartServer()
    {
        InitSingletons();

        // Subscribe to server round events
        roundManager.EventPlayerConnected += ServerPlayerConnected;
        roundManager.EventRoundStart += ServerStartRound;

        eventManager.EventPlayerTookDamage += ServerOnKillEvent;
    }

    [Client]
    public override void OnStartClient()
    {
        InitSingletons();

        // Subscribe to client round events
        roundManager.EventRoundStart += ClientStartRound;
        roundManager.EventRoundEnd += ClientEndRound;
    }

    // When a player loads into the game (on server)
    [Server] public void ServerPlayerConnected(RoundManager.PlayerInfo p)
    {
        RpcPlayerConnected(roundManager.playersConnected, roundManager.totalRoomPlayers);
    }

    // When a player loads into the game (on client)
    [ClientRpc] public void RpcPlayerConnected(int connPlayers, int totalPlayers)
    {
        roundStartEnd.UpdateRoundWaitUI(connPlayers, totalPlayers);
    }

    #region Round Start & End

    [Client] public void ClientStartRound()
    {
        StartCoroutine(roundStartEnd.StartRoundFreezetime(roundManager.startGameFreezeDuration));
    }
    [Client] public void ClientEndRound()
    {
        StartCoroutine(roundStartEnd.EndRoundFreezetime(roundManager.startGameFreezeDuration));
    }

    #endregion

    #region Lives

    [Server] public void ServerStartRound() { ServerEnableLivesUI(); }

    [Server]
    public void ServerEnableLivesUI()
    {
        // Convert playerinfo struct to network transportable gameObjects
        List<RoundManager.PlayerInfo> playerList = roundManager.playerList;

        GameObject[] playerObjects = new GameObject[playerList.Count];

        for (int i = 0; i < playerList.Count; i++)
        {
            playerObjects[i] = playerList[i].player.gameObject;

            // Subscribe to lives change event for specific player
            playerList[i].health.EventLivesChanged += ServerUpdateLives;
        }

        RpcEnableLivesUI(playerObjects);



    }

    [ClientRpc]
    public void RpcEnableLivesUI(GameObject[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i].GetComponent<Player>();

            // enable ui for players
            livesUIs[i].livesObject.SetActive(true);

            // Set steam user avatar
            if (p.steamId != 0)
            {
                CSteamID steamID = new CSteamID(p.steamId);
                int imageId = SteamFriends.GetLargeFriendAvatar(steamID);
                if (imageId == -1) return;
                livesUIs[i].avatar.texture = GetSteamImageAsTexture(imageId);
            }

            // initialize health and username
            livesUIs[i].playerName.text = p.steamName;
            livesUIs[i].playerName.color = p.playerColor; // sets the color to the color of the player
            livesUIs[i].livesCounter.text = "Lives: " + p.GetComponent<Health>().currentLives.ToString();
        }

    }

    // technical debt: having reference to individual player whose live changed is better
    [Server]
    public void ServerUpdateLives(int currentHealth, int maxHealth)
    {
        List<RoundManager.PlayerInfo> playerList = roundManager.playerList;

        for (int i = 0; i < playerList.Count; i++)
        {
            Health life = playerList[i].health;

            RpcClientUpdateLives(i, life.currentLives, playerList[i].steamId); // update UI for each player
        }
    }

    [ClientRpc]
    public void RpcClientUpdateLives(int idx, int lifeCount, ulong steamId)
    {

        string userName = "[Player Name]";

        // Set steam user name and avatars
        if (steamId != 0)
        {
            CSteamID steamID = new CSteamID(steamId);

            userName = SteamFriends.GetFriendPersonaName(steamID);
        }

        // initialize health and username
        livesUIs[idx].playerName.text = userName;
        livesUIs[idx].livesCounter.text = "Lives: " + lifeCount.ToString();
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        return texture;
    }

    #endregion

    #region Killfeed

    [Server]
    public void ServerOnKillEvent(int newLives, GameObject bomb, GameObject player)
    {
        RpcOnKillEvent(newLives, bomb, player);
    }

    [ClientRpc]
    public void RpcOnKillEvent(int newLives, GameObject bomb, GameObject player)
    {
        string killtext = GetPlayerText(player) + " died to " + GetBombText(bomb);

        // Create the killfeed object
        GameObject killfeed = Instantiate(
            killFeedPrefab,
            new Vector3(0, 0, 0),
            Quaternion.identity,
            killFeedCanvas.transform);

        // Set the text
        killfeed.GetComponent<TMP_Text>().text = killtext;

        // Move the position to bottom right corner
        killfeed.GetComponent<RectTransform>().anchoredPosition = new Vector3(-180, 50 + (feedUIs.Count * 50), 0);

        // Add killfeed to list
        feedUIs.Add(killfeed);

        // Start the fade tween animation
        killfeed.GetComponent<TMP_Text>().LeanAlphaText(0, 1).setDelay(fadeDelay).setOnComplete(() => OnKillFeedFadeComplete(killfeed));
    }

    [Client]
    public void OnKillFeedFadeComplete(GameObject killfeed)
    {
        // Save base position (probably better way to do this?)
        float yPosBase = feedUIs[0].transform.localPosition.y;

        // Remove the list
        feedUIs.Remove(killfeed);

        // Destroy the object
        Destroy(killfeed);

        // Update the new killfeed
        ClientUpdateKillfeed(yPosBase);
    }

    [Client]
    public void ClientUpdateKillfeed(float yPosBase)
    {
        for (int i = 0; i < feedUIs.Count; i++)
        {
            LeanTween.moveLocalY(feedUIs[i], yPosBase + (i * 50), 0.5f).setEase(LeanTweenType.easeOutExpo);
        }
    }


    //private IEnumerator StartFade(int idx)
    //{
    //    yield return new WaitForSeconds(fadeDelay);

    //    Debug.Log("start tween");
    //    //feedUIs[0].GetComponent<TMP_Text>().color = Color.clear;

    //    //feedUIs[idx].GetComponent<TMP_Text>().LeanAlphaText(0, 1).setOnComplete(OnFeedFaded);

    //    //LeanTween.alphaText(feedUIs[0].GetComponent<TMP_Text>(), )
    //    //LeanTween.alphaText(feedUIs[0].GetComponent<TMP_Text>(), 0f, 3f);
    //}

    //private void OnFeedFaded()
    //{

    //}


    private string GetPlayerText(GameObject player)
    {
        Player p = player.GetComponent<Player>();
        return "<b><color=#" + ColorUtility.ToHtmlStringRGB(p.playerColor) + ">" + p.steamName + "</color></b>";
    }

    private string GetBombText(GameObject bomb)
    {
        if (bomb.GetComponent<BombObject>() != null)
        {
            return "<color=#B2B2B2>Default Bomb</color>";
        }
        else if (bomb.GetComponent<LaserObject>() != null)
        {
            return "<color=#F9FF23>Laser Bomb</color>";
        }
        else if (bomb.GetComponent<PlasmaObject>() != null)
        {
            return "<color=#17E575>Plasma Bomb</color>";
        }
        else if (bomb.GetComponent<BlinkObject>() != null)
        {
            return "<color=#00D9FF>Blink Bomb</color>";
        }
        else if (bomb.GetComponent<SludgeObject>() != null)
        {
            return "<color=#F153FF>Gravity Bomb</color>";
        }
        else
        {
            Debug.LogError("Could not get bomb type!");
            return "";
        }
    }
    #endregion
}
