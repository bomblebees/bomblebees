using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public static LevelManager Instance { get; private set; }

	public List<Player> PlayerList { get; private set; }

	private bool isTwoPlayer = false;
	private bool roundRunning = false;
	private int numLives;
	private GameStartData gameSettings;

	// Start is called before the first frame update
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	private void OnEnable()
	{
		PlayerList = new List<Player>();

		EventManager.Subscribe("playerDied", new EventHandler<CustomEventArgs>(HandlePlayerDeath));
		EventManager.Subscribe("gameStart", new EventHandler<CustomEventArgs>(GameStart));
	}


	void Start()
    {
        
    }
	
    // Update is called once per frame
    void Update()
    {
        
    }

	void AddPlayers()
	{
		if (gameSettings.NumPlayers == 1)
		{
			// add for one player mode
			Player temp = Instantiate(Resources.Load<Player>("Prefabs/Player1"));
			Health playerHealth = temp.GetComponent<Health>();
			playerHealth.CurrentHealth = gameSettings.MaxHP;
			PlayerList.Add(temp);
		}
		else if (gameSettings.NumPlayers == 2)
		{
			Player temp = Instantiate(Resources.Load<Player>("Prefabs/Player1"));
			Health playerHealth = temp.GetComponent<Health>();
			playerHealth.CurrentHealth = gameSettings.MaxHP;
			PlayerList.Add(temp);

			Player temp2 = Instantiate(Resources.Load<Player>("Prefabs/Player2"));
			Health playerHealth2 = temp2.GetComponent<Health>();
			playerHealth2.CurrentHealth = gameSettings.MaxHP;
			PlayerList.Add(temp2);
		}
	}

	void GameStart(object uiManager, CustomEventArgs args)
	{
		// later we can add some kind of coroutine for a round start countdown or something
		// but the round start functionality for scene gets defined here
		roundRunning = true;
		gameSettings = args.DataObject;
		AddPlayers();
	}

	void HandlePlayerDeath(object player, CustomEventArgs args)
	{
		// Process death of a Player object, change game state, subtract from lives, etc
		// Destroy(args.EventObject);
		args.EventObject.SetActive(false);
	}
}
