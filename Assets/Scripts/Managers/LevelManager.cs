using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private List<Player> playerList;

	private bool isTwoPlayer = false;
	private bool roundRunning = false;
	private int numLives;
	private GameStartData gameSettings;

	// Start is called before the first frame update
	private void Awake()
	{

	}

	private void OnEnable()
	{
		playerList = new List<Player>();

		EventManager.Subscribe("healthObjectDied", new EventHandler<CustomEventArgs>(HandlePlayerDeath));
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
		if (!isTwoPlayer)
		{
			// add for one player mode
			Player temp = Instantiate(Resources.Load<Player>("Prefabs/Player1"));
			Health playerHealth = temp.GetComponent<Health>();
			playerHealth.CurrentHealth = gameSettings.MaxHP;
		} else
		{
			playerList.Add(Instantiate(Resources.Load<Player>("Prefabs/Player1")));
			playerList.Add(Instantiate(Resources.Load<Player>("Prefabs/Player1")));
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
		// Process death of a Player object, change game state, etc
		Destroy(args.EventObject);
	}
}
