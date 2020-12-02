using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
	private void OnEnable()
	{
		// subscribe to event which gets triggered when an object with a Health component dies
		EventManager.Subscribe("healthObjectDied", new EventHandler<CustomEventArgs>(HandleDeathEvent));
	}

	private void HandleDeathEvent(object thingToKill, CustomEventArgs args)
	{
		// destroy the object that the health component was attached to, when this listener sees event trigger
		Destroy(args.EventObject);

		// in the future, before destroying the game object maybe this manager communicates some state changes and stuff here too
		// i wonder if it's good practice to give event listeners the ability to call their own events?
	}
}
