using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamNetworkManager : NetworkRoomManagerExt
{
    // Temp list of player colors
    private Color[] listColors = { Color.red, Color.blue, Color.yellow, Color.green };

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
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

    public override void OnRoomServerPlayersReady()
    {
        showStartButton = true;
    }

    public override void OnRoomServerPlayersNotReady()
    {
        showStartButton = false;
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        MainMenu_UI menu = MainMenu_UI.singleton;

        menu.screenLoading.SetActive(false);
        menu.screenNavigation.SetActive(true);
        //menu.screenError.SetActive(true);


        base.OnClientDisconnect(conn);
    }
}
