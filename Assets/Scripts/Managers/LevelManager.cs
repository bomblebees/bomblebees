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

	private Vector3 player1SpawnPos = new Vector3(85, 5, -15);
	private Vector3 player2SpawnPos = new Vector3(184, 5, 46);
	private float invulnTime = 2.0f;

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
			Player temp = Instantiate(Resources.Load<Player>("Prefabs/Player1"), player1SpawnPos, new Quaternion());
			temp.lives = numLives;
			Health playerHealth = temp.GetComponent<Health>();
			playerHealth.CurrentHealth = gameSettings.MaxHP;
			PlayerList.Add(temp);
		}
		else if (gameSettings.NumPlayers == 2)
		{
			Player temp = Instantiate(Resources.Load<Player>("Prefabs/Player1"), player1SpawnPos, new Quaternion());
			temp.lives = numLives;
			Health playerHealth = temp.GetComponent<Health>();
			playerHealth.CurrentHealth = gameSettings.MaxHP;
			PlayerList.Add(temp);

			Player temp2 = Instantiate(Resources.Load<Player>("Prefabs/Player2"), player2SpawnPos, new Quaternion());
			temp2.lives = numLives;
			Health playerHealth2 = temp2.GetComponent<Health>();
			playerHealth2.CurrentHealth = gameSettings.MaxHP;
			PlayerList.Add(temp2);
		}
	}

	void GameStart(object uiManager, CustomEventArgs args)
	{
		// later we can add some kind of coroutine for a round start countdown or something
		// but the round start functionality for scene gets defined here
		gameSettings = args.DataObject;
		numLives = gameSettings.NumLives;

		roundRunning = true;
		
		AddPlayers();
	}

	void HandlePlayerDeath(object player, CustomEventArgs args)
	{
		Player playerComponent = args.EventObject.GetComponent<Player>();
		Health healthComponent = args.EventObject.GetComponent<Health>();
		// int indexInList;
		for (int i = 0; i < PlayerList.Count; i++)
		{
			if (PlayerList[i].gameObject != null && PlayerList[i].gameObject == args.EventObject)
			{
				//indexInList = i;
				if (playerComponent.lives > 1)
				{
					// player has one or more lives left, do respawn thing (we can standardize "resetting" a new life in a new method later)
					// EventManager.TriggerEvent("playerHealthChanged", this, new CustomEventArgs { Amount = gameSettings.MaxHP, EventObject = args.EventObject });

					// right now, we set the player state to invulnerable and set it to false using coroutine upon dying
					// later, we would probably use a proper finite state machine for this sorts of stuff
					healthComponent.CurrentHealth = gameSettings.MaxHP;
					healthComponent.Invulnerable = true;
					playerComponent.lives--;

					args.EventObject.GetComponentInChildren<Renderer>().material.SetFloat("_isBlinking", 1f);
					StartCoroutine(InvulnUntilEnd(invulnTime, healthComponent));
					
					UIManager.Instance.PlayerDeathUIChange(playerComponent);

					if (i == 0)
					{
						args.EventObject.transform.position = player1SpawnPos;
					}
					else if (i == 1)
					{
						args.EventObject.transform.position = player2SpawnPos;
					}
				}
				else
				{
					// player has no lives left, do end game thing, process wins match results and stuff
					Debug.Log("out of lives, ending match");
					playerComponent.lives--;
					UIManager.Instance.PlayerDeathUIChange(playerComponent);

					args.EventObject.SetActive(false);
					roundRunning = false;
				}
			}
		}
		// Process death of a Player object, change game state, subtract from lives, etc
	}

	private IEnumerator InvulnUntilEnd(float timer, Health health)
	{
		Debug.Log("timer started for invuln");
		yield return new WaitForSeconds(timer);
		Debug.Log("timer ended.");
		health.Invulnerable = false;
		health.gameObject.GetComponentInChildren<Renderer>().material.SetFloat("_isBlinking", 0f);
	}
}
