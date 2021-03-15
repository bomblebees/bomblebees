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
    [SerializeField] private GameObject damageIndicator;

    [Header("Settings")]
    [SerializeField] private int deathUItime = 3;

    public override void OnStartClient()
    {
        base.OnStartClient();

        //ulong steamId = this.GetComponent<Player>().steamId;

        //Debug.Log("server transfer steamid " + steamId);

        CmdUpdatePlayerName(this.gameObject);

        this.gameObject.GetComponent<Health>().EventLivesChanged += OnPlayerTakeDamage;
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

    public void OnPlayerTakeDamage(int _, int __)
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

    public void UpdateStackHud(int idx, char key)
    {
        if (idx == 2 || stackUI[idx+1].color == Color.clear)
            stackUI[idx].gameObject.transform.localScale = new Vector3(0.25f,0.25f,0.25f);
        else
            stackUI[idx].gameObject.transform.localScale = new Vector3(0.1904f,0.1904f,0.1904f);

        if (stackUI[idx].color != GetKeyColor(key))
        {
            if (GetKeyColor(key) == Color.white)
                stackUI[idx].color = new Color(0f,0f,0f,0f);
            else 
                stackUI[idx].color = GetKeyColor(key);

            // Run bounce anim
            // stackUI[idx].gameObject.GetComponent<IconBounceTween>().OnTweenStart();
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