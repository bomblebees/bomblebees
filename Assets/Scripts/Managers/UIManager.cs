using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
// using UnityEngine.UIElements;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }

	// any better way other than to spam serialize fields for pluggable ui elements in editor?
	[SerializeField]
	private TextMeshProUGUI Player1HealthText;
	[SerializeField]
	private GameObject pregamePanel;
	[SerializeField]
	private TextMeshProUGUI LivesNumberText;
	[SerializeField]
	private TextMeshProUGUI HPNumberText;
	[SerializeField]
	private GameObject numLivesSlider;
	[SerializeField]
	private GameObject maxHPSlider;

	GameStartData gameData = new GameStartData();


	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
		} else
		{
			Instance = this;
		}
	}

	private void OnEnable()
	{
		// add listener function to health change event using EventManager singleton
		
		EventManager.Subscribe("healthChanged", new EventHandler<CustomEventArgs>(UpdateUIEvent));
		Player1HealthText.gameObject.SetActive(false);

		gameData.NumLives = (int)numLivesSlider.GetComponent<Slider>().value;
		gameData.MaxHP = (int)maxHPSlider.GetComponent<Slider>().value;

	}

	public void StartGameClicked()
	{
		// called when gameStartButton is clicked

		// hide the pregame settings panel
		pregamePanel.SetActive(false);
		// refresh game UI once with the data we have rn
		initialGameUIRefresh();
		// trigger the event using our event manager
		EventManager.TriggerEvent("gameStart", this, new CustomEventArgs { DataObject = gameData });

		// in a real game initialization scenario, idk if we would load the UI before we load the players into the scene. 
	}

	public void UpdateLivesSliderText(float value)
	{
		LivesNumberText.text = value.ToString();
		gameData.NumLives = (int)value;
	}

	public void UpdateMaxHPSliderText(float value)
	{
		HPNumberText.text = value.ToString();
		gameData.MaxHP = (int)value;
	}

	private void UpdateUIEvent(object healthComponent, CustomEventArgs args)
	{
		Player1HealthText.gameObject.SetActive(true);
		Player1HealthText.text = args.Amount.ToString();
	}

	private void initialGameUIRefresh()
	{
		Player1HealthText.gameObject.SetActive(true);
		Debug.Log("max hp is " + gameData.MaxHP);
		Player1HealthText.text = gameData.MaxHP.ToString();
	}
}
