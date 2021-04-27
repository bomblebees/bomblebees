using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class Hotbar : MonoBehaviour
{
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
    [SerializeField] private GameObject rotateDisabledEffect;
    [SerializeField] private GameObject rotateKey;

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
        }

        UpdateSpinDelayTimer();
    }

    // Plays a button press tween anim when hot keys can be pressed
    // Plays a error sound when they cannot be pressed
    public void KeyPressListener()
    {
        if (localPlayer == null) return;

        if (Input.GetKeyDown(localPlayer.spinKey))
        {
            if (spinHudTimer == 0) spinKey.GetComponent<IconBounceTween>().OnTweenStart();
            else
            {
                FindObjectOfType<AudioManager>().PlaySound("error1");
            }
        }

        if (Input.GetKeyDown(localPlayer.swapKey))
        {
            if (!swapDisabledEffect.activeSelf) swapKey.GetComponent<IconBounceTween>().OnTweenStart();
            else
            {
                FindObjectOfType<AudioManager>().PlaySound("error1");

                string errorMessage = "<color=#FF0000>Inventory stack full</color>";
                gameUIManager.ClientCreateWarningMessage(errorMessage);
            }
        }

        if (Input.GetKeyDown(localPlayer.bombKey))
        {
            if (!placeDisabledEffect.activeSelf) placeKey.GetComponent<IconBounceTween>().OnTweenStart();
            else
            {
                FindObjectOfType<AudioManager>().PlaySound("error1");
                string errorMessage = "<color=#FF0000>No bombs to place</color>";
                gameUIManager.ClientCreateWarningMessage(errorMessage);
            }
        }

        if (Input.GetKeyDown(localPlayer.rotateKey))
        {
            if (!rotateDisabledEffect.activeSelf) rotateKey.GetComponent<IconBounceTween>().OnTweenStart();
            else
            {
                // FindObjectOfType<AudioManager>().PlaySound("error1");
            }
        }
    }
    
    public void UpdateSpinDelayTimer()
    {
        if (spinHudTimer != 0)
        {
            float totalDuration = localPlayer.spinTotalCooldown;
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
        if (localPlayer.selectedTile == null) return;

        char selectedKey = localPlayer.selectedTile.GetComponentInParent<HexCell>().GetKey();

        backHex.color = bombHelper.GetKeyColor(selectedKey);
        frontHex.color = bombHelper.GetKeyColor(localPlayer.GetHeldKey());
    }

    public void StartSpinCooldown()
    {
        if (localPlayer != null)
        {
            spinHudTimer = localPlayer.spinTotalCooldown;
        }
    }

    public void SwapHexes(char newFrontKey)
    {
        frontHex.color = bombHelper.GetKeyColor(newFrontKey);
    }

    public void UpdateInventoryUI(char key, int amt)
    {
        if (amt > 0) placeDisabledEffect.SetActive(false);
        else placeDisabledEffect.SetActive(true);

        bombQuantityText.text = amt.ToString();

        nextBomb.sprite = bombHelper.GetKeySprite(key);
    }

}
