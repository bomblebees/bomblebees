﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamNetworkManager : NetworkRoomManagerExt
{
    // Temp list of player colors
    private Color[] listColors = { Color.red, Color.blue, Color.yellow, Color.green };

    /// <summary>
    /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
    /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
    /// into the GamePlayer object as it is about to enter the Online scene.
    /// </summary>
    /// <param name="roomPlayer"></param>
    /// <param name="gamePlayer"></param>
    /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        // transfer steam id to player
        gamePlayer.GetComponent<Player>().steamId = roomPlayer.GetComponent<NetworkRoomPlayerExt>().steamId;
        gamePlayer.GetComponent<Player>().steamName = roomPlayer.GetComponent<NetworkRoomPlayerExt>().steamUsername;
        gamePlayer.GetComponent<Player>().playerColor = listColors[roomPlayer.GetComponent<NetworkRoomPlayerExt>().characterCode];
        gamePlayer.GetComponent<Player>().characterCode = roomPlayer.GetComponent<NetworkRoomPlayerExt>().characterCode;

        return true;
    }

    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnection conn)
    {
        GameObject newRoomGameObject = Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);

        if (conn.address == "localhost")
        {
            AssignRoomPlayerSteamInfo(newRoomGameObject, SteamUser.GetSteamID().m_SteamID);
        } else
        {
            AssignRoomPlayerSteamInfo(newRoomGameObject, UInt64.Parse(conn.address));
        }

        return newRoomGameObject;
    }

    public GameObject AssignRoomPlayerSteamInfo(GameObject roomObject, ulong steamID)
    {
        NetworkRoomPlayerExt roomPlayer = roomObject.GetComponent<NetworkRoomPlayerExt>();

        CSteamID csteamid = new CSteamID(steamID);

        roomPlayer.steamId = steamID;
        roomPlayer.steamUsername = SteamFriends.GetFriendPersonaName(csteamid);
        roomPlayer.steamAvatarId = SteamFriends.GetLargeFriendAvatar(csteamid);

        return roomObject;
    }

    public Texture2D GetSteamImageAsTexture(int iImage)
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

    public override void OnRoomServerPlayersReady()
    {
        showStartButton = true;
        //foreach (NetworkRoomPlayerExt p in roomSlots)
        //{
        //    //p.UpdateLobbyList();
        //}
    }

    public override void OnRoomServerPlayersNotReady()
    {
        showStartButton = false;
        //foreach (NetworkRoomPlayerExt p in roomSlots)
        //{
        //    //p.UpdateLobbyList();
        //}
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        MainMenu_UI menu = MainMenu_UI.singleton;

        menu.screenLoading.SetActive(false);
        menu.screenNavigation.SetActive(true);
        //menu.screenError.SetActive(true);


        base.OnClientDisconnect(conn);
    }

    //public override void OnRoomClientEnter()
    //{
    //    Debug.Log("i joined: " + SteamFriends.GetPersonaName());
    //}
}
