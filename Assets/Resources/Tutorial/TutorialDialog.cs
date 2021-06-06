using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialog : MonoBehaviour
{
    [SerializeField] public RPGTalk dialog;
    [SerializeField] public GameObject canvas;

    [SerializeField] private TMP_Text nextText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressBar;
    public int section = 0;

    private bool cameraInitalized = false;


    // Update is called once per frame
    void Update()
    {
        if (section == 0) // movement
        {
            if (dialog.cutscenePosition == 3 && !cameraInitalized)
            {
                cameraInitalized = true;
                FindObjectOfType<CameraFollow>().InitCameraFollow();
            }

            if (dialog.cutscenePosition == 3 && dialog.enablePass)
            {
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_Movement();
        } else if (section == 1) // swapping 
        {
            // Re-enable Swap UI
            if (dialog.cutscenePosition == 2 && dialog.enablePass)
            {
                // Enable Swap UI elements
                heldTile.alpha = 1;
                swapTutText.alpha = 1;
            }
            // Re-enable swap action
            if (dialog.cutscenePosition == 3 && dialog.enablePass)
            {
                // Enable Swap UI elements
                swapTileHelper.alpha = 1;
                player.GetComponent<PlayerSwap>().canSwap = true;
            }
            if (dialog.cutscenePosition == 5 && dialog.enablePass)
            {
                // Reset the value for assessment
                curCombos = 0;
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_Swapping();
        } else if (section == 2) // placing
        {
            // Disable Held Tut Text
            if (dialog.cutscenePosition == 1 && dialog.enablePass)
            {
                swapTutText.alpha = 0;
            }
            // Re-enable Bombs UI
            if (dialog.cutscenePosition == 2 && dialog.enablePass)
            {
                inventory.enabled = true;
                selectedBomb.alpha = 1;
                inventoryRotateHelper.alpha = 1;
                selectedBombTutText.alpha = 1;
                inventoryTutText.alpha = 1;
            }
            // Re-enable Bomb placing
            if (dialog.cutscenePosition == 3 && dialog.enablePass)
            {
                placeBombHelper.alpha = 1;
                player.GetComponent<PlayerBombPlace>().canPlaceBombs = true;
            }
            if (dialog.cutscenePosition == 4 && dialog.enablePass)
            {
                // Reset the value for assessment
                curPlaces = 0;
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_Placing();
        } else if (section == 3) // spinning
        {
            // Disable Bomb Tut Text
            if (dialog.cutscenePosition == 1 && dialog.enablePass)
            {
                selectedBombTutText.alpha = 0;
                inventoryTutText.alpha = 0;
            }
            // Re-enable Spin UI
            if (dialog.cutscenePosition == 3 && dialog.enablePass)
            {
                chargeSpinHelper.alpha = 1;
                player.GetComponent<PlayerSpin>().canSpin = true;
            }
            if (dialog.cutscenePosition == 4 && dialog.enablePass)
            {
                // Reset the value for assessment
                curSpins = 0;
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_Spinning();
        } else if (section == 4) // spin charging
        {
            if (dialog.cutscenePosition == 3 && dialog.enablePass)
            {
                // Reset the value for assessment
                curChargeSpins = 0;
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_ChargeSpinning();
        }
    }

    private void Update_Progress(float percentage)
    {
        progressBar.fillAmount = percentage;

        progressText.text = String.Format("{0:0}", percentage * 100) + " %";

        // Reset after 100% reached
        if (percentage >= 1)
        {
            progressBar.gameObject.SetActive(false);
            progressBar.fillAmount = 0;
            progressText.text = "0 %";
        }
    }


    // Cache inputs
    private float horizontalAxis;
    private float verticalAxis;

    private float movedTime = 0f;
    private float moveThreshold = 3f;
    public void Assess_Movement()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        if (horizontalAxis != 0 || verticalAxis != 0)
        {
            // Should hold it for a few seconds before moving on
            movedTime += Time.deltaTime;

            float percentage = movedTime / moveThreshold;

            Update_Progress(percentage);

            if (movedTime >= moveThreshold)
            {
                // Begin new talk and enable passing
                section++; 
                dialog.NewTalk("swap_begin", "swap_end");
                dialog.enablePass = true;
                nextText.enabled = true;
            }

        }
    }

    public int curCombos = 0;
    private int comboThreshold = 5;
    public void Assess_Swapping()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        float percentage = (float)curCombos / (float)comboThreshold;

        Update_Progress(percentage);

        if (curCombos >= comboThreshold)
        {
            // Begin new talk and enable passing
            section++;
            dialog.NewTalk("place_begin", "place_end");
            dialog.enablePass = true;
            nextText.enabled = true;
        }
    }

    public int curPlaces = 0;
    private int placeThreshold = 5;
    public void Assess_Placing()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        float percentage = (float)curPlaces / (float)placeThreshold;

        Update_Progress(percentage);

        if (curPlaces >= placeThreshold)
        {
            // Begin new talk and enable passing
            section++;
            dialog.NewTalk("spin_begin", "spin_end");
            dialog.enablePass = true;
            nextText.enabled = true;
        }
    }

    public int curSpins = 0;
    private int spinThreshold = 3;
    public void Assess_Spinning()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        float percentage = (float)curSpins / (float)spinThreshold;

        Update_Progress(percentage);

        if (curSpins >= spinThreshold)
        {
            // Begin new talk and enable passing
            section++;
            dialog.NewTalk("charge_begin", "charge_end");
            dialog.enablePass = true;
            nextText.enabled = true;
        }
    }

    public int curChargeSpins = 0;
    private int chargeThreshold = 3;
    public void Assess_ChargeSpinning()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        float percentage = (float)curChargeSpins / (float)chargeThreshold;

        Update_Progress(percentage);

        if (curChargeSpins >= chargeThreshold)
        {
            // Begin new talk and enable passing
            section++;
            dialog.NewTalk("outro_begin", "outro_end");
            dialog.enablePass = true;
            nextText.enabled = true;
        }
    }

    // <summary>
    //  Resets Player ability for the start of the tutorial
    // <summary>
    private GameObject player;
    public void ConfigureAbilityLock()
    {
        player = GameObject.Find("LocalPlayer");
        player.GetComponent<PlayerSwap>().canSwap = false;
        player.GetComponent<PlayerBombPlace>().canPlaceBombs = false;
        player.GetComponent<PlayerSpin>().canSpin = false;
    }

    // <summary>
    //  Resets UI elements for the start of the tutorial
    // <summary>
    // Swap UI elements
    private CanvasGroup heldTile;
    private CanvasGroup swapTileHelper;
    private CanvasGroup swapTutText;

    // Bomb UI elements
    private Canvas inventory;
    private CanvasGroup selectedBomb;
    private TMPro.TextMeshProUGUI selectedBombText;
    private CanvasGroup inventoryRotateHelper;
    private CanvasGroup placeBombHelper;
    private Canvas warningFeed;
    private CanvasGroup selectedBombTutText;
    private CanvasGroup inventoryTutText;

    // Spin UI elements
    private CanvasGroup chargeSpinHelper;

    // Points UI elements
    private Canvas points;
    public void ConfigureInitialUI()
    {
        // Disable Timer and PlayerName
        GameObject.Find("RoundTimer_Canvas").GetComponent<Canvas>().enabled = false;
        GameObject.Find("PlayerName").GetComponent<TMPro.TextMeshProUGUI>().enabled = false;

        // Cache and disable Swap UI elements
        heldTile = GameObject.Find("PlayerHUD/HexTile").GetComponent<CanvasGroup>();
        heldTile.alpha = 0;
        swapTileHelper = GameObject.Find("Hotbar_Canvas/Swap").GetComponent<CanvasGroup>();
        swapTileHelper.alpha = 0;
        swapTutText = GameObject.Find("heldTile_tut").GetComponent<CanvasGroup>();
        swapTutText.alpha = 0;

        // Cache and disable Bomb UI elements
        inventory = GameObject.Find("AmmoDisplay_Canvas").GetComponent<Canvas>();
        inventory.enabled = false;
        selectedBomb = GameObject.Find("StackInventorySingleRadial").GetComponent<CanvasGroup>();
        selectedBomb.alpha = 0;
        selectedBombText = GameObject.Find("SelectedBombText").GetComponent<TMPro.TextMeshProUGUI>();
        selectedBombText.enabled = false;
        inventoryRotateHelper = GameObject.Find("Hotbar_Canvas/Rotate").GetComponent<CanvasGroup>();
        inventoryRotateHelper.alpha = 0;
        placeBombHelper = GameObject.Find("Hotbar_Canvas/Place").GetComponent<CanvasGroup>();
        placeBombHelper.alpha = 0;
        warningFeed = GameObject.Find("WarningFeed_Canvas").GetComponent<Canvas>();
        warningFeed.enabled = false;
        selectedBombTutText = GameObject.Find("selectedBomb_tut").GetComponent<CanvasGroup>();
        selectedBombTutText.alpha = 0;
        inventoryTutText = GameObject.Find("inventory_tut").GetComponent<CanvasGroup>();
        inventoryTutText.alpha = 0;

        // Cache and disable Spin UI elements
        chargeSpinHelper = GameObject.Find("Hotbar_Canvas/Spin").GetComponent<CanvasGroup>();
        chargeSpinHelper.alpha = 0;

        // Cache and disable Points UI elements
        points = GameObject.Find("Lives_Canvas").GetComponent<Canvas>();
        points.enabled = false;
    }
}

// TO-DO

// "Current Held Tile"
//