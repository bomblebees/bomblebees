using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerStatTracker : NetworkBehaviour
{

	public class PlayerStats
	{
		public GameObject player;

		public int kills = 0;
		public int deaths = 0;
		public int assists = 0;

		public PlayerStats(GameObject playerObject)
		{
			player = playerObject;
		}
	}

	public List<PlayerStats> playerStatsList = new List<PlayerStats>();

	private RoundManager roundManager;
	private EventManager eventManager;

	private void InitSingletons()
	{
		roundManager = RoundManager.Singleton;
		if (roundManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

		eventManager = EventManager.Singleton;
		if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");

	}

	public override void OnStartServer()
	{
		InitSingletons();
		Debug.Log("Starting PlayerStatTracker on server");
		eventManager.EventStartRound += InitializePlayerStatsList;
		eventManager.EventPlayerTookDamage += PlayerDeathUpdate;
	}



	public void CreatePlayerStatObject(GameObject player)
	{
		playerStatsList.Add(new PlayerStats(player));
	}

	private void InitializePlayerStatsList()
	{
		// initialize the stats list with player indices after round start to ensure roundmanager's player list is already populated
	}

	[Server]
	public void PlayerDeathUpdate(int _, GameObject bomb, GameObject playerThatDied)
	{
		ComboObject bombComponent = bomb.GetComponent<ComboObject>();

		int killerIndex = -1;
		int deadPlayerIndex = -1;

		// Iterate through player stats list and find both the player that got kill and player that got hit
		for (int i = 0; i < playerStatsList.Count; i++)
		{
			if (ReferenceEquals(bombComponent.triggeringPlayer, playerStatsList[i].player))
			{
				killerIndex = i;
			}
			
			if (ReferenceEquals(playerThatDied, playerStatsList[i].player))
			{
				deadPlayerIndex = i;
			}
		}

		// If not self-destruct, give kill credit to the bomb's latest triggerer/pusher
		if (bombComponent.triggeringPlayer != playerThatDied)
		{
			if (killerIndex != -1)
			{
				playerStatsList[killerIndex].kills++;
			}
			else
			{
				Debug.Log("Killer player not found in stats list");
			}
		}

		// Add a death to the player that got hit
		if (deadPlayerIndex != -1)
		{
			playerStatsList[deadPlayerIndex].deaths++;
		}
		else
		{
			Debug.Log("Player that died not found in stats list");
		}
	}

	public string PrintStats(GameObject player)
	{
		string toPrint = "";

		if (!player.GetComponent<Player>())
		{
			return "Player component not found, cannot print stats";
		}

		for (int i = 0; i < playerStatsList.Count; i++)
		{
			// is this a reliable method of checking if the player inputted is the same as player stored in playerStatsList??
			if (GameObject.ReferenceEquals(player, playerStatsList[i].player))
			{
				toPrint = player.GetComponent<Player>().steamName + " - Kills: " + playerStatsList[i].kills + ", Deaths: " + playerStatsList[i].deaths + ", Assists: " + playerStatsList[i].assists;
			}
		}
		// toPrint = "Kills: " + playerStatsList[index].kills + ", Deaths: " + playerStatsList[index].deaths + ", Assists: " + playerStatsList[index].assists;

		return toPrint;
	}
}
