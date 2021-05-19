using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeldHexUIIcon : MonoBehaviour
{
    // GameObject
    public Image tileTypeImage;

    // Sprites
	public Sprite bombIcon;
    public Sprite honeyIcon;
    public Sprite laserIcon;
    public Sprite plasmaIcon;


    /// <summary>
	/// Changes the sprite of the icon image based on the current held tile passed by its index
	/// </summary>
	public void SwapType(int tileType)
	{
		switch (tileType)
		{
			case 0:
				tileTypeImage.sprite = bombIcon;
				break;
			case 1:
				tileTypeImage.sprite = honeyIcon;
				break;
            case 2:
				tileTypeImage.sprite = laserIcon;
				break;
            case 3:
				tileTypeImage.sprite = plasmaIcon;
				break;
			default:
				// Debug.Log("Tried to switch inventory frame to unavailable UI sprite");
				break;
		}
	}

	/// <summary>
	/// Changes the color of the icon based on the current held tile passed by its index
	/// </summary>
	public void SetIconColor(int index)
	{
		Image tileImage = GetComponent<Image>();
		switch (index)
		{
			case 0: // red/bomble bomb
				// tileImage.color = new Color32(152, 0, 0, 255);
                tileImage.color = new Color32(255, 0, 2, 255);
				break;
			case 1: // yellow/honey bomb
				// tileImage.color = new Color32(154, 112, 0, 255);
                tileImage.color = new Color32(255, 236, 0, 255);
				break;
			case 2: // blue/laser beem
				// tileImage.color = new Color32(21,/ 57, 99, 255);
                tileImage.color = new Color32(0, 176, 255, 255);
				break;
			case 3: // green/plasma bomb
				// tileImage.color = new Color32(48, 101, 0, 255);
                tileImage.color = new Color32(41, 214, 53, 255);
				break;

		}
	}
}
