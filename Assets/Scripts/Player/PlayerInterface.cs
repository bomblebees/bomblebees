using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using Steamworks;

public class PlayerInterface : NetworkBehaviour
{
    [SerializeField] private GameObject playerObject;
    //public GameObject playerModelsAndVfx;

    [Header("Player HUD")]
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Image[] stackUI = new Image[3];
    [SerializeField] private RawImage hexUI;
    [SerializeField] private Image bombCooldownFilter;
    [SerializeField] private Image spinChargeBar;
    private float bombHudTimer = 0;

    [Header("User Interface")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject damageIndicator;

    [Header("Settings")]
    [SerializeField] private int deathUItime = 3;

    private Player player;
    private GameUIManager gameUIManager;
    private BombHelper bombHelper;

    public override void OnStartClient()
    {
        base.OnStartClient();

        //ulong steamId = this.GetComponent<Player>().steamId;

        //Debug.Log("server transfer steamid " + steamId);

        CmdUpdatePlayerName(this.gameObject);

        this.gameObject.GetComponent<Health>().EventLivesChanged += OnPlayerTakeDamage;


        gameUIManager = GameUIManager.Singleton;
        if (gameUIManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

        bombHelper = gameUIManager.GetComponent<BombHelper>();


        player = this.GetComponent<Player>();
        player.itemStack.Callback += OnUIStackChange;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        UpdateSpinChargeBar();
        UpdateBombFilterCooldown();
    }

    public void UpdateSpinChargeBar()
    {
        spinChargeBar.fillAmount = player.spinChargeTime / 1.5f;
    }

    public void UpdateBombFilterCooldown()
    {
        if (bombHudTimer > 0)
        {
            float totalDuration = this.GetComponent<Player>().defaultBombCooldown;
            bombCooldownFilter.fillAmount = bombHudTimer / totalDuration;
            bombHudTimer -= Time.deltaTime;

            if (bombHudTimer < 0)
            {
                bombHudTimer = 0;
                bombCooldownFilter.fillAmount = 0;
            }
        }
    }

    public void OnPlayerTakeDamage(int _, int __, GameObject ___)
    {
        if (isLocalPlayer) damageIndicator.GetComponent<FlashTween>().StartFlash();
    }

    public IEnumerator EnableDeathUI()
    {
        // WARN: lazy way to disable player after death, may still be able to place bombs
        //playerModelsAndVfx.SetActive(false); 

        deathUI.SetActive(true);
        yield return new WaitForSeconds(deathUItime);
        deathUI.SetActive(false);
    }

    public void EnableGameOverUI()
    {
        deathUI.SetActive(false);
        gameOverUI.SetActive(true);
    }


    #region Heads Up Display (HUD)

    [Command(ignoreAuthority=true)]
    public void CmdUpdatePlayerName(GameObject player)
    {
        RpcUpdatePlayerName(player);
    }

    [ClientRpc]
    public void RpcUpdatePlayerName(GameObject player)
    {
        playerName.text = player.GetComponent<Player>().steamName;
        playerName.color = player.GetComponent<Player>().playerColor;
    }

    [Client]
    void OnUIStackChange(SyncList<char>.Operation op, int idx, char oldColor, char newColor)
    {
        PlayerInterface hud = this.GetComponent<PlayerInterface>();
        // for (int i = 0; i < 3; i++)
        for (int i = 2; i >= 0; i--)
        {
            if (i < player.itemStack.Count)
            {
                hud.UpdateStackHud(i, player.itemStack[i]);
            }

            else hud.UpdateStackHud(i, 'e');
        }
    }

    public void UpdateStackHud(int idx, char key)
    {
        // Enlarge if front of stack (ie. next bomb to drop)
        if (idx == player.itemStack.Count - 1)
            stackUI[idx].gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        else
            stackUI[idx].gameObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        // Set the sprite if it is a bomb, otherwise disable it
        if (key == 'e')
        {
            stackUI[idx].color = Color.clear;
        } else
        {
            stackUI[idx].color = Color.white;
            stackUI[idx].sprite = bombHelper.GetKeySprite(key);
        }


    }

    public void UpdateHexHud(char key)
    {
        hexUI.color = GetKeyColor(key);

        // Run bounce anim
        hexUI.gameObject.GetComponent<IconBounceTween>().OnTweenStart();
    }

    // get color associated with key
    Color GetKeyColor(char key)
    {
        switch (key)
        {
            case 'b': return new Color32(0, 217, 255, 255);
            case 'g': return new Color32(23, 229, 117, 255);
            case 'y': return new Color32(249, 255, 35, 255);
            case 'r': return Color.red;
            case 'p': return new Color32(241, 83, 255, 255);
            case 'w': return new Color32(178, 178, 178, 255);
            case 'e': return Color.white;
            default: return Color.white;
        }
    }

    [Server]
    public void StartBombHudCooldown(float duration)
    {
        RpcStartBombHudCooldown(duration);
    }

    [ClientRpc]
    public void RpcStartBombHudCooldown(float duration)
    {
        bombHudTimer = duration;
    }

    #endregion




}