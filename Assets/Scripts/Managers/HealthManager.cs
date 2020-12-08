using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// i think in the current state the healthmanager is overrelying on the eventmanager a little too much.
// shouldn't a healthmanager be more closely coupled to the actual health components rather than trying to 
// manipulate them through the event system?

// on the surface, it seems like this class would be managing every health component in the scene, but
// atm it's only coupling with health components is via the eventmanager

public class HealthManager : MonoBehaviour
{
	private void OnEnable()
	{
		// subscribe to event which gets triggered when an object with a Health component dies
		EventManager.Subscribe("healthObjectDied", new EventHandler<CustomEventArgs>(HandleDeathEvent));
		EventManager.Subscribe("healthChanged", new EventHandler<CustomEventArgs>(HandleHealthChangeEvent));
	}

	// EventHandler listeners
	private void HandleHealthChangeEvent(object thingWithHealth, CustomEventArgs args)
	{
		if (args.EventObject.CompareTag("Player"))
		{
			EventManager.TriggerEvent("playerHealthChanged", this, new CustomEventArgs { Amount = args.Amount, EventObject = args.EventObject });
		}
	}

	private void HandleDeathEvent(object thingToKill, CustomEventArgs args)
	{
		// destroy the object that the health component was attached to, when this listener sees event trigger
		
		if (args.EventObject.CompareTag("Player"))
		{
			// for player deaths, we hide the gameobject instead of destroying it. 
			// trigger event of name playerDeath from class HealthManager with custom arg of player that just died
			EventManager.TriggerEvent("playerDied", this, new CustomEventArgs { EventObject = args.EventObject.gameObject });
		}
		else
		{
			Destroy(args.EventObject);
		}

		// in the future, before destroying the game object maybe this manager communicates some state changes and stuff here too
		// i wonder if it's good practice to give event listeners the ability to call their own events?
	}
}
