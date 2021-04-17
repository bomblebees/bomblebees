using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class Hotbar : MonoBehaviour
{
    private Player localPlayer = null;

    private BombHelper bombHelper;

    [Header("Swap")]
    [SerializeField] private Image backHex;
    [SerializeField] private Image frontHex;
    [SerializeField] private GameObject swapDisabledEffect;
    [SerializeField] private GameObject swapKey;

    [Header("Place")]
    [SerializeField] private Image[] placeStack = new Image[3];
    [SerializeField] private TMP_Text nextBombText;
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

        // Set all UI to empty initially
        for (int i = 0; i < 3; i++)
        {
            UpdateStackUI(i, 'e');
        }

        // Turn on Disable place/rotate effect
        placeDisabledEffect.SetActive(true);
        rotateDisabledEffect.SetActive(true);
    }

    void Update()
    {
        if (localPlayer == null)
        {
            GameObject player = GameObject.Find("LocalPlayer");
            if (player != null)
            {
                localPlayer = player.GetComponent<Player>();
                localPlayer.itemStack.Callback += OnStackChange;
            }
        } else
        {
            UpdateHexUI();
            KeyPressListener();
        }

        UpdateSpinDelayTimer();
    }

    // Plays a button press tween anim when hot keys can be pressed
    public void KeyPressListener()
    {
        if (localPlayer == null) return;

        if (Input.GetKeyDown(localPlayer.spinKey) && spinHudTimer == 0)
        {
            spinKey.GetComponent<IconBounceTween>().OnTweenStart();
        }

        if (Input.GetKeyDown(localPlayer.swapKey) && !swapDisabledEffect.activeSelf)
        {
            swapKey.GetComponent<IconBounceTween>().OnTweenStart();
        }

        if (Input.GetKeyDown(localPlayer.bombKey) && !placeDisabledEffect.activeSelf)
        {
            placeKey.GetComponent<IconBounceTween>().OnTweenStart();
        }

        if (Input.GetKeyDown(localPlayer.rotateKey) && !rotateDisabledEffect.activeSelf)
        {
            rotateKey.GetComponent<IconBounceTween>().OnTweenStart();
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

    void OnStackChange(SyncList<char>.Operation op, int idx, char oldColor, char newColor)
    {
        SyncList<char> stack = localPlayer.itemStack;

        // If stack empty, turn on disable effect, otherwise turn off
        if (localPlayer.itemStack.Count == 0)
        {
            placeDisabledEffect.SetActive(true);
            rotateDisabledEffect.SetActive(true);
            swapDisabledEffect.SetActive(false);
        } else
        {
            // if more than one item, can rotate
            if (localPlayer.itemStack.Count > 1) rotateDisabledEffect.SetActive(false);
            else rotateDisabledEffect.SetActive(true);

            // if max items, cannot swap
            if (localPlayer.itemStack.Count == 3) swapDisabledEffect.SetActive(true);
            else swapDisabledEffect.SetActive(false);

            placeDisabledEffect.SetActive(false);
        }

        List<char> reversedStack = new List<char>();

        // Reverse the stack
        for (int i = stack.Count - 1; i >= 0; i--)
        {
            reversedStack.Add(stack[i]);
        }

        // Apply to UI
        for (int i = 0; i < 3; i++)
        {
            // Update the stack with player stack
            if (i < reversedStack.Count)
            {
                UpdateStackUI(i, reversedStack[i]);
                if (i == 0) nextBombText.text = bombHelper.GetBombTextByKey(reversedStack[i]);
            }
            else // Update the rest of stack
            {
                UpdateStackUI(i, 'e');
                if (i == 0) nextBombText.text = "None";
            }
        }


        if (op == SyncList<char>.Operation.OP_ADD)
        {
            Debug.Log("Move up stack");
        } else if (op == SyncList<char>.Operation.OP_REMOVEAT)
        {
            Debug.Log("Move down stack");
        }

    }

    void UpdateStackUI(int idx, char key)
    {
        placeStack[idx].sprite = bombHelper.GetKeySprite(key);
    }

}
