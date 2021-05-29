using System;
using System.Linq;
using Mirror;
using UnityEngine;

public class PlayerStatTracker : NetworkBehaviour
{


	[SyncVar] public GameObject player;

	[SyncVar(hook = nameof(OnChangeKills))] public int kills = 0;
	[SyncVar(hook = nameof(OnChangeDeaths))] public int deaths = 0;
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

	// number of bomb's made from combos
	[SyncVar(hook = nameof(OnChangeBombCombos))] public int totalPoints = 0;
	[SyncVar] public int totalBombCombosMade = 0;

	// individual combo objects made from combos
	[SyncVar] public int bombsMade = 0;
	[SyncVar] public int sludgesMade = 0;
	[SyncVar] public int lasersMade = 0;
	[SyncVar] public int plasmasMade = 0;

	// the time the player was eliminated
	[SyncVar] public double timeOfElimination = 0f;


	[SerializeField] public GameObject PlayerStatsUIElementPrefab;

	private EventManager eventManager;
	private RoundManager roundManager;
	private LobbySettings lobbySettings;
	private PlayerEventDispatcher dispatcher;

	private void InitSingletons()
	{
		eventManager = EventManager.Singleton;
		if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");

		roundManager = RoundManager.Singleton;
		if (roundManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

		lobbySettings = FindObjectOfType<LobbySettings>();
		if (lobbySettings == null) Debug.LogError("Cannot find Singleton: LobbySettings");
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

		dispatcher = GetComponent<PlayerEventDispatcher>();
		if (dispatcher == null) Debug.LogError("Cannot find component: ClientPlayerDispatcher");
	}

	[Server]
	public void PlayerEliminatedUpdate(double timeOfElim, GameObject player)
	{
		if (roundManager.roundOver) return; // if round over, do not update

		// if this was not the player that was eliminated, return
		if (!ReferenceEquals(player, gameObject)) return;

		// set the time of elim
		timeOfElimination = timeOfElim;
	}

	[Server]
	public void PlayerDeathUpdate(int _, GameObject bomb, GameObject playerThatDied)
	{
		if (roundManager.roundOver) return; // if round over, do not update

		ComboObject bombComponent = bomb.GetComponent<ComboObject>();

		if (ReferenceEquals(playerThatDied, gameObject))
		{
			// the player that died in the event is this player
			deaths++;

			// if combo gamemode and died, apply combo penalty
			if (lobbySettings.GetGamemode() is ComboGamemode)
            {
				if (totalPoints > ComboGamemode.comboPenalty)
					totalPoints -= ComboGamemode.comboPenalty;
				else
					totalPoints = 0;
            }
		}

		if (ReferenceEquals(bombComponent.triggeringPlayer, gameObject) && !ReferenceEquals(gameObject, playerThatDied))
		{
			// if this player was owner of the bomb that killed, and also wasn't the one who died, then award kill to this player
			kills++;

			// if combo gamemode and died, apply combo bonus
			if (lobbySettings.GetGamemode() is ComboGamemode)
			{
				totalPoints += ComboGamemode.comboBonus;
			}
		}
	}

	[Server]
	public void PlayerSwapUpdate(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
	{
		if (roundManager.roundOver) return; // if round over, do not update

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
				totalBombCombosMade += numBombsAwarded;
				totalPoints += numBombsAwarded;

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


	[ClientCallback]
	public void OnChangeKills(int prevKills, int newKills)
    {
		dispatcher.OnChangeKills(prevKills, newKills);
    }

	[ClientCallback]
	public void OnChangeDeaths(int prevDeaths, int newDeaths)
	{
        dispatcher.OnChangeDeaths(prevDeaths, newDeaths);
	}

	[ClientCallback]
	public void OnChangeBombCombos(int prevCombos, int newCombos)
	{
		dispatcher.OnChangeCombos(prevCombos, newCombos);
	}


	#region UI/Display
	// ideally stat tracker script purely tracks stats and we leave UI stuff up to a different script

	// populate stat block with the necessary stats
	
	private int _comboReward;
	private int _killReward;
	private int _deathPenalty;
	
	private void GetPointSystemData()
	{
		_comboReward = 1;
		_killReward = ComboGamemode.comboBonus;
	}

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

		GetPointSystemData();
		
		// Total death penalty math
		var totalDeathPenalty = Math.Abs(kills * _killReward) + Math.Abs(totalBombCombosMade * _comboReward) - totalPoints;
		totalDeathPenalty = Math.Abs(totalDeathPenalty);
		
		uiElement.totalPointsText.text = $"{totalPoints}";
		uiElement.killsText.text = $"{kills} (+{Math.Abs(kills * _killReward)})";
		uiElement.deathsText.text = $"{deaths} (-{totalDeathPenalty})";
		uiElement.bombComboMadeText.text = $"{totalBombCombosMade} (+{Math.Abs(totalBombCombosMade * _comboReward)})";

		uiElement.totalPoints = totalPoints;
		uiElement.kills = kills;
		uiElement.totalDeathPenalty = totalDeathPenalty;
		uiElement.totalBombCombosMade = totalBombCombosMade;
		
		CmdHighlightStats();
	}

	[Command]
	private void CmdHighlightStats()
	{
		RpcHighlightStats();
	}

	[ClientRpc]
	private void RpcHighlightStats()
	{
		var playerStatsUIElements = FindObjectsOfType<PlayerStatsUIElement>();
		var totalPointsArray = new int[playerStatsUIElements.Length];
		var killsArray = new int[playerStatsUIElements.Length];
		var deathsArray = new int[playerStatsUIElements.Length];
		var bombCombosMadeArray = new int[playerStatsUIElements.Length];

		for (var i = 0; i < playerStatsUIElements.Length; i++)
		{
			totalPointsArray[i] = playerStatsUIElements[i].totalPoints;
			killsArray[i] = playerStatsUIElements[i].kills;
			deathsArray[i] = playerStatsUIElements[i].totalDeathPenalty;
			bombCombosMadeArray[i] = playerStatsUIElements[i].totalBombCombosMade;
		}

		// Highlight colors. Source: https://www.colorhexa.com/color-names
		var pastelGreen = new Color(0.47f, 0.87f, 0.47f);
		var pastelRed = new Color(1f, 0.41f, 0.38f);
		
		// Set highlights
		for (var i = 0; i < playerStatsUIElements.Length; i++)
		{
			if (totalPointsArray[i].Equals(totalPointsArray.Max()) && !totalPointsArray[i].Equals(0))
			{
				// Most total points
				playerStatsUIElements[i].totalPointsText.color = pastelGreen;
			}

			if (killsArray[i].Equals(killsArray.Max()) && !killsArray[i].Equals(0))
			{
				// Most kills
				playerStatsUIElements[i].killsText.color = pastelGreen;
			}
			
			if (deathsArray[i].Equals(deathsArray.Max()) && !deathsArray[i].Equals(0))
			{
				// Most deaths
				playerStatsUIElements[i].deathsText.color = pastelRed;
			}
			
			if (bombCombosMadeArray[i].Equals(bombCombosMadeArray.Max()) && !bombCombosMadeArray[i].Equals(0))
			{
				// Most combos
				playerStatsUIElements[i].bombComboMadeText.color = pastelGreen;
			}
		}
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
