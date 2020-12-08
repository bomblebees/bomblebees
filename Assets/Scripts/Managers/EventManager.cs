/* 
 * referenced:
 * https://learn.unity.com/tutorial/create-a-simple-messaging-system-with-events#5cf5960fedbc2a281acd21fa <- unity learn course
 * https://docs.microsoft.com/en-us/dotnet/api/system.eventhandler?view=net-5.0 <- microsoft c# docs
 * https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/how-to-publish-events-that-conform-to-net-framework-guidelines <- microsoft c# docs
 * 
 * more advanced singleton implementations: https://wiki.unity3d.com/index.php/Singleton
 * 
 * Creates a Dictionary of events other classes can subscribe to using this singleton instance
 * 
 * Subscribing to a non-existent event creates it and stores it in the Dictionary, which other classes can Trigger
 * and send data using a CustomEventArgs object deriving from EventArgs
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
	// the event is the subject to which observers can subscribe to, and this EventManager class helps observers with subscription and unsubscription methods from these events.
	// dictionary of EventHandlers to which other managers and classes can subscribe to
	private Dictionary<string, EventHandler<CustomEventArgs>> eventDictionary;

	private static EventManager eventManager;


	public static EventManager instance
	{
		// "true" singleton ensures that no other instance can be found in scene at one time
		// when other classes use the singleton instance (EventManager.instance) this method returns

		get
		{
			if (!eventManager)
			{
				eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

				if (!eventManager)
				{
					Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
				}
				else
				{
					eventManager.Init();
				}
			}

			return eventManager;
		}
	}

	void Init()
	{
		if (eventDictionary == null)
		{
			eventDictionary = new Dictionary<string, EventHandler<CustomEventArgs>>();
		}
	}

	// pass in name of event and the subscribing function which follows the EventHandler delegate type
	public static void Subscribe(string eventName, EventHandler<CustomEventArgs> func)
	{
		EventHandler<CustomEventArgs> thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			// if the event exists in dictionary, then subscribe func to event
			thisEvent += func;
		}
		else
		{
			// if event doesn't exist in dictionary, subscribe func to event, and then add event to dictionary
			thisEvent += func;
			instance.eventDictionary.Add(eventName, thisEvent);
		}
	}

	public static void Unsubscribe(string eventName, EventHandler<CustomEventArgs> func)
	{
		EventHandler<CustomEventArgs> thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			// if the event exists in dictionary, then subscribe func to event
			thisEvent -= func;
		}
		else
		{
			Debug.Log("event name " + eventName + " doesn't exist!");
		}
	}

	public static void TriggerEvent (string eventName, object eventObject, CustomEventArgs args)
	{
		// Debug.Log("event id " + eventName + " emitted from " + eventObject);
		EventHandler<CustomEventArgs> thisEvent = null;
		if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent?.Invoke(eventObject, args);
		}
	}

}
