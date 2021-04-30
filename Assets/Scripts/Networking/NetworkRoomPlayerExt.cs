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
    [SyncVar(hook = nameof(OnChangeCharacterCode))] public int characterCode;
    private CharacterSelectionInfo _characterSelectionInfo;

    [Header("Default UI")]
    [SerializeField] private Texture2D defaultAvatar;
    [SerializeField] private string defaultUsername;
    
    // List of player colors
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
        if (!isLocalPlayer) return;

        if (!room)
        {
            room = NetworkManager.singleton as SteamNetworkManager;
            if (!room) room = NetworkManager.singleton as NetworkRoomManagerExt;
            return;
        }
        if (!_characterSelectionInfo) { _characterSelectionInfo = FindObjectOfType<CharacterSelectionInfo>(); return; }

        if (!roomUI) return;


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
    }

    //void OnEnable()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    //public override void OnDisable()
    //{
    //    base.OnDisable();

    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    //private void OnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
    //{
    //    _characterSelectionInfo = FindObjectOfType<CharacterSelectionInfo>();
    //    _characterSelectionInfo.EventCharacterChanged += OnCharacterChanged;
    //}

    public override void OnClientEnterRoom()
    {
        //Debug.Log("player " + index + " joined");
        UpdateLobbyListPlayer();
    }

    public override void OnClientExitRoom()
    {
        SteamNetworkManager room = NetworkManager.singleton as SteamNetworkManager;
        if (!room) return;
        Debug.Log("exit room count" + room.roomSlots.Count);

        this.playerColor = listColors[index];
        Debug.Log("player " + index + " left");
    }

    // Updates the lobby information for the specific player
    public void UpdateLobbyListPlayer()
    {
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

        // If steam is active, set steam avatars
        if (steamId != 0)
        {
            CSteamID steamid = new CSteamID(this.steamId);

            // User avatar
            int imgId = SteamFriends.GetLargeFriendAvatar(steamid);
            if (imgId > 0) card.avatar.texture = GetSteamImageAsTexture(imgId);
            else { Debug.LogWarning("ImgId invalid!"); }
        }

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
        //UpdateLobbyListPlayer();
    }

    // Syncvar Callback for ready status
    [ClientCallback] public override void ReadyStateChanged(bool _, bool newReadyState)
    {
        //UpdateLobbyList();
        //Debug.Log("player " + index + " changed ready state to " + newReadyState);

        if (newReadyState == true)
        {
            _characterSelectionInfo.characterAvailable[characterCode] = false;
        } else
        {
            _characterSelectionInfo.characterAvailable[characterCode] = true;
        }

        UpdateLobbyListPlayer();
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

    // Syncvar Callback for character code
    [ClientCallback]
    public void OnChangeCharacterCode(int _, int __)
    {
        //UpdateLobbyList();
        UpdateLobbyListPlayer();
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
