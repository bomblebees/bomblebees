using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    [SyncVar] public ulong steamId;
    [SyncVar] public string steamUsername;
    [SyncVar] public int steamAvatarId;

    Room_UI roomUI;
    

    public override void OnStartClient()
    {
        roomUI = Room_UI.singleton;
        roomUI.EventReadyButtonClicked += OnReadyButtonClick;
        roomUI.EventStartButtonClicked += OnStartButtonClick;

        base.OnStartClient();
    }

    public override void OnClientEnterRoom()
    {
        UpdateLobbyList();
    }

    public override void OnClientExitRoom()
    {
        SteamNetworkManager room = NetworkManager.singleton as SteamNetworkManager;
        Debug.Log("exit room count" + room.roomSlots.Count);
        UpdateLobbyList();
    }

    public override void ReadyStateChanged(bool _, bool newReadyState)
    {
        UpdateLobbyList();
    }

    public void UpdateLobbyList()
    {
        if (!roomUI)
        {
            // reenable and resubscribe to events
            roomUI = Room_UI.singleton;
            roomUI.EventReadyButtonClicked += OnReadyButtonClick;
            roomUI.EventStartButtonClicked += OnStartButtonClick;

            if (!roomUI)
            {
                Debug.LogWarning("room UI not found!");
                return;
            }
        }

        SteamNetworkManager room = NetworkManager.singleton as SteamNetworkManager;

        // update all existing players
        for (int i = 0; i < 4; i++)
        {
            Room_UI.PlayerLobbyCard card = roomUI.playerLobbyUi[i];

            // if player does not exist
            if (i >= room.roomSlots.Count)
            {
                card.playerCard.SetActive(false);
                break;
            }

            NetworkRoomPlayerExt player = room.roomSlots[i] as NetworkRoomPlayerExt;

            CSteamID steamid = new CSteamID(player.steamId);

            // Player list background
            card.playerCard.SetActive(true);

            // User name
            card.username.text = player.steamUsername;

            // User avatar
            card.avatar.texture = room.GetSteamImageAsTexture(player.steamAvatarId);

            // Ready check mark
            if (player.readyToBegin) card.readyStatus.SetActive(true);
            else card.readyStatus.SetActive(false);
        }

        // Start button
        if (room.allPlayersReady && room.showStartButton)
        {
            roomUI.buttonStart.SetActive(true);
        } else
        {
            roomUI.buttonStart.SetActive(false);
        }
    }

    public void OnStartButtonClick()
    {
        SteamNetworkManager room = NetworkManager.singleton as SteamNetworkManager;
        room.showStartButton = false;
        room.ServerChangeScene(room.GameplayScene);
    }

    public void OnReadyButtonClick()
    {
        if (readyToBegin) CmdChangeReadyState(false);
        else CmdChangeReadyState(true);

        UpdateLobbyList();
    }
}
