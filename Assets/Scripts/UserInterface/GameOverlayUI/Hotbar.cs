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

    [Header("Place")]
    [SerializeField] private Image[] placeStack = new Image[3];
    [SerializeField] private TMP_Text nextBombText;

    [Header("Spin")]
    [SerializeField] private Image spinCooldownFilter;
    [SerializeField] private TMP_Text cooldownText;
    private float spinHudTimer = 0;

    //[Header("End Round")]
    //[SerializeField] private GameObject endGameUI;


    private void Start()
    {
        bombHelper = this.gameObject.transform.parent.GetComponent<BombHelper>();
    }

    void Update()
    {
        if (localPlayer != null)
        {
            if (localPlayer.selectedTile == null) return;

            char selectedKey = localPlayer.selectedTile.GetComponentInParent<HexCell>().GetKey();

            backHex.color = bombHelper.GetKeyColor(selectedKey);
            frontHex.color = bombHelper.GetKeyColor(localPlayer.GetHeldKey());
        } else
        {
            GameObject player = GameObject.Find("LocalPlayer");
            if (player != null)
            {
                localPlayer = player.GetComponent<Player>();
                localPlayer.itemStack.Callback += OnStackChange;
            }

        }

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
        //placeStack[idx].color = GetKeyColor(key);

        placeStack[idx].sprite = bombHelper.GetKeySprite(key);
    }

}
