using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialFrameBombTypeIndicator : MonoBehaviour
{
	public Image frameTypeImage;

	public Sprite radialFrameBombSprite;

	public Sprite radialFrameDeployableSprite;


	public void SwapType(int frameType)
	{
		RectTransform rect = GetComponent<RectTransform>();

		switch (frameType)
		{
			case 0:
				frameTypeImage.sprite = radialFrameBombSprite;
				break;
			case 1:
				frameTypeImage.sprite = radialFrameDeployableSprite;
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
	public void SetFrameColor(int index)
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
