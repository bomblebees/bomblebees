using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using Steamworks;
using System.Collections.Generic;

public class PlayerInterface : NetworkBehaviour
{
    [SerializeField] private GameObject playerObject;
    //public GameObject playerModelsAndVfx;

    [Header("Player HUD")]
    [SerializeField] private TMP_Text playerName;

    [SerializeField] private Image hexUI;
    [SerializeField] public Image spinChargeBar;
    [SerializeField] public GameObject spinUI;
    [SerializeField] public GameObject inventoryUI;
	[SerializeField] public GameObject inventoryUIRadial;
	[SerializeField] public GameObject sludgedSpinBarUI;

    [Header("User Interface")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject damageIndicator;
    [SerializeField] private GameObject damageFlash;
    [SerializeField] private TMP_Text selectedBombText;

    [Header("Settings")]
    [SerializeField] private int deathUItime = 3;
    [SerializeField] private float spinExitDelay = 0.5f;

    [Header("Inventory")]
    [SerializeField] private Image selectedHighlight;
    [SerializeField] private GameObject[] invSlotsContainers = new GameObject[4];
	[SerializeField] private Image[] invSlotsRadial = new Image[4];

	// Each game object in array holds either 3, 4, or 5 slot UI frames which change on inv size change
	[SerializeField] private GameObject[] slottedFrames = new GameObject[4];

	[SerializeField] private TMP_Text[] invCounters = new TMP_Text[4];
    [SerializeField] private TMP_Text[] invAddTexts = new TMP_Text[4];

    private Player player;
    private GameUIManager gameUIManager;

    private PlayerInterface[] playerList = null;

    public override void OnStartServer()
    {
        base.OnStartServer();

        this.gameObject.GetComponent<Health>().EventLivesChanged += RpcOnPlayerTakeDamage;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        UpdatePlayerName();

        UpdateInventoryQuantity();

        if (!isLocalPlayer)
        {
            hexUI.gameObject.SetActive(false);
            // inventoryUI.gameObject.SetActive(false);
			inventoryUIRadial.gameObject.SetActive(false);
            playerName.transform.localPosition = new Vector3(0f, -11.6f, 0f);

        }

        gameUIManager = GameUIManager.Singleton;
        if (gameUIManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

        player = this.GetComponent<Player>();
    }

    bool spinChargeStarted = false;
    float spinChargeTime = 0;
    

    private void Update()
    {
        if (!isLocalPlayer) return;

        // If player null, return
        if (!player) return;

        UpdateSpinCharge();

        // Show other player infos when the key is pressed
        if (KeyBindingManager.GetKey(KeyAction.ShowInfo))
        {
            ShowPlayerInfo();
        } else
        {
            UnshowPlayerInfo();
        }
    }



    /// <summary>
    /// Called after spin charge is released, sets it back to zero after some time
    /// </summary>
    [Client] private IEnumerator DelaySpinChargeExit()
    {
        yield return new WaitForSeconds(spinExitDelay);
        spinChargeTime = 0;
        spinUI.GetComponent<CanvasGroup>().alpha = 0.5f;
        UpdateSpinChargeBar();
    }

    [Client] private void UpdateSpinCharge()
    {
        PlayerSpin ps = player.GetComponent<PlayerSpin>();

        // Show full opacity charge bar if sludged
        if (player.GetComponent<Player>().isSludged == true) spinUI.GetComponent<CanvasGroup>().alpha = 1;
        else spinUI.GetComponent<CanvasGroup>().alpha = .5f;

        if (ps.spinHeld)
        {
            spinChargeStarted = true;
            spinUI.GetComponent<CanvasGroup>().alpha = 1;
        }
        else if (spinChargeStarted)
        {
            // Reset the spin charge to the level that it acheived
            int level = ps.CalculateSpinLevel(spinChargeTime);
            if (level == 0) spinChargeTime = 0;
            else spinChargeTime = ps.spinTimings[level - 1];

            // Spin charge has finished 
            spinChargeStarted = false;

            // Reupdate the UI
            UpdateSpinChargeBar();

            // Set to back to zero after a short time
            StartCoroutine(DelaySpinChargeExit());
        }

        if (spinChargeStarted)
        {
            UpdateSpinChargeBar();
        }
    }

    public void UpdateSpinChargeBar()
    {
        float[] spinTimes = player.GetComponent<PlayerSpin>().spinTimings;
        spinChargeTime += Time.deltaTime;
        spinChargeBar.fillAmount = spinChargeTime / spinTimes[spinTimes.Length - 3];
    }

    [ClientRpc] public void RpcOnPlayerTakeDamage(int currentHealth, int _, GameObject __)
    {
        if (!isLocalPlayer) return;

        damageIndicator.GetComponent<ColorTween>().StartTween();

        if (currentHealth == 1)
        {
            damageFlash.GetComponent<ColorTween>().LoopTween();
        } else if (currentHealth == 0)
        {
            damageFlash.GetComponent<ColorTween>().EndLoopTween();
        }
	}


    #region Heads Up Display (HUD)

    [Client] public void UpdatePlayerName()
    {

        LobbySettings settings = FindObjectOfType<LobbySettings>();

        // Set color of name if teams is on
        if (settings.GetGamemode() is TeamsGamemode)
        {
            if (settings.localTeamIndex == this.GetComponent<Player>().teamIndex) playerName.color = Color.green;
            else playerName.color = Color.red;
        } else
        {
            playerName.color = this.GetComponent<Player>().playerColor;
        }

        playerName.text = this.GetComponent<Player>().steamName;
    }

    public void UpdateHexHud(char key)
    {
        hexUI.color = BombHelper.GetKeyColor(key);

        // Run bounce anim
        hexUI.gameObject.GetComponent<ScaleTween>().StartTween();
    }

	[ClientRpc]
    public void DisplayInventoryAdd(int slot, int amt)
    {
		
        invAddTexts[slot].text = "+" + amt.ToString();
        //invAddTexts[slot].GetComponent<ScaleTween>().StartTween();
        invAddTexts[slot].GetComponent<AlphaTextTween>().StartTween();
        invAddTexts[slot].GetComponent<MoveTween>().StartTween();

		ScaleTween scaleTween = invSlotsContainers[slot].GetComponent<ScaleTween>();

		// 1 = 1.1, 2 = 1.4, 3+ = 1.9
		scaleTween.scaleMultipler = 1.3f + ((amt * amt) / 10f);

		scaleTween.StartTween();
    }

    public void UpdateInventoryQuantity()
    {
        SyncList<int> list = this.GetComponent<PlayerInventory>().inventoryList;

		// non-radial
		/*
        for (int i = 0; i < list.Count; i++)
        {
            invCounters[i].text = list[i].ToString();
            // update color if stack is full
            if (invCounters[i].text == "5") invCounters[i].color = Color.yellow;
            else                            invCounters[i].color = Color.white;

            if (list[i] <= 0) invSlots[i].color = new Color(0.5f, 0.5f, 0.5f);
            else invSlots[i].color = new Color(1f, 1f, 1f);
        }
		*/

		// radial
		for (int i = 0; i < list.Count; i++)
		{
			invSlotsRadial[i].fillAmount = (float)list[i] / (float)GetComponent<PlayerInventory>().GetMaxInvSizes()[i];
		}
    }

	public void UpdateInventorySize()
	{
		SyncList<int> playerInventorySizes = this.GetComponent<PlayerInventory>().inventorySize;
		SyncList<int> list = this.GetComponent<PlayerInventory>().inventoryList;

		Debug.Log("updating inventory size UI on " + gameObject.name + ", current inventory size in index 0: " + playerInventorySizes[0]);

		// for each radial frame container, deactivate each frame inside, and reactivate the correct one
		for (int i = 0; i < slottedFrames.Length; i++)
		{
			InventoryRadialSlottedFrame frameImage = slottedFrames[i].GetComponent<InventoryRadialSlottedFrame>();

			frameImage.SwapFrame(playerInventorySizes[i]);
		}

		for (int i = 0; i < list.Count; i++)
		{
			invSlotsRadial[i].fillAmount = (float)list[i] / (float)GetComponent<PlayerInventory>().GetMaxInvSizes()[i];
		}
	}


	public void UpdateInventorySelected()
    {
        int selected = this.GetComponent<PlayerInventory>().selectedSlot;
        selectedHighlight.gameObject.transform.localPosition = invSlotsRadial[selected].transform.parent.transform.localPosition;
            
        if (isLocalPlayer)
        {
            char key = PlayerInventory.INVEN_BOMB_TYPES[selected];
            selectedBombText.text = BombHelper.GetBombTextByKey(key) + " Bomb";
            selectedBombText.GetComponent<ColorTween>().StartTween();
        }
    }

    [Client] private void ShowPlayerInfo()
    {
        if (playerList == null) playerList = FindObjectsOfType<PlayerInterface>();

        foreach (PlayerInterface p in playerList)
        {
            if (p.GetComponent<Player>().isEliminated) continue;

            if (p == this) continue;

            if (!p.inventoryUIRadial.gameObject.activeSelf)
            {
                //p.hexUI.gameObject.SetActive(true);
                // p.inventoryUI.gameObject.SetActive(true);
				p.inventoryUIRadial.gameObject.SetActive(true);
                p.playerName.transform.localPosition = new Vector3(0f, 12.2f, 0f);
            }
        }
    }

    [Client] private void UnshowPlayerInfo()
    {
        if (playerList == null) return;

        foreach (PlayerInterface p in playerList)
        {
            if (p == this) continue;

			// if (p.inventoryUI.gameObject.activeSelf)
			if (p.inventoryUIRadial.gameObject.activeSelf)
            {
                p.hexUI.gameObject.SetActive(false);
                // p.inventoryUI.gameObject.SetActive(false);
				p.inventoryUIRadial.gameObject.SetActive(false);
				p.playerName.transform.localPosition = new Vector3(0f, -11.6f, 0f);
            }
        }
    }

    #endregion
}