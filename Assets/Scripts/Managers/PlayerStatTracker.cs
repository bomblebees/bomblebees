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

		// placement from 1st to 4th at end game so stats screen can display in order
		public int placement = -1;

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

	[SerializeField] public GameObject PlayerStatsUIElementPrefab;
	[SerializeField] public int StatUIBlockSpacing = 112; // not too robust, maybe use array of preset anchors like in LivesUI later?

	public List<PlayerStats> playerStatsList = new List<PlayerStats>();
	public List<PlayerStats> playerStatsOrderedByElimination = new List<PlayerStats>();

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

		// if the KO'd player has less than 1 life, they are eliminated; add to ordered player list at index 0.

		// Number of lives gets set before the event is called, so we should already be set to do the check here
		
		if (playerThatDied.GetComponent<Health>().currentLives < 1)
		{
			// Get total number of players in the lobby
			Debug.Log(playerStatsOrderedByElimination.Count);
			Debug.Log("num players in lobby: " + roundManager.playerList.Count);
			Debug.Log("dead player in stat tracker: " + playerThatDied);
			int lobbyNumPlayers = roundManager.playerList.Count;
			

			if (!(lobbyNumPlayers <= 1))
			{
				playerStatsList[deadPlayerIndex].placement = lobbyNumPlayers - playerStatsOrderedByElimination.Count;
				playerStatsOrderedByElimination.Insert(0, playerStatsList[deadPlayerIndex]);
			}

			/*
			// if that was the second to last player, only one remains so add them to ordered list
			if (roundManager.alivePlayers.Count <= 1)
			{
				// this is so fucked lol
				// to get the last player's PlayerStat object, find their position in the player list by using the round manager's alivePlayers list
				PlayerStats winningPlayerStat = playerStatsList[getPlayerIndexInList(roundManager.alivePlayers[0].player.gameObject)];

				winningPlayerStat.placement = 1;

				playerStatsOrderedByElimination.Insert(0, winningPlayerStat);
			}
			*/
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


	#region UI/Display
	// ideally stat tracker script purely tracks stats and we leave UI stuff up to a different script

	// populate stat block with the necessary stats
	public void CreateStatsUIElement(PlayerStats playerStat)
	{
		for (int i = 0; i < playerStatsOrderedByElimination.Count; i++)
		{
			Player playerObject = playerStatsOrderedByElimination[i].player.GetComponent<Player>();

			GameObject obj = Instantiate(
				PlayerStatsUIElementPrefab,
				new Vector3(0, 0, 0),
				Quaternion.identity,
				roundManager.statsElementUIAnchorObject.transform);

			obj.transform.position += new Vector3(0, i * StatUIBlockSpacing, 0);

			PlayerStatsUIElement uiElement = obj.GetComponent<PlayerStatsUIElement>();

			uiElement.avatar.sprite = uiElement.GetComponent<CharacterHelper>().GetCharImage(playerObject.characterCode);
			uiElement.playerName.text = playerObject.steamName;

			uiElement.killsText.text = playerStat.kills.ToString();
			uiElement.deathsText.text = playerStat.deaths.ToString();
			uiElement.combosMadeText.text = playerStat.totalCombosMade.ToString();
		}
	}

	#endregion

	#region Helpers

	public int getPlayerIndexInList(GameObject player)
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

	public void PrintStats()
	{
		

		// int playerToPrint = getPlayerIndexInList(player);

		for (int i = 0; i < playerStatsOrderedByElimination.Count; i++)
		{
			string toPrint = "";
			toPrint = playerStatsOrderedByElimination[i].player.GetComponent<Player>().steamName + "\n" +
				"Placement: " + playerStatsOrderedByElimination[i].placement + "\n" +
				"Kills: " + playerStatsOrderedByElimination[i].kills + "\n" +
				"Deaths: " + playerStatsOrderedByElimination[i].deaths + "\n" +
				"Assists: " + playerStatsOrderedByElimination[i].assists + "\n" +
				"Total Combos Made: " + playerStatsOrderedByElimination[i].totalCombosMade + "\n" +
				"Total Bombs Created: " + playerStatsOrderedByElimination[i].bombsMade + "\n" +
				"Total Sludges Created: " + playerStatsOrderedByElimination[i].sludgesMade + "\n" +
				"Total Lasers Created: " + playerStatsOrderedByElimination[i].lasersMade + "\n" +
				"Total Plasmas Created: " + playerStatsOrderedByElimination[i].plasmasMade;
			Debug.Log(toPrint);
		}
		/*
		toPrint = playerStatsOrderedByElimination[placement].player.GetComponent<Player>().steamName +"\n" +
			"Placement: " + playerStatsOrderedByElimination[placement].placement + "\n" +
			"Kills: " + playerStatsOrderedByElimination[placement].kills + "\n" +
			"Deaths: " + playerStatsOrderedByElimination[placement].deaths + "\n" +
			"Assists: " + playerStatsOrderedByElimination[placement].assists + "\n" +
			"Total Combos Made: " + playerStatsOrderedByElimination[placement].totalCombosMade + "\n" +
			"Total Bombs Created: " + playerStatsOrderedByElimination[placement].bombsMade + "\n" +
			"Total Sludges Created: " + playerStatsOrderedByElimination[placement].sludgesMade + "\n" +
			"Total Lasers Created: " + playerStatsOrderedByElimination[placement].lasersMade + "\n" +
			"Total Plasmas Created: " + playerStatsOrderedByElimination[placement].plasmasMade;
			
		 */

		// toPrint = "Kills: " + playerStatsList[index].kills + ", Deaths: " + playerStatsList[index].deaths + ", Assists: " + playerStatsList[index].assists;

		// return toPrint;
	}

	#endregion
}
