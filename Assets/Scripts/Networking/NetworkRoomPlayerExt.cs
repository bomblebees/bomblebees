using System;
using UnityEngine;
using Mirror;
using Steamworks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    [SyncVar] public ulong steamId;
    [SyncVar] public string steamUsername;
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

    public override void OnStartClient()
    {
        this.playerColor = listColors[index];

        roomUI = Room_UI.singleton;
        roomUI.EventReadyButtonClicked += OnReadyButtonClick;
        roomUI.EventStartButtonClicked += OnStartButtonClick;

        _characterSelectionInfo = FindObjectOfType<CharacterSelectionInfo>();
        _characterSelectionInfo.EventCharacterChanged += OnCharacterChanged;

        base.OnStartClient();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
    {
        _characterSelectionInfo = FindObjectOfType<CharacterSelectionInfo>();
        _characterSelectionInfo.EventCharacterChanged += OnCharacterChanged;
    }

    public override void OnClientEnterRoom()
    {
        UpdateLobbyList();
    }

    public override void OnClientExitRoom()
    {
        SteamNetworkManager room = NetworkManager.singleton as SteamNetworkManager;
        if (!room) return;
        Debug.Log("exit room count" + room.roomSlots.Count);

        this.playerColor = listColors[index];
        UpdateLobbyList();
    }

    public override void ReadyStateChanged(bool _, bool newReadyState)
    {
        UpdateLobbyList();
    }

    bool[] characterAvailable = { true, true, true, true };
    bool selfReady = false;

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
        if (!room) return;
        
        // update all existing players
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

            CSteamID steamid = new CSteamID(player.steamId);

            // Player list background
            card.playerCard.SetActive(true);

            // User name
            card.username.text = player.steamUsername;

            // User avatar
            int imgId = SteamFriends.GetLargeFriendAvatar(steamid);
            if (imgId > 0) card.avatar.texture = GetSteamImageAsTexture(imgId);
            else { Debug.LogWarning("ImgId invalid!");  }
            
            // Character selection
            card.characterPortrait.texture = _characterSelectionInfo.characterPortraitList[player.characterCode];

            // Disable clicking another player's character portrait && lock character on ready
            if (player == this && !player.readyToBegin)
            {
                card.changeCharacterButton.enabled = true;
                card.changeCharacterButtonHoverTween.enabled = true;
            }
            else
            {
                card.changeCharacterButton.enabled = false;
                card.changeCharacterButtonHoverTween.enabled = false;
            }

            // Ready check mark
            card.readyStatus.SetActive(player.readyToBegin);
            
            //// Check if you are ready
            //if (player == this)
            //{
            //    selfReady = player.readyToBegin;
            //}
            
            // Cache character Availability
            if (player.readyToBegin)
            {
                characterAvailable[player.characterCode] = false;
            } else
            {
                characterAvailable[player.characterCode] = true;
            }
        }

        string t = "";
        for (int i = 0; i < 4; i++)
        {
            t += characterAvailable[i] + ", ";
        }
        Debug.Log(t);

        // Ready button
        if (!characterAvailable[characterCode] && !this.readyToBegin)
        {
            roomUI.DeactivateReadyButton();
        } else
        {
            roomUI.ActivateReadyButton();
        }
        
        // Start button
        if (room.allPlayersReady && room.showStartButton)
        {
            roomUI.ActivateStartButton();
        } else
        {
            roomUI.DeactivateStartButton();
        }
        
        // Prevent buttons from infinitely growing
        roomUI.buttonReady.transform.localScale = new Vector3(1f, 1f, 1f);
        roomUI.buttonStart.transform.localScale = new Vector3(1f, 1f, 1f);
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

    public void OnStartButtonClick()
    {
        SteamNetworkManager room = NetworkManager.singleton as SteamNetworkManager;
        room.showStartButton = false;
        room.ServerChangeScene(room.GameplayScene);
    }
    
    public void OnReadyButtonClick()
    {
        if (!hasAuthority) return;

        CmdChangeReadyState(!readyToBegin);
    }

    [Command]
    private void CmdChangeCharacterCode()
    {
        characterCode = (characterCode + 1) % 4;
    }

    public void OnChangeCharacterCode(int oldCode, int newCode)
    {
        UpdateLobbyList();
    }

    public void OnCharacterChanged()
    {
        if (!hasAuthority) return;

        CmdChangeCharacterCode();

        UpdateLobbyList();
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
