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
        public Image avatar;
        public RawImage background;
        public GameObject[] hearts;
        public TMP_Text playerName;
        public TMP_Text livesCounter;
    }

    //[SerializeField] private LivesElem[] livesUIs = new LivesElem[4];

    [SerializeField] private GameObject[] livesAnchors = new GameObject[4];

    [SerializeField] private GameObject livesUIElementPrefab;

    [SerializeField] GameUIManager gameUIManager = null;

    private List<LivesUIElement> livesUIs = new List<LivesUIElement>();

    public void EnableLivesUI(GameObject[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i].GetComponent<Player>();

            // create the player card
            GameObject obj = Instantiate(
                livesUIElementPrefab,
                new Vector3(0, 0, 0),
                Quaternion.identity,
                livesAnchors[i].transform);

            // to make sure its positioned at 0 0 0 locally
            obj.transform.localPosition = new Vector3(0, 0, 0);

            LivesUIElement elem = obj.GetComponent<LivesUIElement>();

            // add to a list
            livesUIs.Add(elem);

            // enable ui for players
            elem.livesObject.SetActive(true);

            // set the avatar
            elem.avatar.sprite = gameUIManager.GetComponent<CharacterHelper>().GetCharImage(p.characterCode);

            // initialize username
            //elem.playerName.text = p.steamName;

            // Set the lives
            for (int j = 0; j < elem.hearts.Length; j++)
            {
                elem.hearts[j].SetActive(true);
                elem.hearts[j].GetComponent<Image>().sprite = gameUIManager.GetComponent<CharacterHelper>().GetLivesImage(p.characterCode);
            }
        }

    }

    public void UpdateLives(int currentLives, Player player)
    {
        int i = player.playerListIndex;

        // If player name has not been updated, initialize it
        if (livesUIs[i].playerName.text.Length <= 0)
        {
            ulong steamId = player.steamId;

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
        int lifeCount = currentLives;

        Debug.Log("player " + i + " has " + lifeCount + " lives");

        livesUIs[i].livesCounter.text = "Lives: " + lifeCount.ToString();

        switch (lifeCount)
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
