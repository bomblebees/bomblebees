using UnityEngine;
using System.Collections;

/// <summary>
/// This script is included for the sake of the demo scene 
/// and is not required to use Click to Bind - feel free to delete
/// </summary>
public class Listener : MonoBehaviour {

	//Script used as a example of how to use keyBindingManager

	public GameObject box1;
	public GameObject box2;
	public GameObject box3;
	public GameObject box4;
	
	// Update is called once per frame
	void Update () {

		if(KeyBindingManager.GetKeyDown(KeyAction.Up))
		{
			box1.SetActive(!box1.activeSelf);
		}

		if(KeyBindingManager.GetKeyDown(KeyAction.Down))
		{
			box2.SetActive(!box2.activeSelf);
		}

		if(KeyBindingManager.GetKeyDown(KeyAction.Left))
		{
			box3.SetActive(!box3.activeSelf);
		}
				
		if(KeyBindingManager.GetKeyDown(KeyAction.Right))
		{
			box4.SetActive(!box4.activeSelf);
		}
	}
}
