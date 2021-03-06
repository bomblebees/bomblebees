using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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

    [Header("Start Round")]
    [SerializeField] private GameObject startGameUI;
    [SerializeField] private TMP_Text waitText;
    [SerializeField] private TMP_Text timerText;

    [Header("End Round")]
    [SerializeField] private GameObject endGameUI;

    [Header("Lives")]
    [SerializeField] private LivesUI[] livesUIs = new LivesUI[4];
    //[SerializeField] private TMP_Text playerLivesText;

    // singletons
    public static GameUIManager _instance;
    public static GameUIManager Singleton { get { return _instance; } }

    private RoundManager roundManager;

    private void Awake()
    {
        if (_instance != null && _instance != this) Debug.LogError("Multiple instances of singleton: RoundManager");
        else _instance = this;

        roundManager = RoundManager.Singleton;
        if (roundManager == null) Debug.LogError("Cannot find Singleton: RoundManager");
    }

    public override void OnStartServer()
    {
        // Subscribe to server round events
        roundManager.EventPlayerConnected += ServerPlayerConnected;
        roundManager.EventRoundStart += ServerStartRound;
    }

    [Client]
    public override void OnStartClient()
    {
        // Subscribe to client round events
        roundManager.EventRoundStart += ClientStartRound;
        roundManager.EventRoundEnd += ClientEndRound;
    }

    [Server] public void ServerPlayerConnected(RoundManager.PlayerInfo p)
    {
        ServerEnableLivesUI();
        RpcPlayerConnected(roundManager.playersConnected, roundManager.totalRoomPlayers);
    }

    [ClientRpc] public void RpcPlayerConnected(int connPlayers, int totalPlayers)
    {
        UpdateRoundWaitUI(connPlayers, totalPlayers);
    }

    #region Round Start & End

    [Client] public void ServerStartRound() { ServerEnableLivesUI(); }
    [Client] public void ClientStartRound() { StartCoroutine(StartRoundFreezetime()); }
    [Client] public void ClientEndRound() { StartCoroutine(EndRoundFreezetime()); }

    public IEnumerator StartRoundFreezetime()
    {
        waitText.text = "All players loaded!";
        yield return new WaitForSeconds(1);
        waitText.text = "Game Starting in";
        timerText.gameObject.SetActive(true);

        for (int i = roundManager.startGameFreezeDuration; i > 0; i--)
        {
            timerText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        waitText.gameObject.SetActive(false);
        timerText.text = "Begin!";

        yield return new WaitForSeconds(1);
        startGameUI.SetActive(false);
    }

    public IEnumerator EndRoundFreezetime()
    {
        endGameUI.SetActive(true);
        yield return new WaitForSeconds(roundManager.endGameFreezeDuration);
        endGameUI.SetActive(false);
    }

    public void UpdateRoundWaitUI(int connPlayers, int totalPlayers)
    {
        // ex. Waiting for players... (2/4)
        waitText.text = "Waiting for players... (" +
            connPlayers +
            "/" +
            totalPlayers +
            ")";
    }

    #endregion

    #region Lives

    //[Server]
    //public void ServerEnableLivesUI(ulong steamId)
    //{
    //    List<RoundManager.PlayerInfo> playerList = roundManager.playerList;

    //    // Set steam avatar if using steam
    //    if (steamId != 0)
    //    {
    //        // Find the index for the player
    //        int idx = playerList.FindIndex(p => p.steamId == steamId);

    //        // Get the image and username
    //        CSteamID steamID = new CSteamID(steamId);
    //        string steamName = SteamFriends.GetFriendPersonaName(steamID);
    //        int imageId = SteamFriends.GetLargeFriendAvatar(steamID);

    //        RpcClientEnableLivesUI(idx, imageId, steamName);
    //    }
    //}

    //[ClientRpc]
    //public void RpcClientEnableLivesUI(int idx, int imageId, string name)
    //{
    //    // Enable the lives object
    //    if (!livesUIs[idx].livesObject.activeSelf) livesUIs[idx].livesObject.SetActive(true);

    //    // Set the player name
    //    livesUIs[idx].playerName.text = name;

    //    // Set the player avatar, if applicable
    //    if (imageId == -1) return;
    //    livesUIs[idx].avatar.texture = GetSteamImageAsTexture(imageId);
    //}

    //[Server]
    //public void ServerUpdateLives(int currentHealth, int maxHealth)
    //{
    //    List<RoundManager.PlayerInfo> playerList = roundManager.playerList;

    //    for (int i = 0; i < playerList.Count; i++)
    //    {
    //        Health life = playerList[i].health;

    //        RpcClientUpdateLives(i, life.currentLives, playerList[i].steamId); // update UI for each player
    //    }
    //}

    //[ClientRpc]
    //public void RpcClientUpdateLives(int idx, int lifeCount, ulong steamId)
    //{
    //    // placeholder username
    //    string userName = "[Player Name]";

    //    // Set steam user name if applicable
    //    if (steamId != 0)
    //    {
    //        CSteamID steamID = new CSteamID(steamId);

    //        userName = SteamFriends.GetFriendPersonaName(steamID);
    //    }

    //    // initialize health and username
    //    livesUIs[idx].playerName.text = userName + ": " + lifeCount.ToString();
    //}

    [Server]
    public void ServerEnableLivesUI()
    {
        List<RoundManager.PlayerInfo> playerList = roundManager.playerList;

        for (int i = 0; i < playerList.Count; i++)
        {
            Health life = playerList[i].health;

            RpcEnableLivesUI(i, life.currentLives, playerList[i].steamId); // enable and init UI for each player

            // subscribe to all lives of player
            life.EventLivesChanged += ServerUpdateLives;
        }
    }

    [ClientRpc]
    public void RpcEnableLivesUI(int idx, int lifeCount, ulong steamId)
    {
        // enable ui for players
        livesUIs[idx].livesObject.SetActive(true);

        string userName = "[Player Name]";

        // Set steam user name and avatars
        if (steamId != 0)
        {
            CSteamID steamID = new CSteamID(steamId);

            userName = SteamFriends.GetFriendPersonaName(steamID);

            int imageId = SteamFriends.GetLargeFriendAvatar(steamID);

            if (imageId == -1) return;

            livesUIs[idx].avatar.texture = GetSteamImageAsTexture(imageId);
        }

        // initialize health and username
        livesUIs[idx].playerName.text = userName;
        livesUIs[idx].livesCounter.text = "Lives: " + lifeCount.ToString();
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
}
