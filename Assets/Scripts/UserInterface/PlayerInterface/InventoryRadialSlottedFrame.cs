using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryRadialSlottedFrame : MonoBehaviour
{
	public Image slottedFrameImage;

	[Tooltip("Scale for the generic frame without the bomb/deployable icons")]
	[SerializeField] private Vector2 genericFrameSize;
	[Tooltip("Scale for the frame with the bomb/deployable bits attached")]
	[SerializeField] private Vector2 embellishedFrameSize;

	public Sprite threeSlottedFrameSprite;

	public Sprite fourSlottedFrameSprite;

	public Sprite fiveSlottedFrameSprite;

	public Sprite threeSlottedBombFrame;
	public Sprite fourSlottedBombFrame;
	public Sprite fiveSlottedBombFrame;
	public Sprite threeSlottedDeployableFrame;
	public Sprite fourSlottedDeployableSprite;
	public Sprite fiveSlottedDeployableSprite;

	/// <summary>
	/// Swaps the UI frame of this element to the respective sprite
	/// </summary>
	/// <param name="slotSize"></param>
	public void SwapFrame(int slotSize)
	{
		RectTransform rect = GetComponent<RectTransform>();
		switch (slotSize)
		{
			case 3:
				rect.sizeDelta = genericFrameSize;
				slottedFrameImage.sprite = threeSlottedFrameSprite;
				break;
			case 4:
				rect.sizeDelta = genericFrameSize;
				slottedFrameImage.sprite = fourSlottedFrameSprite;
				break;
			case 5:
				rect.sizeDelta = genericFrameSize;
				slottedFrameImage.sprite = fiveSlottedFrameSprite;
				break;
			default:
				// Debug.Log("Tried to switch inventory frame to unavailable UI sprite");
				break;
		}
	}
}
