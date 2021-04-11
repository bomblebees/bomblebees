﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class Hotbar : MonoBehaviour
{
    private Player localPlayer = null;


    [Header("Swap")]
    [SerializeField] private Image backHex;
    [SerializeField] private Image frontHex;

    [Header("Place")]
    [SerializeField] private Image[] placeStack = new Image[3];
    [SerializeField] private TMP_Text nextBombText;

    //[Header("End Round")]
    //[SerializeField] private GameObject endGameUI;


    //private void Start()
    //{
    //    localPlayer = GameObject.Find("LocalPlayer").GetComponent<Player>();
    //}

    void Update()
    {
        if (localPlayer != null)
        {
            if (localPlayer.selectedTile == null) return;

            char selectedKey = localPlayer.selectedTile.GetComponentInParent<HexCell>().GetKey();

            backHex.color = GetKeyColor(selectedKey);
            frontHex.color = GetKeyColor(localPlayer.GetHeldKey());
        } else
        {
            GameObject player = GameObject.Find("LocalPlayer");
            if (player != null)
            {
                localPlayer = player.GetComponent<Player>();
                localPlayer.itemStack.Callback += OnStackChange;
            }

        }
    }

    public void SwapHexes(char newFrontKey)
    {
        frontHex.color = GetKeyColor(newFrontKey);
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
                if (i == 0) nextBombText.text = GetBombTextByKey(reversedStack[i]);
            }
            else // Update the rest of stack
            {
                UpdateStackUI(i, 'e');
                if (i == 0) nextBombText.text = "None";
            }
        }


    }

    void UpdateStackUI(int idx, char key)
    {
        placeStack[idx].color = GetKeyColor(key);
    }

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

    private string GetBombTextByKey(char key)
    {
        switch (key)
        {
            case 'b': return "Blink";
            case 'g': return "Plasma";
            case 'y': return "Laser";
            case 'r': return "Big";
            case 'p': return "Sludge";
            case 'w': return "Queen Bee";
            default: return "None";
        }
    }

}
