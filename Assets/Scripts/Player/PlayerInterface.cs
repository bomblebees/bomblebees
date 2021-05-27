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
	private GameActions _gameActions;
	
    [SerializeField] private GameObject playerObject;
    //public GameObject playerModelsAndVfx;

    [Header("Player HUD")]
    [SerializeField] private TMP_Text playerName;

    [SerializeField] private Image hexUI;
    [SerializeField] private HeldHexUIIcon hexUIIcon;
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
    [SerializeField] private InventoryStackItem[] invStackItems = new InventoryStackItem[4];
	[SerializeField] private InventoryStackItem localPlayerSingleRadial; // assign the single radial so local player can use this one

    private Player player;
    private GameUIManager gameUIManager;

    private PlayerInterface[] playerList = null;

    private void Awake()
    {
	    _gameActions = FindObjectOfType<MenuManager>().GameActions;
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        UpdatePlayerName();

        UpdateInventoryQuantity();

        if (!isLocalPlayer)
        {
            hexUI.gameObject.SetActive(false);
            spinUI.gameObject.SetActive(false);
            inventoryUIRadial.gameObject.SetActive(false);
			localPlayerSingleRadial.gameObject.SetActive(false);
            playerName.transform.localPosition = new Vector3(0f, -11.6f, 0f);

        } else
        {
            inventoryUIRadial.transform.localPosition = new Vector3(0f, 36.4f, 0f);
        }

        this.gameObject.GetComponent<Health>().EventLivesChanged += OnPlayerTakeDamage;

        // Spin charge bar invisible until held
        spinUI.GetComponent<CanvasGroup>().alpha = 0f;

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
        if (_gameActions.ShowInfo.IsPressed || this.GetComponent<Player>().isEliminated)
        {
            ShowPlayerInfo();
        } else if (!this.GetComponent<Player>().isEliminated)
        {
            UnshowPlayerInfo();
        }
    }



    /// <summary>
    /// Called after spin charge is released, sets it back to zero after some time
    /// </summary>
    [Client] private IEnumerator DelaySpinChargeExit()
    {
        spinUI.GetComponent<CanvasGroup>().alpha = 0.5f;
        yield return new WaitForSeconds(spinExitDelay);
        spinUI.GetComponent<CanvasGroup>().alpha = 0f;
        spinChargeTime = 0;
        UpdateSpinChargeBar();
    }

    [Client] private void UpdateSpinCharge()
    {
        PlayerSpin ps = player.GetComponent<PlayerSpin>();

        // Show full opacity charge bar if sludged
        if (player.GetComponent<Player>().isSludged == true) spinUI.GetComponent<CanvasGroup>().alpha = 1;
		else spinUI.GetComponent<CanvasGroup>().alpha = 0;
		//else spinUI.GetComponent<CanvasGroup>().alpha = 0f;

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

    [Client] public void OnPlayerTakeDamage(int currentHealth, int _, GameObject __)
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

	[Client] public void ToggleNameToSludged()
	{
		if (GetComponent<Player>().isSludged)
		{
			playerName.text = "Sludged!";
			playerName.color = new Color32(255, 216, 25, 255);
			playerName.fontSize = 29;
		}
		else
		{
			UpdatePlayerName();
			playerName.fontSize = 25;
		}
	}

    public void UpdateHexHud(char key)
    {
        hexUI.color = BombHelper.GetKeyColor(key);

         // Set current hex icon to new color
        if      (key == 'r') { hexUIIcon.SwapType(0); hexUIIcon.SetIconColor(0); }
        else if (key == 'p') { hexUIIcon.SwapType(1); hexUIIcon.SetIconColor(1); }
        else if (key == 'y') { hexUIIcon.SwapType(2); hexUIIcon.SetIconColor(2); }
        else if (key == 'g') { hexUIIcon.SwapType(3); hexUIIcon.SetIconColor(3); }

        

        // Run bounce anim
        // hexUI.gameObject.GetComponent<ScaleTween>().StartTween();


        // Run bounce anim (background tile)
        hexUI.gameObject.GetComponent<ScaleTween>().StartTween();
    }

	[ClientRpc]
    public void DisplayInventoryAdd(int slot, int amt)
    {

        invStackItems[slot].invAddText.text = "+" + amt.ToString();
        //invAddTexts[slot].GetComponent<ScaleTween>().StartTween();
        //invStackItems[slot].invAddText.GetComponent<AlphaTextTween>().StartTween();
        invStackItems[slot].invAddText.GetComponent<MoveTween>().StartTween();

		// Text/tween animation for 4 slot radial UI on combo made, reactivate later for separate UI settings
		// invStackItems[slot].invAddText.GetComponent<AlphaTextTween>().StartTween();
		// invStackItems[slot].invAddText.GetComponent<MoveTween>().StartTween();

		// localPlayerSingleRadial.GetComponent<ScaleTween>().StartTween();
		

		

		// 1 = 1.1, 2 = 1.4, 3+ = 1.9

		if (isLocalPlayer)
		{
			localPlayerSingleRadial.invAddText.text = "+" + amt.ToString();
			switch (slot)
			{
				case 0:
					localPlayerSingleRadial.invAddText.color = BombHelper.GetKeyColor('r');
					break;
				case 1:
					localPlayerSingleRadial.invAddText.color = BombHelper.GetKeyColor('p');
					break;
				case 2:
					localPlayerSingleRadial.invAddText.color = BombHelper.GetKeyColor('y');
					break;
				case 3:
					localPlayerSingleRadial.invAddText.color = BombHelper.GetKeyColor('g');
					break;
			}

			localPlayerSingleRadial.invAddText.GetComponent<AlphaTextTween>().StartTween();

			// Configure scale tween for inventory "dots" HUD system later
			/*
			ScaleTween scaleTween = localPlayerSingleRadial.GetComponent<ScaleTween>();
			scaleTween.scaleMultipler = 1.3f + ((amt * amt) / 10f);
			scaleTween.StartTween();
			*/
		}
    }
	
	/// <summary>
	/// Called when player removes a bomb from their inventory
	/// </summary>
	[ClientRpc]
	public void DisplayInventoryUse()
	{
		if (isLocalPlayer)
		{
			ScaleTween scaleTween = localPlayerSingleRadial.GetComponent<ScaleTween>();
			scaleTween.scaleMultipler = 0.7f;
			scaleTween.StartTween();
		}
	}

    public void UpdateInventoryQuantity()
    {
        SyncList<int> list = this.GetComponent<PlayerInventory>().inventoryList;
		int selected = this.GetComponent<PlayerInventory>().selectedSlot;

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
            invStackItems[i].invSlotRadial.fillAmount = (float)list[i] / (float)GetComponent<PlayerInventory>().GetMaxInvSizes()[i];

            invStackItems[i].invCounter.text = list[i].ToString();
        }
		if (isLocalPlayer)
		{
			localPlayerSingleRadial.invSlotRadial.fillAmount = (float)list[selected] / (float)GetComponent<PlayerInventory>().GetMaxInvSizes()[selected];
			localPlayerSingleRadial.invCounter.text = list[selected].ToString();
            FindObjectOfType<AmmoDisplay>().UpdateInventoryQuantity(this.gameObject);
        }
	}

	public void UpdateInventorySize()
	{
		SyncList<int> playerInventorySizes = this.GetComponent<PlayerInventory>().inventorySize;
		SyncList<int> list = this.GetComponent<PlayerInventory>().inventoryList;
		int selected = this.GetComponent<PlayerInventory>().selectedSlot;

		Debug.Log("updating inventory size UI on " + gameObject.name + ", current inventory size in index 0: " + playerInventorySizes[0]);

		// for each radial frame container, deactivate each frame inside, and reactivate the correct one
		for (int i = 0; i < invStackItems.Length; i++)
		{
			InventoryRadialSlottedFrame frameImage = invStackItems[i].slottedFrame.GetComponent<InventoryRadialSlottedFrame>();

			frameImage.SwapFrame(playerInventorySizes[i]);
		}

		for (int i = 0; i < list.Count; i++)
		{
            invStackItems[i].invSlotRadial.fillAmount = (float)list[i] / (float)GetComponent<PlayerInventory>().GetMaxInvSizes()[i];
        }

		// update local player HUD
		if (isLocalPlayer)
		{
			localPlayerSingleRadial.invSlotRadial.fillAmount = (float)list[selected] / (float)GetComponent<PlayerInventory>().GetMaxInvSizes()[selected];
            FindObjectOfType<AmmoDisplay>().UpdateInventorySize(this.gameObject);

			// copy pasted code to refresh local player HUD radial
			InventoryRadialSlottedFrame frame = localPlayerSingleRadial.GetComponentInChildren<InventoryRadialSlottedFrame>();
			RadialFrameBombTypeIndicator frameType = localPlayerSingleRadial.GetComponentInChildren<RadialFrameBombTypeIndicator>();

			frame.SetSlotColor(selected);
			frame.SwapFrame(playerInventorySizes[selected]);

			if (selected < 2)
			{
				// slot index is 0 or 1, pass in 0 for bomb or honey bomb
				frameType.SwapType(0);
			}
			else if (selected > 1 && selected < 4)
			{
				// slot index is 2 or 3, pass in 1 for laser or plasma
				frameType.SwapType(1);
			}

			frameType.SetFrameColor(selected);
			frame.SwapFrame(playerInventorySizes[selected]);
		}
	}


	public void UpdateInventorySelected()
    {
        int selected = this.GetComponent<PlayerInventory>().selectedSlot;
		SyncList<int> playerInventorySizes = this.GetComponent<PlayerInventory>().inventorySize;

		selectedHighlight.gameObject.transform.localPosition = invStackItems[selected].invSlotRadial.transform.parent.transform.localPosition;
            
        if (isLocalPlayer)
        {
            char key = PlayerInventory.INVEN_BOMB_TYPES[selected];
            selectedBombText.text = BombHelper.GetBombTextByKey(key) + " Bomb";
            selectedBombText.GetComponent<ColorTween>().StartTween();

			// change color of HUD radial UI frame
			InventoryRadialSlottedFrame frame = localPlayerSingleRadial.GetComponentInChildren<InventoryRadialSlottedFrame>();
			RadialFrameBombTypeIndicator frameType = localPlayerSingleRadial.GetComponentInChildren<RadialFrameBombTypeIndicator>();

			frame.SetSlotColor(selected);
			frame.SwapFrame(playerInventorySizes[selected]);

			// Choosing which bomb type frame to display based on hard-coded inv slot values is fragile, refactor to-do
			if (selected < 2)
			{
				// slot index is 0 or 1, pass in 0 for bomb or honey bomb
				frameType.SwapType(0);
			}
			else if (selected > 1 && selected < 4)
			{
				// slot index is 2 or 3, pass in 1 for laser or plasma
				frameType.SwapType(1);
			}

			frameType.SetFrameColor(selected);
			frame.SwapFrame(playerInventorySizes[selected]);

			// change color of radial fill charges
			localPlayerSingleRadial.invSlotRadial.GetComponent<Image>().sprite = localPlayerSingleRadial.radialFillColors[selected];

			// and then refresh it
			localPlayerSingleRadial.invSlotRadial.fillAmount = (float)this.GetComponent<PlayerInventory>().inventoryList[selected] / (float)GetComponent<PlayerInventory>().GetMaxInvSizes()[selected];

			// also refresh the quantity number text
			localPlayerSingleRadial.invCounter.text = GetComponent<PlayerInventory>().inventoryList[selected].ToString();
			FindObjectOfType<AmmoDisplay>().UpdateInventorySelected(this.gameObject);

			// Scale bounce tween upon switching selected slot
			ScaleTween scaleTween = localPlayerSingleRadial.GetComponent<ScaleTween>();
			scaleTween.scaleMultipler = 1.3f;
			scaleTween.StartTween();
		}
    }

    // cache show player info
    private bool playerInfoEnabled = false;

    [Client] private void ShowPlayerInfo()
    {
        // if already enabled, return
        if (playerInfoEnabled) return;
        playerInfoEnabled = true;

        if (playerList == null) playerList = FindObjectsOfType<PlayerInterface>();

        foreach (PlayerInterface p in playerList)
        {
            if (p.GetComponent<Player>().isEliminated) continue;

            if (!p.inventoryUIRadial.gameObject.activeSelf)
            {
                p.inventoryUIRadial.gameObject.SetActive(true);

                if (p == this) p.playerName.transform.localPosition = new Vector3(0f, 47.6f, 0f);
                else p.playerName.transform.localPosition = new Vector3(0f, 12.2f, 0f);

            }
        }
    }

    [Client] private void UnshowPlayerInfo()
    {
        // if already disabled, return
        if (!playerInfoEnabled) return;
        playerInfoEnabled = false;

        if (playerList == null) return;

        foreach (PlayerInterface p in playerList)
        {
			if (p.inventoryUIRadial.gameObject.activeSelf)
            {
				p.inventoryUIRadial.gameObject.SetActive(false);

                if (p == this) p.playerName.transform.localPosition = new Vector3(0f, 18.5f, 0f);
                else p.playerName.transform.localPosition = new Vector3(0f, -11.6f, 0f);
            }
        }
    }

    #endregion
}