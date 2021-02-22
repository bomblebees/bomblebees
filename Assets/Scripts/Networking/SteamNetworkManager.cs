﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamNetworkManager : NetworkRoomManagerExt
{
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


        //return newRoomGameObject;
        //Debug.Log("SERVER ADD PLAYER " + conn.address);

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
        foreach(NetworkRoomPlayerExt p in roomSlots)
        {
            showStartButton = true;
            p.UpdateLobbyList();
        }
    }

    public override void OnRoomServerPlayersNotReady()
    {
        foreach (NetworkRoomPlayerExt p in roomSlots)
        {
            showStartButton = false;
            p.UpdateLobbyList();
        }
    }

    //public override void OnRoomClientEnter()
    //{
    //    Debug.Log("i joined: " + SteamFriends.GetPersonaName());
    //}
}