using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    private Player localPlayer = null;


    [Header("Swap")]
    //[SerializeField] private GameObject startGameUI;
    [SerializeField] private Image backHex;
    [SerializeField] private Image frontHex;

    //[Header("End Round")]
    //[SerializeField] private GameObject endGameUI;


    //private void Start()
    //{
    //    localPlayer = GameObject.Find("LocalPlayer").GetComponent<Player>();
    //}

    void Update()
    {
        if (localPlayer != null && localPlayer.selectedTile != null)
        {
            char selectedKey = localPlayer.selectedTile.GetComponentInParent<HexCell>().GetKey();
            backHex.color = GetKeyColor(selectedKey);
            frontHex.color = GetKeyColor(localPlayer.GetHeldKey());
        } else
        {
            GameObject player = GameObject.Find("LocalPlayer");
            if (player != null) localPlayer = player.GetComponent<Player>();
        }
    }

    public void SwapHexes(char newFrontKey)
    {
        frontHex.color = GetKeyColor(newFrontKey);
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

}
