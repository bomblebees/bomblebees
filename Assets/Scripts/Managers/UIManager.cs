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
	private TextMeshProUGUI Player2HealthText;
	[SerializeField]
	private TextMeshProUGUI Player1LivesText;
	[SerializeField]
	private TextMeshProUGUI Player2LivesText;

	[SerializeField]
	private GameObject pregamePanel;
	[SerializeField]
	private TextMeshProUGUI NumLivesSliderText;
	[SerializeField]
	private TextMeshProUGUI MaxHPNumberSliderText;
	[SerializeField]
	private GameObject numLivesSlider;
	[SerializeField]
	private GameObject maxHPSlider;
	[SerializeField]
	private GameObject numPlayersDropdown;

	private GameStartData gameData = new GameStartData();
	private List<Player> playerList;

	private List<TextMeshProUGUI> playerHealthUIList;
	private List<TextMeshProUGUI> playerLivesUIList;

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
		
		EventManager.Subscribe("playerHealthChanged", new EventHandler<CustomEventArgs>(UpdatePlayerHealthEvent));
		Player1HealthText.gameObject.SetActive(false);
		Player2HealthText.gameObject.SetActive(false);
		Player1LivesText.gameObject.SetActive(false);
		Player2LivesText.gameObject.SetActive(false);

		// add HP and Lives Remaining text fields to list so we dont need conditionals on receiving health update events
		playerHealthUIList = new List<TextMeshProUGUI>();
		playerHealthUIList.Add(Player1HealthText);
		playerHealthUIList.Add(Player2HealthText);

		playerLivesUIList = new List<TextMeshProUGUI>();
		playerLivesUIList.Add(Player1LivesText);
		playerLivesUIList.Add(Player2LivesText);

		gameData.NumLives = (int)numLivesSlider.GetComponent<Slider>().value;
		gameData.MaxHP = (int)maxHPSlider.GetComponent<Slider>().value;
		gameData.NumPlayers = numPlayersDropdown.GetComponent<TMP_Dropdown>().value + 1;

		playerList = LevelManager.Instance.PlayerList;
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
		NumLivesSliderText.text = value.ToString();
		gameData.NumLives = (int)value;
	}

	public void UpdateMaxHPSliderText(float value)
	{
		MaxHPNumberSliderText.text = value.ToString();
		gameData.MaxHP = (int)value;
	}

	public void UpdatePlayerCount(int value)
	{
		// add one since dropdown options array starts at 0
		gameData.NumPlayers = value + 1;

	}

	// public void PlayerDeathUIChange(Player player)
	// {
	// 	// change number of lives and reset hp UI
	// 	for (int i = 0; i < playerList.Count; i++)
	// 	{
	// 		if (playerList[i].gameObject != null && playerList[i].gameObject == player.gameObject)
	// 		{
	// 			playerHealthUIList[i].text = player.GetComponent<Health>().CurrentHealth.ToString();
	// 			playerLivesUIList[i].text = player.lives.ToString();
	// 		}
	// 	}
	// }

	private void initialGameUIRefresh()
	{
		Player1HealthText.gameObject.SetActive(true);
		Player1LivesText.gameObject.SetActive(true);

		if (gameData.NumPlayers == 2)
		{
			Player2HealthText.gameObject.SetActive(true);
			Player2LivesText.gameObject.SetActive(true);
		}
		Debug.Log("max hp is " + gameData.MaxHP);
		Player1HealthText.text = gameData.MaxHP.ToString();
		Player2HealthText.text = gameData.MaxHP.ToString();
		Player1LivesText.text = gameData.NumLives.ToString();
		Player2LivesText.text = gameData.NumLives.ToString();
	}

	// EventManager listeners
	private void UpdatePlayerHealthEvent(object healthComponent, CustomEventArgs args)
	{
		int changedHealthAmount = args.Amount;
		for(int i = 0; i < playerList.Count; i++)
		{
			if (playerList[i].gameObject != null && playerList[i].gameObject == args.EventObject)
			{
				playerHealthUIList[i].text = changedHealthAmount.ToString();
			}
		}
	}
}
