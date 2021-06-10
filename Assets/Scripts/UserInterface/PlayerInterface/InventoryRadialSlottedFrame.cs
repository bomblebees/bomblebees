using UnityEngine;
using UnityEngine.UI;

public class InventoryRadialSlottedFrame : MonoBehaviour
{
	public Image slottedFrameImage;

	[Tooltip("Scale for the generic frame without the bomb/deployable icons")]
	[SerializeField] private Vector2 genericFrameSize;
	[Tooltip("Scale for the frame with the bomb/deployable bits attached")]
	[SerializeField] private Vector2 embellishedFrameSize;

	[SerializeField] private bool isLocalPlayerHUD = false;

	public Sprite threeSlottedFrameSprite;

	public Sprite fourSlottedFrameSprite;

	public Sprite fiveSlottedFrameSprite;

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

	/// <summary>
	/// Changes the color of the frame based on the currently selected slot passed by its index
	/// </summary>
	/// <param name=""></param>
	public void SetSlotColor(int index)
	{
		Image frameImage = GetComponent<Image>();
		switch (index)
		{
			case 0: // red/bomble bomb
				frameImage.color = new Color32(152, 0, 0, 255);
				break;
			case 1: // yellow/honey bomb
				frameImage.color = new Color32(154, 112, 0, 255);
				break;
			case 2: // blue/laser beem
				frameImage.color = new Color32(21, 57, 99, 255);
				break;
			case 3: // green/plasma bomb
				frameImage.color = new Color32(48, 101, 0, 255);
				break;

		}
	}
}
