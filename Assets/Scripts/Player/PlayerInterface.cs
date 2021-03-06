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
    private float bombHudTimer = 0;

    [Header("User Interface")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject gameOverUI;

    [Header("Settings")]
    [SerializeField] private int deathUItime = 3;

    public override void OnStartClient()
    {
        base.OnStartClient();

        ulong steamId = this.GetComponent<Player>().steamId;

        Debug.Log("server transfer steamid " + steamId);

        if (steamId != 0)
        {
            CmdUpdatePlayerName(steamId);
        }
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (bombHudTimer > 0)
        {
            float totalDuration = this.GetComponent<Player>().defaultBombCooldown;
            bombCooldownFilter.fillAmount = bombHudTimer / totalDuration;
            bombHudTimer -= Time.deltaTime;

            if (bombHudTimer < 0) bombHudTimer = 0;
        }
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
    public void CmdUpdatePlayerName(ulong steamId)
    {
        RpcUpdatePlayerName(steamId);
    }

    [ClientRpc]
    public void RpcUpdatePlayerName(ulong steamId)
    {
        CSteamID id = new CSteamID(steamId);

        playerName.text = SteamFriends.GetFriendPersonaName(id);
    }

    public void UpdateStackHud(int idx, char key)
    {
        if (stackUI[idx].color != GetKeyColor(key))
        {
            stackUI[idx].color = GetKeyColor(key);

            // Run bounce anim
            stackUI[idx].gameObject.GetComponent<IconBounceTween>().OnTweenStart();
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
            case 'b': return Color.blue;
            case 'g': return Color.green;
            case 'y': return Color.yellow;
            case 'r': return Color.red;
            case 'p': return Color.magenta;
            case 'w': return Color.grey;
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