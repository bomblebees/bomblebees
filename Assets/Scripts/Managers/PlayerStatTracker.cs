using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerStatTracker : NetworkBehaviour
{


	[SyncVar] public GameObject player;

	[SyncVar] public int kills = 0;
	[SyncVar] public int deaths = 0;
	[SyncVar] public int assists = 0;

	// placement from 1st to 4th at end game so stats screen can display in order
	[SyncVar] public int placement = -1;

	[SyncVar] public int doubleKills = 0;
	[SyncVar] public int tripleKills = 0;
	// temp - what happens when u get a multikill greater than 3? (theoretically impossible)
	[SyncVar] public int superKill = 0;

	// number of bombs used
	[SyncVar] public int comboObjectsDropped = 0;

	// number of self destructs
	[SyncVar] public int selfDestructs = 0;

	// number of combos made
	[SyncVar] public int totalCombosMade = 0;

	// individual combo objects made from combos
	[SyncVar] public int bombsMade = 0;
	[SyncVar] public int sludgesMade = 0;
	[SyncVar] public int lasersMade = 0;
	[SyncVar] public int plasmasMade = 0;

	// the time the player was eliminated
	[SyncVar] public double timeOfElimination = 0f;


	[SerializeField] public GameObject PlayerStatsUIElementPrefab;

	private EventManager eventManager;

	private void InitSingletons()
	{
		eventManager = EventManager.Singleton;
		if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");

	}

	public override void OnStartServer()
	{
		InitSingletons();
		Debug.Log("Starting PlayerStatTracker on server");

		// functions are subscribed on server, playerstats syncvar will always update from server
		eventManager.EventPlayerTookDamage += PlayerDeathUpdate;
		eventManager.EventPlayerSwap += PlayerSwapUpdate;
		eventManager.EventPlayerEliminated += PlayerEliminatedUpdate;

		player = gameObject;
	}

	public override void OnStartClient()
	{
		InitSingletons();
	}

	[Server]
	public void PlayerEliminatedUpdate(double timeOfElim, GameObject player)
	{
		// if this was not the player that was eliminated, return
		if (!ReferenceEquals(player, gameObject)) return;

		// set the time of elim
		timeOfElimination = timeOfElim;
	}

	[Server]
	public void PlayerDeathUpdate(int _, GameObject bomb, GameObject playerThatDied)
	{
		ComboObject bombComponent = bomb.GetComponent<ComboObject>();

		if (ReferenceEquals(playerThatDied, gameObject))
		{
			// the player that died in the event is this player
			deaths++;
		}

		if (ReferenceEquals(bombComponent.triggeringPlayer, gameObject) && !ReferenceEquals(gameObject, playerThatDied))
		{
			// if this player was owner of the bomb that killed, and also wasn't the one who died, then award kill to this player
			kills++;
		}
	}

	[Server]
	public void PlayerSwapUpdate(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
	{
		// if the player that made combo isn't this player, then return
		if (!ReferenceEquals(player, gameObject))
		{
			return;
		}
		else
		{
			// if 1 or more bombs awarded, add to respective counter
			if (numBombsAwarded > 0)
			{
				totalCombosMade++;
				// this shit will be a problem when we rework colors unless we can go through the codebase and change all the hardcoded color values
				switch (oldKey)
				{
					// award the number of bombs made for respective combo:
					case 'r':
						bombsMade += numBombsAwarded;
						break;
					case 'p':
						sludgesMade += numBombsAwarded;
						break;
					case 'y':
						lasersMade += numBombsAwarded;
						break;
					case 'g':
						plasmasMade += numBombsAwarded;
						break;
					default:
						Debug.Log("Cannot award bomb type to total bomb stat counter, bomb type not found");
						break;
				}
			}
		}
	}


	
	#region UI/Display
	// ideally stat tracker script purely tracks stats and we leave UI stuff up to a different script

	// populate stat block with the necessary stats
	
	public void CreateStatsUIElement(GameObject anchorObject)
	{
		GameObject obj = Instantiate(
			PlayerStatsUIElementPrefab,
			new Vector3(0, 0, 0),
			Quaternion.identity,
			anchorObject.transform
		);

		obj.transform.localPosition = new Vector3(0, 0, 0);

		PlayerStatsUIElement uiElement = obj.GetComponent<PlayerStatsUIElement>();

		uiElement.avatar.sprite = uiElement.GetComponent<CharacterHelper>().GetCharImage(gameObject.GetComponent<Player>().characterCode);
		uiElement.playerName.text = gameObject.GetComponent<Player>().steamName;

		uiElement.killsText.text = kills.ToString();
		uiElement.deathsText.text = deaths.ToString();
		uiElement.combosMadeText.text = totalCombosMade.ToString();

	}
	

	#endregion

	#region Helpers

	public void PrintStats()
	{
		string toPrint = "";
		toPrint =  gameObject.GetComponent<Player>().steamName + "\n" +
			"Placement: " + placement + "\n" +
			"Kills: " + kills + "\n" +
			"Deaths: " + deaths + "\n" +
			"Assists: " + assists + "\n" +
			"Total Combos Made: " + totalCombosMade + "\n" +
			"Total Bombs Created: " + bombsMade + "\n" +
			"Total Sludges Created: " + sludgesMade + "\n" +
			"Total Lasers Created: " + lasersMade + "\n" +
			"Total Plasmas Created: " + plasmasMade + "\n";
		Debug.Log(toPrint);
	}

	#endregion
}
