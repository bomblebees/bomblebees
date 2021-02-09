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
    [SerializeField] private GameObject endGameUI;
    [SerializeField] private int endGameFreezeDuration = 5;

    public override void OnStartServer()
    {
        NetworkManagerLobby.OnServerReadied += Test;
    }
    
    [Server]
    public void Test(NetworkConnection conn)
    {
        Health live = conn.identity.gameObject.GetComponent<Health>();
        live.EventLivesChanged += CheckRoundEnd;
        playerLives.Add(live);
        Debug.Log("added player " + playerLives.Count);
    }

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
        Debug.Log("moving back to lobby");
    }

}
