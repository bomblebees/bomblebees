using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;
using Mirror;

public class RoundManager : NetworkBehaviour
{
    private List<Health> playerLives = new List<Health>();
    private List<Player> playerList = new List<Player>();

    [Header("Start Game")]
    [SerializeField] private GameObject startGameUI;
    [SerializeField] private TMP_Text loadText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private int startGameFreezeDuration = 5;

    [Header("End Game")]
    [SerializeField] private GameObject endGameUI;
    [SerializeField] private int endGameFreezeDuration = 5;

    private int playersConnected = 0;
    private int totalRoomPlayers;

    public override void OnStartServer()
    {
        NetworkRoomManagerExt room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        totalRoomPlayers = room.roomSlots.Count;
        loadText.text = "Waiting for players... (0/" + totalRoomPlayers + ")";
        //Debug.Log("total room players: " + totalRoomPlayers);
    }
    
    [Client]
    public override void OnStartClient()
    {
        CmdAddPlayerToRound();
        //NetworkRoomManagerExt.OnServerReadied += Test;
    }

    [Command(ignoreAuthority = true)]
    public void CmdAddPlayerToRound(NetworkConnectionToClient sender = null)
    {
        playersConnected++;
        loadText.text = "Waiting for players... (" + playersConnected + "/" + totalRoomPlayers + ")";
        Debug.Log("player " + playersConnected + " has loaded");

        // Add player to list
        Player player = sender.identity.gameObject.GetComponent<Player>();
        playerList.Add(player);

        // Add player's lives to list
        Health live = sender.identity.gameObject.GetComponent<Health>();
        live.EventLivesChanged += CheckRoundEnd;
        playerLives.Add(live);

        if (totalRoomPlayers == playersConnected)
        {
            StartRound();
        }
    }

    #region Round Start

    [Server]
    public void StartRound()
    {
        RpcStartRound();
        StartCoroutine(ServerStartRound());
    }

    [ClientRpc]
    public void RpcStartRound()
    {
        StartCoroutine(ClientStartRound());
    }

    [Server]
    public IEnumerator ServerStartRound()
    {
        yield return new WaitForSeconds(startGameFreezeDuration + 1);
        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].isFrozen = false;
        }
        RpcUnfreezePlayers();
    }

    [ClientRpc]
    public void RpcUnfreezePlayers()
    {
        GameObject.Find("LocalPlayer").GetComponent<Player>().isFrozen = false;
    }

    [Client]
    public IEnumerator ClientStartRound()
    {
        loadText.text = "All players loaded!";
        yield return new WaitForSeconds(1);
        loadText.text = "Game Starting in";
        timerText.gameObject.SetActive(true);

        for (int i = startGameFreezeDuration; i > 0; i--)
        {
            timerText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        loadText.gameObject.SetActive(false);
        timerText.text = "Begin!";

        yield return new WaitForSeconds(1);
        startGameUI.SetActive(false);
    }

    #endregion

    #region Round End

    [ServerCallback]
    public void CheckRoundEnd(int currentHealth, int maxHealth)
    {
        if (currentHealth < 1)
        {
            int aliveCount = 0;
            for (int i = 0; i < playerLives.Count; i++)
            {
                //Debug.Log("ROUND MANAGER: player " + i + " has lives: " + playerLives[i].currentLives);
                if (playerLives[i].currentLives > 0)
                {
                    aliveCount++;
                }
            }

            // End the round/game if only one player alive
            if (aliveCount <= 1)
            {
                EndRound();
            }
        }
    }

    [Server]
    public void EndRound()
    {
        Debug.Log("ending game");
        RpcEndRound();
        StartCoroutine(ServerEndRound());
    }

    [ClientRpc]
    public void RpcEndRound()
    {
        StartCoroutine(ClientEndRound());
    }

    [Client]
    public IEnumerator ClientEndRound()
    {
        endGameUI.SetActive(true);
        yield return new WaitForSeconds(endGameFreezeDuration);
        endGameUI.SetActive(false);
    }

    [Server]
    public IEnumerator ServerEndRound()
    {
        yield return new WaitForSeconds(endGameFreezeDuration);
        NetworkRoomManagerExt room = NetworkRoomManager.singleton as NetworkRoomManagerExt;
        room.ServerChangeScene(room.RoomScene);
    }

    #endregion
}
