using System.Collections;
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
        public RawImage avatar;
        public RawImage background;
        public TMP_Text playerName;
        public TMP_Text livesCounter;
    }

    [SerializeField] private LivesElem[] livesUIs = new LivesElem[4];

    public void EnableLivesUI(GameObject[] players)
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
            //livesUIs[i].playerName.color = p.playerColor; // sets the color to the color of the player
            livesUIs[i].background.color = p.playerColor; // sets the color to the color of the player
            livesUIs[i].livesCounter.text = "Lives: " + p.GetComponent<Health>().currentLives.ToString();
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
