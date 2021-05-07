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
		Player playerComponent = playerThatDied.GetComponent<Player>();

		if (bombComponent.triggeringPlayer != playerThatDied)
		{
			playerStatsList[bombComponent.triggeringPlayer.GetComponent<Player>().playerListIndex].kills++;
		}

		playerStatsList[playerComponent.playerListIndex].deaths++;
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
