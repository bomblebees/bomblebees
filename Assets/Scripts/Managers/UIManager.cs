using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
	private static UIManager uiManager;
	public static UIManager Instance { get { return uiManager; } }

	[SerializeField]
	private TextMeshProUGUI Player1HealthText;
	private EventHandler<CustomEventArgs> updateUIListener;

	private void Awake()
	{
		if (uiManager != null && uiManager != this)
		{
			Destroy(gameObject);
		} else
		{
			uiManager = this;
		}
	}

	private void OnEnable()
	{
		// add listener function to health change event using EventManager singleton
		updateUIListener = new EventHandler<CustomEventArgs>(UpdateUIEvent);
		EventManager.Subscribe("healthChanged", updateUIListener);
	}

	private void OnDisable()
	{

	}

	private void UpdateUIEvent(object healthComponent, CustomEventArgs args)
	{
		// Runs when the change health event is invoked in our Health script
		// Update the p1 health text we dragged in through the inspector
		Debug.Log("new HP amount is " + args.Amount);
		Debug.Log(Player1HealthText);
		Player1HealthText.text = "P1 Health: " + args.Amount;
	}
}
