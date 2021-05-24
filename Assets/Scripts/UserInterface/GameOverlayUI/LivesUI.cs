﻿using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System.Linq;

public class LivesUI : MonoBehaviour
{

    [SerializeField] private GameObject[] livesAnchors = new GameObject[4];

    [SerializeField] private GameObject livesUIElementPrefab;

    [SerializeField] GameUIManager gameUIManager = null;

    [SerializeField] TMP_Text conditionText;

    private LivesUIElement[] livesUIs = new LivesUIElement[4];
    private List<GameObject> playerList = new List<GameObject>();

    private LobbySettings _lobbySettings;
    private Gamemode selectedGamemode;

    private void Awake()
    {
        _lobbySettings = FindObjectOfType<LobbySettings>();
        if (_lobbySettings == null) Debug.LogError("LobbySettings not found!");

        selectedGamemode = _lobbySettings.GetGamemode();

        if (selectedGamemode is StandardGamemode)
            conditionText.text = "Last bee standing wins!";
        else if (selectedGamemode is TeamsGamemode)
            conditionText.text = "Last team standing wins!";
        else if (selectedGamemode is EliminationGamemode)
            conditionText.text = "First to " + (selectedGamemode as EliminationGamemode).eliminations + " kills win!";
    }

    public void EnableLivesUI(Player p)
    {
        // create the player card
        GameObject obj = Instantiate(
            livesUIElementPrefab,
            livesAnchors[p.playerRoomIndex].transform.position,
            livesAnchors[p.playerRoomIndex].transform.rotation,
            gameObject.transform);

        // resacle to anchor size
        obj.transform.localScale = livesAnchors[p.playerRoomIndex].transform.localScale;

        LivesUIElement elem = obj.GetComponent<LivesUIElement>();

        // add to a list
        livesUIs[p.playerRoomIndex] = elem;

        // enable ui for players
        elem.livesObject.SetActive(true);

        // set the avatar
        elem.avatar.sprite = gameUIManager.GetComponent<CharacterHelper>().GetCharImage(p.characterCode);

        // initialize username
        //elem.playerName.text = p.steamName;

        elem.player = p.gameObject;

        // add player to player list
        playerList.Add(p.gameObject);


        if (_lobbySettings.GetGamemode() is StandardGamemode)
        {
            elem.heartsObject.SetActive(true);

            // Set the lives
            for (int j = 0; j < elem.hearts.Length; j++)
            {
                if (j < p.GetComponent<Health>().maxLives)
                {
                    elem.hearts[j].SetActive(true);
                    elem.hearts[j].GetComponent<Image>().sprite = gameUIManager.GetComponent<CharacterHelper>().GetLivesImage(p.characterCode);
                } else
                {
                    elem.hearts[j].SetActive(false);
                }
            }
        }
         
        if (_lobbySettings.GetGamemode() is EliminationGamemode) elem.eliminationsObject.SetActive(true);
    }

    public void UpdateLives(int currentLives, Player player)
    {
        int i = Array.FindIndex(livesUIs, e => e.player.GetComponent<Player>().playerRoomIndex == player.playerRoomIndex);

        Debug.Log("update lives: " + currentLives);

        switch (currentLives)
        {
            case 2: { livesUIs[i].hearts[2].SetActive(false); break; }
            case 1: {
                    livesUIs[i].hearts[1].SetActive(false);
                    livesUIs[i].background.GetComponent<ColorTween>().LoopTween(); 
                    break;
                }
            case 0: {
                    livesUIs[i].hearts[0].SetActive(false);
                    livesUIs[i].background.GetComponent<ColorTween>().EndLoopTween();
                    livesUIs[i].background.color = new Vector4(.1f, .1f, .1f, 1f); // not working for some reason
                    livesUIs[i].avatar.color = new Vector4(.5f, .5f, .5f, 5f);
                    break;
            }
        }
    }

    public void UpdateCombos(int combos, Player player)
    {

    }

    public void UpdateEliminations(int killAmt, Player player)
    {
        int i = Array.FindIndex(livesUIs, e => e.player.GetComponent<Player>().playerRoomIndex == player.playerRoomIndex);

        livesUIs[i].elimsText.text = killAmt.ToString();
    }

    public void UpdateOrdering()
    {
        // Recalculate the winning order
        GameObject[] orderedList = _lobbySettings.GetGamemode().GetWinningOrder(playerList.ToArray());

        // Reorder livesUI based on new list 
        livesUIs = livesUIs.OrderBy(e => e == null ? 4 : Array.IndexOf(orderedList, e.player)).ToArray();


        for (int i = 0; i < playerList.Count; i++)
        {
            // Move lives ui to the new positions
            LeanTween.moveLocal(livesUIs[i].gameObject, livesAnchors[i].transform.localPosition, 1f)
                .setEase(LeanTweenType.easeOutExpo);
        }
    }
}
