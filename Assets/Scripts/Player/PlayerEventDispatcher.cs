using System.Collections;
using UnityEngine;
using Mirror;


/// <summary>
/// Handles outbound event communication from this player to other game objects
/// </summary>
public class PlayerEventDispatcher : NetworkBehaviour
{
	private GameUIManager gameUIManager;
	private LobbySettings lobbySettings;

	[Client]
	private void InitSingletons()
	{
		gameUIManager = GameUIManager.Singleton;
		if (gameUIManager == null) Debug.LogError("Cannot find Singleton: LivesUI");

		lobbySettings = FindObjectOfType<LobbySettings>();
		if (lobbySettings == null) Debug.LogError("Cannot find Singleton: LobbySettings");
	}

	[Client]
	public override void OnStartClient()
	{
		InitSingletons();

		gameUIManager.livesUI.EnableLivesUI(this.GetComponent<Player>());
		gameUIManager.livesUI.UpdateLives(this.GetComponent<Health>().currentLives, this.GetComponent<Player>());
	}

	[Client]
	public void OnChangeLives(int prevLives, int newLives)
	{
		gameUIManager.OnChangeLives(prevLives, newLives, this.gameObject);
	}

	[Client]
	public void OnChangeSpinHeld(bool prevHeld, bool newHeld)
	{
		// if spin was held, then released (i.e. spin was launched)
		if (prevHeld && !newHeld) gameUIManager.OnSpin(prevHeld, newHeld, this.gameObject);
	}

	[Client]
	public void OnInventorySelectChange(int newSlot, char type, int quantity)
	{

		gameUIManager.OnInventorySelectChanged(type, quantity);
	}

	[Client]
	public void OnInventoryQuantityChange(int slot, char type, int quantity)
	{
		if (slot == this.GetComponent<PlayerInventory>().selectedSlot)
		{
			gameUIManager.OnInventorySelectChanged(type, quantity);
		}
	}

	[Client]
	public void OnChangeKills(int prevKills, int newKills)
	{
		gameUIManager.OnChangeKills(prevKills, newKills, this.gameObject);
	}

	[Client]
	public void OnChangeHeldKey(char oldKey, char newKey)
	{
		gameUIManager.OnSwap(oldKey, newKey, this.gameObject);
	}

	[Client]
	public void OnChangeCombos(int prevCombos, int newCombos)
	{
		gameUIManager.OnChangeCombos(prevCombos, newCombos, this.gameObject);
	}

	[Client]
	public void OnChangeDeaths(int prevDeaths, int newDeaths)
	{

	}
}