using System;
using UnityEngine;
using Mirror;
using Steamworks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    [SyncVar] public ulong steamId = 0;
    [SyncVar] public string steamUsername = "Username";
    [SyncVar] public int steamAvatarId;
    [SyncVar] public Color playerColor;
    
    [Header("Character Selection")]
    [SyncVar] public int characterCode;
    private CharacterSelectionInfo _characterSelectionInfo;

    [Header("Default UI")]
    [SerializeField] private Texture2D defaultAvatar;
    [SerializeField] private string defaultUsername;
    
    // Temp list of player colors
    private List<Color> listColors = new List<Color> {
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green,
    };
    
    Room_UI roomUI;

    NetworkRoomManagerExt room;

    private void Update()
    {

        if (!_characterSelectionInfo)
        {
            _characterSelectionInfo = FindObjectOfType<CharacterSelectionInfo>();
            if (!_characterSelectionInfo) return;
            if (isLocalPlayer)
            {
                _characterSelectionInfo.EventCharacterChanged += OnCharacterChanged;
            }
        }

        if (!room)
        {
            room = NetworkManager.singleton as SteamNetworkManager;
            if (!room) room = NetworkManager.singleton as NetworkRoomManagerExt;
        }

        if (!roomUI)
        {
            roomUI = Room_UI.singleton;
            if (!roomUI) return;
            if (isLocalPlayer)
            {
                roomUI.EventReadyButtonClicked += OnReadyButtonClick;
                roomUI.EventStartButtonClicked += OnStartButtonClick;
            }
        }

        // EXIT IF IS LOCAL PLAYER
        if (!isLocalPlayer) return;

        if (room.allPlayersReady && room.showStartButton)
        {
            roomUI.ActivateStartButton();
        }
        else
        {
            roomUI.DeactivateStartButton();
        }

        // If not ready, and character portrait is unavailable, deactive ready button
        if (!this.readyToBegin && !_characterSelectionInfo.characterAvailable[characterCode])
        {
            roomUI.DeactivateReadyButton();
        } else
        {
            roomUI.ActivateReadyButton();
        }

        UpdateLobbyList();
    }

    public override void OnClientEnterRoom()
    {
        //this.playerColor = listColors[index];
        base.OnClientEnterRoom();
    }

    public override void OnClientExitRoom()
    {
        SteamNetworkManager room = NetworkManager.singleton as SteamNetworkManager;
        if (!room) return;
        Debug.Log("exit room count" + room.roomSlots.Count);

        //this.playerColor = listColors[index];
        Debug.Log("player " + index + " left");
    }

    public void UpdateLobbyList()
    {
        // Update all existing players
        for (int i = 0; i < 4; i++)
        {
            Room_UI.PlayerLobbyCard card = roomUI.playerLobbyUi[i];

            // if player does not exist
            if (i >= room.roomSlots.Count)
            {
                card.username.text = defaultUsername;
                card.avatar.texture = FlipTexture(defaultAvatar);
                card.readyStatus.SetActive(false);
                card.characterPortrait.enabled = false;
                continue;
            }

            card.characterPortrait.enabled = true;

            NetworkRoomPlayerExt player = room.roomSlots[i] as NetworkRoomPlayerExt;

            // If it is the local player, allow them to change the portrait
            if (player == this)
            {
                if (this.readyToBegin)
                {
                    card.changeCharacterButton.enabled = false;
                    card.changeCharacterButtonHoverTween.enabled = false;
                }
                else
                {
                    card.changeCharacterButton.enabled = true;
                    card.changeCharacterButtonHoverTween.enabled = true;
                }
            }

            // Player list background
            card.playerCard.SetActive(true);

            // User name
            card.username.text = player.steamUsername;

            // If steam is active, set steam avatars
            //card.avatar.texture = GetSteamImageAsTexture(player.steamAvatarId);

            // Character selection
            card.characterPortrait.enabled = true;
            card.characterPortrait.texture = _characterSelectionInfo.characterPortraitList[player.characterCode];

            // Ready status
            card.readyStatus.SetActive(player.readyToBegin);

            foreach (Image elem in card.colorFrames)
            {
                elem.color = listColors[player.characterCode];
            }
        }
    }

    // Updates the lobby information for the specific player
    public void UpdateLobbyListPlayer()
    {
        return;

        if (!roomUI)
        {
            // enable room UI
            roomUI = Room_UI.singleton;

            // only subscribe to events if we are the local player
            if (isLocalPlayer)
            {
                roomUI.EventReadyButtonClicked += OnReadyButtonClick;
                roomUI.EventStartButtonClicked += OnStartButtonClick;
            }

            if (!roomUI)
            {
                Debug.LogWarning("room UI not found!");
                return;
            }
        }

        if (!_characterSelectionInfo)
        {
            _characterSelectionInfo = FindObjectOfType<CharacterSelectionInfo>();

            // only subscribe to events if we are the local player
            if (isLocalPlayer)
            {
                _characterSelectionInfo.EventCharacterChanged += OnCharacterChanged;
            }

            if (!_characterSelectionInfo)
            {
                Debug.LogWarning("_characterSelectionInfo not found!");
                return;
            }
        }

        Room_UI.PlayerLobbyCard card = roomUI.playerLobbyUi[this.index];

        // If it is the local player, allow them to change the portrait
        if (isLocalPlayer)
        {
            if (this.readyToBegin)
            {
                card.changeCharacterButton.enabled = false;
                card.changeCharacterButtonHoverTween.enabled = false;
            } else
            {
                card.changeCharacterButton.enabled = true;
                card.changeCharacterButtonHoverTween.enabled = true;
            }
        }

        // Player list background
        card.playerCard.SetActive(true);

        // User name
        card.username.text = this.steamUsername;



        // Character selection
        card.characterPortrait.enabled = true;
        card.characterPortrait.texture = _characterSelectionInfo.characterPortraitList[this.characterCode];

        // Ready status
        card.readyStatus.SetActive(this.readyToBegin);

        //string t = "";
        //for (int i = 0; i < 4; i++)
        //{
        //    t += _characterSelectionInfo.characterAvailable[i] + ", ";
        //}
        //Debug.Log(t);


        //if (this.readyToBegin) roomUI.DeactivateReadyButton();
        //else roomUI.ActivateReadyButton();
    }

    // Called when the start button is clicked
    [Client] public void OnStartButtonClick()
    {
        room.showStartButton = false;
        room.ServerChangeScene(room.GameplayScene);
    }

    // Called by the local player when ready button is clicked
    [Client] public void OnReadyButtonClick()
    {
        CmdChangeReadyState(!readyToBegin);
    }

    // Syncvar Callback for ready status
    [ClientCallback] public override void ReadyStateChanged(bool _, bool newReadyState)
    {
        if (!_characterSelectionInfo) return;

        if (newReadyState == true)
        {
            _characterSelectionInfo.characterAvailable[characterCode] = false;
        } else
        {
            _characterSelectionInfo.characterAvailable[characterCode] = true;
        }
    }


    // Called by the local player when the character card is pressed
    [Client] public void OnCharacterChanged()
    {
        CmdChangeCharacterCode();
    }

    [Command] private void CmdChangeCharacterCode()
    {
        characterCode = (characterCode + 1) % 4;
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

    Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
    {

        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;


        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                if (upSideDown)
                {
                    flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                }
                else
                {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
        }
        flipped.Apply();

        return flipped;
    }
}
