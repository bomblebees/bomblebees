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

    [SerializeField] private Image hexUI;
    [SerializeField] public Image spinChargeBar;
    [SerializeField] public GameObject spinUI;

    [Header("User Interface")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject damageIndicator;

    [Header("Settings")]
    [SerializeField] private int deathUItime = 3;

    [Header("Inventory")]
    [SerializeField] private Image selectedHighlight;
    [SerializeField] private Image[] invSlots = new Image[4];
    [SerializeField] private TMP_Text[] invCounters = new TMP_Text[4];

    private Player player;
    private GameUIManager gameUIManager;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Turn off held hex and spin charge UI for other players
        if (!isLocalPlayer)
        {
            hexUI.gameObject.SetActive(false);
            //spinUI.SetActive(false);
        }

        CmdUpdatePlayerName(this.gameObject);

        this.gameObject.GetComponent<Health>().EventLivesChanged += OnPlayerTakeDamage;

        UpdateInventoryQuantity();

        gameUIManager = GameUIManager.Singleton;
        if (gameUIManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

        player = this.GetComponent<Player>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        // If player null, return
        if (!player) return;

        UpdateSpinChargeBar();
    }

    public void UpdateSpinChargeBar()
    {
        float[] spinTimes = player.spinTimings;
        spinChargeBar.fillAmount = player.spinChargeTime / spinTimes[spinTimes.Length - 2];
    }

    public void OnPlayerTakeDamage(int _, int __, GameObject ___)
    {
        if (isLocalPlayer) damageIndicator.GetComponent<FlashTween>().StartFlash();
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

    public void UpdateInventoryQuantity()
    {
        SyncList<int> list = this.GetComponent<PlayerInventory>().inventoryList;

        for (int i = 0; i < list.Count; i++)
        {
            invCounters[i].text = list[i].ToString();

            if (list[i] <= 0) invSlots[i].color = new Color(0.5f, 0.5f, 0.5f);
            else invSlots[i].color = new Color(1f, 1f, 1f);
        }
    }

    public void UpdateInventorySelected()
    {
        int selected = this.GetComponent<PlayerInventory>().selectedSlot;

        selectedHighlight.gameObject.transform.localPosition = invSlots[selected].transform.localPosition;
    }

    #endregion
}