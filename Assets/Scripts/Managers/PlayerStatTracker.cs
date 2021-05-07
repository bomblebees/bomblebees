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

		public int doubleKills = 0;
		public int tripleKills = 0;
		// temp - what happens when u get a multikill greater than 3? (theoretically impossible)
		public int superKill = 0;

		// number of bombs used
		public int comboObjectsDropped = 0;

		// number of self destructs
		public int selfDestructs = 0;

		// number of combos made
		public int totalCombosMade = 0;

		// individual combo objects made from combos
		public int bombsMade = 0;
		public int sludgesMade = 0;
		public int lasersMade = 0;
		public int plasmasMade = 0;

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
		eventManager.EventPlayerSwap += PlayerSwapUpdate;
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

	[Server]
	public void PlayerSwapUpdate(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
	{
		int playerIndex = getPlayerIndexInList(player);

		// if a combo was made, add to total combo counter
		if (combo)
		{
			playerStatsList[playerIndex].totalCombosMade++;
		}

		// if 1 or more bombs awarded, add to respective counter
		if (numBombsAwarded > 0)
		{
			// this shit will be a problem when we rework colors unless we can go through the codebase and change all the hardcoded color values
			switch (oldKey)
			{
				// award the number of bombs made for respective combo:
				case 'r':
					playerStatsList[playerIndex].bombsMade += numBombsAwarded;
					break;
				case 'p':
					playerStatsList[playerIndex].sludgesMade += numBombsAwarded;
					break;
				case 'y':
					playerStatsList[playerIndex].lasersMade += numBombsAwarded;
					break;
				case 'g':
					playerStatsList[playerIndex].plasmasMade += numBombsAwarded;
					break;
				default:
					Debug.Log("Cannot award bomb type to total bomb stat counter, bomb type not found");
					break;
			}
		}
	}

	#region Helpers

	private int getPlayerIndexInList(GameObject player)
	{
		int playerIndex = -1;
		for (int i = 0; i < playerStatsList.Count; i++)
		{
			if (ReferenceEquals(player, playerStatsList[i].player))
			{
				playerIndex = i;
			}
		}
		if (playerIndex == -1)
		{
			Debug.Log("Player not found in stat tracker list.");
			return playerIndex;
		}

		return playerIndex;
	}

	public string PrintStats(GameObject player)
	{
		string toPrint = "";

		if (!player.GetComponent<Player>())
		{
			return "Player component not found, cannot print stats";
		}

		int playerToPrint = getPlayerIndexInList(player);

		toPrint = player.GetComponent<Player>().steamName +"\n" +
			"Kills: " + playerStatsList[playerToPrint].kills + "\n" +
			"Deaths: " + playerStatsList[playerToPrint].deaths + "\n" +
			"Assists: " + playerStatsList[playerToPrint].assists + "\n" +
			"Total Combos Made: " + playerStatsList[playerToPrint].totalCombosMade + "\n" +
			"Total Bombs Created: " + playerStatsList[playerToPrint].bombsMade + "\n" +
			"Total Sludges Created: " + playerStatsList[playerToPrint].sludgesMade + "\n" +
			"Total Lasers Created: " + playerStatsList[playerToPrint].lasersMade + "\n" +
			"Total Plasmas Created: " + playerStatsList[playerToPrint].plasmasMade;


		// toPrint = "Kills: " + playerStatsList[index].kills + ", Deaths: " + playerStatsList[index].deaths + ", Assists: " + playerStatsList[index].assists;

		return toPrint;
	}

	#endregion
}
