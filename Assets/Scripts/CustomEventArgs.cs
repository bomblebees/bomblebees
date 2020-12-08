using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// sort of like a struct, instantiated upon event trigger with desired custom parameters to pass along with the events
public class CustomEventArgs : EventArgs
{
	public int Amount { get; set; }
	public GameObject EventObject { get; set; }
	public GameStartData DataObject { get; set; }
}

