using System;
using System.Collections;
using UnityEngine;
using Mirror;

public class PlayerInventory : NetworkBehaviour
{
    [Tooltip("Controls the order of bombs as shown in the player HUD")]
    [SerializeField] private char[] INVEN_BOMB_TYPES = { 'r', 'p', 'y', 'g' };

    [Tooltip("Controls the max amount of bombs a player can carry for each bomb type")]
    [SerializeField] private int[] INVEN_MAX_VALUES = { 5, 5, 5, 5 };

    // A list of ints corresponding to how many of INVEN_BOMB_TYPES the player is carrying, initialized to zero
    public readonly SyncList<int> inventoryList = new SyncList<int>(new int[] { 0, 0, 0, 0 });

    // The currently selected slot of the inventory (i.e. the bomb slot to be placed next)
    [SyncVar(hook=nameof(OnSelectedSlotChange))] public int selectedSlot = 0;

    // Singleton
    private GameUIManager gameUIManager;

    public override void OnStartClient()
    {
        gameUIManager = GameUIManager.Singleton;
        if (gameUIManager == null) Debug.LogError("Cannot find Singleton: GameUIManager");

        // Subscribe to the synclist hook
        inventoryList.Callback += OnInventoryChange;
    }

    // Adds "amt" number of bombs of type "type" to the players inventory
    [Server] public void AddInventoryBomb(char type, int amt)
    {
        // Get the index corresponding to the bomb type
        int idx = Array.IndexOf(INVEN_BOMB_TYPES, type);

        // If the amount we want to add is larger than max values, then set the bomb quantity to max
        if (inventoryList[idx] + amt > INVEN_MAX_VALUES[idx])
        {
            inventoryList[idx] = INVEN_MAX_VALUES[idx];

            this.GetComponent<PlayerInterface>().DisplayInventoryAdd(idx, INVEN_MAX_VALUES[idx]);

        }
        else
        {
            // Otherwise add the amount
            inventoryList[idx] += amt;

            this.GetComponent<PlayerInterface>().DisplayInventoryAdd(idx, amt);
        }
    }

	public char[] GetBombTypes()
	{
		return INVEN_BOMB_TYPES;
	}

    // Removes a bomb type from the inventory
    [Server] public void RemoveInventoryBomb(char type)
    {
        // Get the index corresponding to the bomb type
        int idx = Array.IndexOf(INVEN_BOMB_TYPES, type);

        // Check if it can be removed
        if (inventoryList[idx] > 0)
        {
            // Decrement the bomb quantity by one
            inventoryList[idx]--;
        } else
        {
            // This function should not be called if bomb type has zero quantity
            Debug.LogError("Error: RemoveInventoryBomb called when bomb type has 0 quantity");
        }
    }

    // Resets all bomb quantity back to zero
    [Server] public void ResetInventory()
    {
        for (int i = 0; i < inventoryList.Count; i++)
        {
            inventoryList[i] = 0;
        }
    }

    // Changes the selected slot to the next available slot
    // If no slots are available, does not move slot
    [Server] public void RotateSelectedSlot()
    {
        // Start at current slot
        int nextSlot = selectedSlot;

        // Set it to the next available slot
        for (int i = 1; i <= inventoryList.Count; i++)
        {
            // Set to next slot
            nextSlot = selectedSlot + i;

            // If past the last slot rotate back to the beginning
            if (nextSlot >= inventoryList.Count) nextSlot = nextSlot - inventoryList.Count;

            // If this slot has bombs, leave loop
            if (inventoryList[nextSlot] > 0) break;
        }

        // Increment selected slot, if at the last slot rotate back to the beginning
        if (nextSlot >= INVEN_BOMB_TYPES.Length) SwitchToSlot(0);
        else SwitchToSlot(nextSlot);
    }

	// Changes the selected swap to the hotkey button pressed
	[Server] public void SwitchToSlot(int index)
	{
		selectedSlot = index;
	}

    [Client] private void OnSelectedSlotChange(int oldSlot, int newSlot)
    {
        // Update the player interface when selected slot changes
        this.GetComponent<PlayerInterface>().UpdateInventorySelected();

        if (isLocalPlayer) gameUIManager.ClientOnInventorySelectChanged(INVEN_BOMB_TYPES[newSlot], inventoryList[newSlot]);
    }

    [Client] private void OnInventoryChange(SyncList<int>.Operation op, int idx, int oldAmt, int newAmt)
    {
        // Update the player interface everytime the inventory changes
        this.GetComponent<PlayerInterface>().UpdateInventoryQuantity();

        if (idx == selectedSlot && isLocalPlayer)
        {
            gameUIManager.ClientOnInventorySelectChanged(INVEN_BOMB_TYPES[idx], inventoryList[idx]);
        }
    }

    // Returns the bomb type corresponding to the currently selected bomb
    // returns 'e' if the bomb slot is empty
    public char GetSelectedBombType()
    {
        if (inventoryList[selectedSlot] <= 0) return 'e';

        return INVEN_BOMB_TYPES[selectedSlot];
    }
}
