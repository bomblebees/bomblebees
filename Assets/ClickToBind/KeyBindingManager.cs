using UnityEngine;
using System.Collections.Generic;


//static class that stores the key dictionary. The dictionary is loaded at runtime from Keybinding scripts.
//The keybinding scripts will load from the inspector unless there is a corresponding key in player prefs.
public static class KeyBindingManager  {

	public static Dictionary<KeyAction, KeyCode> keyDict = new Dictionary<KeyAction, KeyCode>();

	//Returns key code
	public static KeyCode GetKeyCode(KeyAction key)
	{
		KeyCode _keyCode = KeyCode.None;
		keyDict.TryGetValue(key, out _keyCode);
		return _keyCode;
	}

	//Use in place of Input.GetKey
	public static bool GetKey(KeyAction key)
	{
		KeyCode _keyCode = KeyCode.None;
		keyDict.TryGetValue(key, out _keyCode);
		return Input.GetKey(_keyCode);
	}

	//Use in place of Input.GetKeyDown
	public static bool GetKeyDown(KeyAction key)
	{
		KeyCode _keyCode = KeyCode.None;
		keyDict.TryGetValue(key, out _keyCode);
		return Input.GetKeyDown(_keyCode);
	}

	//Use in place of Input.GetKeyUP
	public static bool GetKeyUp(KeyAction key)
	{
		KeyCode _keyCode = KeyCode.None;
		keyDict.TryGetValue(key, out _keyCode);
		return Input.GetKeyUp(_keyCode);
	}

	public static void UpdateDictionary(KeyBinding key)
	{
        if (!keyDict.ContainsKey(key.keyAction))
            keyDict.Add(key.keyAction, key.keyCode);
        else
            keyDict[key.keyAction] = key.keyCode;
	}
}

//used to safe code inputs
//Add new keys to "bind" here
public enum KeyAction
{
	Up,
	Down,
	Left,
	Right,
	ToggleSettings,
	Swap,
	Place,
	Spin,
	RotateNext,
	RotatePrevious,
	BigBomb,
	SludgeBomb,
	LaserBeem,
	PlasmaBall
}
