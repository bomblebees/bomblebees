using System;
using UnityEngine;
using Mirror;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    [SyncVar] public ulong steamId = 0;
    [SyncVar] public string steamUsername = "Username";
    [SyncVar] public int steamAvatarId;
    [SyncVar] public Color playerColor;
    [SyncVar] public string ping;
    
    [Header("Character Selection")]
    [SyncVar] public int characterCode;
    private CharacterSelectionInfo _characterSelectionInfo;

    [Header("Default UI")]
    [SerializeField] private Texture2D defaultAvatar;
    [SerializeField] private string defaultUsername;

    private PingDisplay _pingDisplay;
    
    // Temp list of player colors
    private List<Color> listColors = new List<Color> {
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green,
    };
    
    Room_UI roomUI;

    NetworkRoomManagerExt room;

    public override void OnStartClient()
    {
        InitRequiredVars();
        base.OnStartClient();

        _pingDisplay = FindObjectOfType<PingDisplay>();
        InvokeRepeating(nameof(SetPing), 0f, _pingDisplay.updateInterval);
    }

    private void SetPing()
    {
        Debug.Log("SetPing() is called");
        CmdSetPing(_pingDisplay.myPing);
        UpdatePingDisplay();
    }

    [Command]
    private void CmdSetPing(string newPing)
    {
        ping = newPing;
    }
    
    public void UpdatePingDisplay()
    {
        for (int i = 0; i < room.roomSlots.Count; i++)
        {
            Room_UI.PlayerLobbyCard card = roomUI.playerLobbyUi[i];
            NetworkRoomPlayerExt player = room.roomSlots[i] as NetworkRoomPlayerExt;

            // TODO: Test only
            card.username.text = player.steamUsername + " " + player.ping;
        }
    }

    private void InitRequiredVars()
    {
        // Only init if we are in the room scene
        if (SceneManager.GetActiveScene().name != "Room") return;

        if (!_characterSelectionInfo)
        {
            subscribed = false;
            _characterSelectionInfo = FindObjectOfType<CharacterSelectionInfo>();
            if (!_characterSelectionInfo) Debug.LogError("_characterSelectionInfo not found");
        }

        if (!room)
        {
            subscribed = false;
            room = NetworkManager.singleton as SteamNetworkManager;
            if (!room) room = NetworkManager.singleton as NetworkRoomManagerExt;
            if (!room) Debug.LogError("room not found");
        }

        if (!roomUI)
        {
            subscribed = false;
            roomUI = Room_UI.singleton;
            if (!roomUI) Debug.LogError("room not found");
        }
    }

    private void Update()
    {
        InitRequiredVars();

        // EXIT IF REQUIRED VARS NOT INITIALIZED
        if (!_characterSelectionInfo || !room || !roomUI) return;

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

    // Whether THIS player is subscribed to the events already or not
    private bool subscribed = false;

    public override void OnClientEnterRoom()
    {
        base.OnClientEnterRoom();

        Debug.Log("on client enter: player " + index);

        // Subscribe to events
        InitRequiredVars();

        if (!subscribed)
        {
            _characterSelectionInfo.EventCharacterChanged += OnCharacterChanged;
            roomUI.EventReadyButtonClicked += OnReadyButtonClick;
            roomUI.EventStartButtonClicked += OnStartButtonClick;
            subscribed = true;
        }
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
            card.username.text = player.steamUsername + " " + player.ping; // TODO: Test only

            // If steam is active, set steam avatars
            //card.avatar.texture = GetSteamImageAsTexture(player.steamAvatarId);

            // Character selection
            card.characterPortrait.enabled = true;
            card.characterPortrait.texture = _characterSelectionInfo.characterPortraitList[player.characterCode];

            // Ready status
            card.readyStatus.SetActive(player.readyToBegin);

            // Set colors of the color frames
            foreach (Image elem in card.colorFrames)
            {
                elem.color = listColors[player.characterCode];
            }

            // If not ready, and character portrait is unavailable, grey out the portrait
            if (!player.readyToBegin && !_characterSelectionInfo.characterAvailable[player.characterCode])
            {
                card.characterPortrait.color = new Color(0.4f, 0.4f, 0.4f);
            }
            else
            {
                card.characterPortrait.color = new Color(1f, 1f, 1f);
            }

            // if player is ready, update the character availability
            if (player.readyToBegin && _characterSelectionInfo.characterAvailable[player.characterCode])
            {
                _characterSelectionInfo.characterAvailable[player.characterCode] = false;
            }
        }
    }

    // Returns the next available character in the character array
    [Client] public int GetNextAvailableCharacter()
    {
        int nextCode = -1;

        int len = _characterSelectionInfo.characterAvailable.Length;

        // Check the next available character
        for (int i = characterCode + 1; i < characterCode + len + 1; i++)
        {
            if (_characterSelectionInfo.characterAvailable[i % len])
            {
                nextCode = i % len;
                break;
            }
        }

        // return the character code
        return nextCode;
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
        CmdChangeCharacterCode(GetNextAvailableCharacter());
    }

    [Command] private void CmdChangeCharacterCode(int code)
    {
        characterCode = code;
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
