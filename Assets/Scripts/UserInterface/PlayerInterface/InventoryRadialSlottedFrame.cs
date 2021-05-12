using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryRadialSlottedFrame : MonoBehaviour
{
	public Image slottedFrameImage;

	public Sprite threeSlottedFrameSprite;

	public Sprite fourSlottedFrameSprite;

	public Sprite fiveSlottedFrameSprite;

	/// <summary>
	/// Swaps the UI frame of this element to the respective sprite
	/// </summary>
	/// <param name="slotSize"></param>
	public void SwapFrame(int slotSize)
	{
		switch (slotSize)
		{
			case 3:
				slottedFrameImage.sprite = threeSlottedFrameSprite;
				break;
			case 4:
				slottedFrameImage.sprite = fourSlottedFrameSprite;
				break;
			case 5:
				slottedFrameImage.sprite = fiveSlottedFrameSprite;
				break;
			default:
				// Debug.Log("Tried to switch inventory frame to unavailable UI sprite");
				break;
		}
	}
}
