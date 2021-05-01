﻿using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class LivesUI : MonoBehaviour
{
    [Serializable]
    public class LivesElem
    {
        public GameObject livesObject;
        public Image avatar;
        public RawImage background;
        public GameObject heart1;
        public GameObject heart2;
        public TMP_Text playerName;
        public TMP_Text livesCounter;
    }

    [SerializeField] private LivesElem[] livesUIs = new LivesElem[4];
    [SerializeField] GameUIManager gameUIManager = null;


    public void EnableLivesUI(GameObject[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i].GetComponent<Player>();

            LivesElem elem = livesUIs[i];

            // enable ui for players
            elem.livesObject.SetActive(true);

            //// Set steam user avatar
            //if (p.steamId != 0)
            //{
            //    CSteamID steamID = new CSteamID(p.steamId);
            //    int imageId = SteamFriends.GetLargeFriendAvatar(steamID);
            //    if (imageId == -1) return;
            //    elem.avatar.texture = GetSteamImageAsTexture(imageId);
            //}

            elem.avatar.sprite = gameUIManager.GetComponent<CharacterHelper>().GetCharImage(p.characterCode);
                
            // initialize health and username
            elem.playerName.text = p.steamName;
            //livesUIs[i].playerName.color = p.playerColor; // sets the color to the color of the player
            //elem.background.color = p.playerColor; // sets the color to the color of the player

            // Set the lives
            elem.livesCounter.text = "Lives: " + p.GetComponent<Health>().currentLives.ToString();

            elem.heart1.SetActive(true);
            elem.heart1.GetComponent<Image>().sprite = gameUIManager.GetComponent<CharacterHelper>().GetLivesImage(p.characterCode);

            elem.heart2.SetActive(true);
            elem.heart2.GetComponent<Image>().sprite = gameUIManager.GetComponent<CharacterHelper>().GetLivesImage(p.characterCode);
            
        }

    }

    public void UpdateLives(GameObject[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            // If player name has not been updated, initialize it
            if (livesUIs[i].playerName.text.Length <= 0)
            {
                ulong steamId = players[i].GetComponent<Player>().steamId;

                string userName = "[Player Name]";

                // Set steam user name and avatars
                if (steamId != 0)
                {
                    CSteamID steamID = new CSteamID(steamId);

                    userName = SteamFriends.GetFriendPersonaName(steamID);
                }

                // Update username
                livesUIs[i].playerName.text = userName;
            }

            // Update health
            int lifeCount = players[i].GetComponent<Health>().currentLives;

            livesUIs[i].livesCounter.text = "Lives: " + lifeCount.ToString();

            if (lifeCount <= 1) { livesUIs[i].heart2.SetActive(false); livesUIs[0].background.GetComponent<ColorTween>().LoopTween(); }
            if (lifeCount <= 0) { livesUIs[i].heart1.SetActive(false); }
        }
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
}
