using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class Hotbar : MonoBehaviour
{
    private GameActions _gameActions;
    
    private Player localPlayer = null;
    [SerializeField] GameUIManager gameUIManager = null;

    private BombHelper bombHelper;

    [Header("Swap")]
    [SerializeField] private Image backHex;
    [SerializeField] private Image frontHex;
    [SerializeField] private GameObject swapDisabledEffect;
    [SerializeField] private GameObject swapKey;

    [Header("Place")]
    [SerializeField] private Image nextBomb;
    [SerializeField] private TMP_Text bombQuantityText;
    [SerializeField] private GameObject placeDisabledEffect;
    [SerializeField] private GameObject placeKey;

    [Header("Spin")]
    [SerializeField] private Image spinCooldownFilter;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private GameObject spinDisabledEffect;
    [SerializeField] private GameObject spinKey;
    private float spinHudTimer = 0;

    [Header("Rotate")]
    //[SerializeField] private GameObject rotateDisabledEffect;
    [SerializeField] private GameObject nextRotateKey;
    [SerializeField] private GameObject prevRotateKey;

    private void Awake()
    {
        _gameActions = FindObjectOfType<MenuManager>().GameActions;
    }
    
    private void Start()
    {
        bombHelper = this.gameObject.transform.parent.GetComponent<BombHelper>();

        // Set place UI to red bomb initially
        UpdateInventoryUI('r', 0);
    }

    void Update()
    {
        if (localPlayer == null)
        {
            GameObject player = GameObject.Find("LocalPlayer");
            if (player != null)
            {
                localPlayer = player.GetComponent<Player>();
                //localPlayer.itemStack.Callback += OnStackChange;
            }
        } else
        {
            UpdateHexUI();
            KeyPressListener();
            UpdateSpinDelayTimer();
        }

    }

    // Plays a button press tween anim when hot keys can be pressed
    // Plays a error sound when they cannot be pressed
    public void KeyPressListener()
    {
        if (localPlayer == null) return;

        if (_gameActions.Spin.WasPressed)
        {
            if (spinHudTimer == 0) spinKey.GetComponent<ScaleTween>().StartTween();
            else
            {
                FindObjectOfType<AudioManager>().PlaySound("error1");
            }
        }

        if (_gameActions.Swap.WasPressed)
        {
            if (!swapDisabledEffect.activeSelf) swapKey.GetComponent<ScaleTween>().StartTween();
            else
            {
                FindObjectOfType<AudioManager>().PlaySound("error1");

                string errorMessage = "<color=#FF0000>Inventory stack full</color>";
                gameUIManager.ClientCreateWarningMessage(errorMessage);
            }
        }

        if (_gameActions.Place.WasPressed)
        {
            if (!placeDisabledEffect.activeSelf) placeKey.GetComponent<ScaleTween>().StartTween();
            else
            {
                FindObjectOfType<AudioManager>().PlaySound("error1");
                string errorMessage = "<color=#FF0000>No bombs to place</color>";
                gameUIManager.ClientCreateWarningMessage(errorMessage);
            }
        }

        if (_gameActions.ChooseNextBomb.WasPressed)
        {
            nextRotateKey.GetComponent<ScaleTween>().StartTween();
        }

        if (_gameActions.ChoosePreviousBomb.WasPressed)
        {
            prevRotateKey.GetComponent<ScaleTween>().StartTween();
        }
    }
    
    public void UpdateSpinDelayTimer()
    {
        if (spinHudTimer != 0)
        {
            float totalDuration = localPlayer.GetComponent<PlayerSpin>().spinTotalCooldown;
            spinCooldownFilter.fillAmount = spinHudTimer / totalDuration;
            spinHudTimer -= Time.deltaTime;

            cooldownText.text = spinHudTimer.ToString("F1");

            if (spinHudTimer < 0)
            {
                spinHudTimer = 0;
                spinCooldownFilter.fillAmount = 0;
                cooldownText.text = "";
            }
        }
    }

    public void UpdateHexUI()
    {
        if (localPlayer.GetComponent<PlayerSwap>().selectedTile == null) return;

        char selectedKey = localPlayer.GetComponent<PlayerSwap>().selectedTile.GetComponentInParent<HexCell>().GetKey();

        backHex.color = BombHelper.GetKeyColor(selectedKey);
        frontHex.color = BombHelper.GetKeyColor(localPlayer.GetComponent<PlayerSwap>().heldKey);
    }

    public void StartSpinCooldown()
    {
        if (localPlayer != null)
        {
            spinHudTimer = localPlayer.GetComponent<PlayerSpin>().spinTotalCooldown;
        }
    }

    public void SwapHexes(char newFrontKey)
    {
        frontHex.color = BombHelper.GetKeyColor(newFrontKey);
    }

    public void UpdateInventoryUI(char key, int amt)
    {
        if (amt > 0) placeDisabledEffect.SetActive(false);
        else placeDisabledEffect.SetActive(true);

        bombQuantityText.text = amt.ToString();

        nextBomb.sprite = bombHelper.GetKeySprite(key);
    }
}
