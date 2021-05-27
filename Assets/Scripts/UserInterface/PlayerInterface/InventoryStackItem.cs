using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStackItem : MonoBehaviour
{
    public Image invSlotRadial;
    public GameObject slottedFrame;
    public TMP_Text invCounter;
    public TMP_Text invAddText;
	public Sprite[] radialFillColors = new Sprite[4];
}
