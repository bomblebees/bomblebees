using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[System.Serializable]
//[RequireComponent(typeof(SaveKeyBinding))] //used to save data to database
public class KeyBinding : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

	public delegate void remap(KeyBinding key);
	public static event remap keyRemap;

	//Name comes from enum in KeyBindingsManager scrip
	public KeyAction keyAction;
	//keycode set in inspector and by player
	public KeyCode keyCode = KeyCode.W;
	//Text to display keycode for user feedback
	public Text keyDisplay;

	//Used for color changing during key binding
	public GameObject button;
	public Color toggleColor = new Color(0.75f,0.75f,0.75f,1f);
	Image buttonImage;
	Color originalColor;

    //mouse rebinding
    public bool allowMouseButtons = true;

	//Internal variables
	bool reassignKey = false;
	Event curEvent;
    bool isHovering = false; //used for mouse controls

	//Changes in button behavior should be made here
	void OnGUI()
	{
        curEvent = Event.current;

        //Checks if key is pressed and if button has been pressed indicating wanting to re - assign
        if (curEvent.isKey && curEvent.keyCode != KeyCode.None && reassignKey)
        {
            keyCode = curEvent.keyCode;
            ChangeKeyCode(false);
            UpdateKeyCode();
            SaveKeyCode();
        }
        else if (curEvent.shift && reassignKey) //deals with some oddity in how Unity deals with shifts keys
        {
            if (Input.GetKey(KeyCode.LeftShift))
                keyCode = KeyCode.LeftShift;
            else
                keyCode = KeyCode.RightShift;

            ChangeKeyCode(false);
            UpdateKeyCode();
            SaveKeyCode();
        }
        //checks if mouse is pressed and assigns appropriate keycode
        else if (curEvent.isMouse && reassignKey && isHovering && allowMouseButtons)
        {
            StartCoroutine(Wait());//prevents "over clicking"
            //converts mouse button to keycode - see Keycode defintion for why 323 is added.
            int _int = curEvent.button + 323;
            KeyCode mouseKeyCode = (KeyCode)_int;

            keyCode = mouseKeyCode;
            ChangeKeyCode(false);
            UpdateKeyCode();
            SaveKeyCode();
            
        }
        //cancels binding if not hovering and mouse clicked
        else if(curEvent.isMouse && !isHovering)
        {
            ChangeKeyCode(false);
        }
	}

    public void OnPointerEnter(PointerEventData data)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        isHovering = false;
    }

    //Initializes
    void Awake()
	{
		buttonImage = button.GetComponent<Image>();
		originalColor = buttonImage.color;
		button.GetComponent<Button>().onClick.AddListener(() => ChangeKeyCode(true));
	}

	//Loads keycodes from player preferences
	void OnEnable()
	{
		//Comment out this line it you want to allow multiple simultaneous assignments
		KeyBinding.keyRemap += PreventDoubleAssign;

		KeyCode tempKey;
		tempKey = (KeyCode) PlayerPrefs.GetInt(keyAction.ToString()); //saved by the keyType, but returns int that corresponds to keyCode

		if(tempKey.ToString() == "None")
		{
			Debug.Log(keyCode.ToString());
			keyDisplay.text = keyCode.ToString();	
			UpdateKeyCode();
			SaveKeyCode();
		}
		else
		{	
			keyCode = tempKey;
			keyDisplay.text = keyCode.ToString();	
			UpdateKeyCode();
		}
	}

	void OnDisable()
	{
		KeyBinding.keyRemap -= PreventDoubleAssign;
	}

	//Called by button on GUI
	public void ChangeKeyCode(bool toggle)
	{
        reassignKey = toggle;

		if(toggle)
		{
			buttonImage.color = toggleColor;

			if(keyRemap != null)
				keyRemap(this);
		}
		else
			buttonImage.color = originalColor;
	}
		
	//saves keycode to player prefs
	public void SaveKeyCode()
	{
		keyDisplay.text = keyCode.ToString();
		PlayerPrefs.SetInt(keyAction.ToString(),(int)keyCode);
		PlayerPrefs.Save();
	}

	//Prevents user from remapping two keys at the same time
	void PreventDoubleAssign(KeyBinding kb)
	{
		if(kb != this)
		{
			reassignKey = false;
			buttonImage.color = originalColor;
		}
	}

	//updates dictionary on key bindings manager
	public void UpdateKeyCode()
	{		
		KeyBindingManager.UpdateDictionary(this);
	}

    //prevent button from getting clicked.
    IEnumerator Wait()
    {
        button.GetComponent<Button>().onClick.RemoveAllListeners();
        yield return new WaitUntil(() => !Input.GetMouseButton(0));
        button.GetComponent<Button>().onClick.AddListener(() => ChangeKeyCode(true));
    }

    
}
