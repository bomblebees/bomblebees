using System;
using System.Collections;
using UnityEngine;
using Mirror;

public class PlayerInventory : NetworkBehaviour
{
    [Tooltip("Controls the order of bombs as shown in the player HUD")]
    [SerializeField] public static char[] INVEN_BOMB_TYPES = { 'r', 'p', 'y', 'g' };

    [Tooltip("Controls the max and starting amount of bombs a player can carry for each bomb type")]
    [SerializeField] public static int[] INVEN_MAX_VALUES = { 5, 5, 5, 5 };
	[SerializeField] public static int INVEN_START_VALUE = 3;

	[Tooltip("Ground item prefabs for spawning bombs on ground")]
	[SerializeField] private GameObject groundItem_r;
	[SerializeField] private GameObject groundItem_p;
	[SerializeField] private GameObject groundItem_y;
	[SerializeField] private GameObject groundItem_g;


	public readonly SyncList<int> inventorySize = new SyncList<int>();
	// A list of ints corresponding to how many of INVEN_BOMB_TYPES the player is carrying, initialized to zero
	public readonly SyncList<int> inventoryList = new SyncList<int>();
	

	// The currently selected slot of the inventory (i.e. the bomb slot to be placed next)
	[SyncVar(hook=nameof(OnSelectedSlotChange))] public int selectedSlot = 0;

    // Singleton
    private GameUIManager gameUIManager;
	private RoundManager roundManager;


	public override void OnStartServer()
    {
		for (int i = 0; i < INVEN_BOMB_TYPES.Length; i++) inventorySize.Add(0);
		for (int i = 0; i < INVEN_BOMB_TYPES.Length; i++) inventoryList.Add(0);

		base.OnStartServer();
		// Subscribe to the damage event on the server
		this.GetComponent<Health>().EventLivesLowered += OnGhostEnter;

		roundManager = RoundManager.Singleton;
		if (roundManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

		roundManager.EventInventorySizeChanged += ChangeInventorySize;

		//Debug.Log("iterating through inventorysize refreshing with roundmanager value, current roundmanager inv size: " + roundManager.currentGlobalInventorySize);
		for (int i = 0; i < inventorySize.Count; i++)
		{
			inventorySize[i] = roundManager.currentGlobalInventorySize;
		}

	}

    // When player enters ghost mode, reset the inventroy
    [Server] private void OnGhostEnter(bool _) { ResetInventory(); }

    public override void OnStartClient()
    {
		inventoryList.Callback += OnInventoryChange;
		inventorySize.Callback += OnInventorySizeChange;

		gameUIManager = GameUIManager.Singleton;
        if (gameUIManager == null) Debug.LogError("Cannot find Singleton: GameUIManager");

		//Debug.Log("Client PlayerInventory starting for: " + gameObject.name);

		this.GetComponent<PlayerInterface>().UpdateInventorySize();
		// Subscribe to the synclist hook
		// inventoryList.Callback += OnInventoryChange;
		// inventorySize.Callback += OnInventorySizeChange;
	}

    private void Update()
    {
        // Code after this point is run only on the local player
        if (!isLocalPlayer || this.GetComponent<Player>().isEliminated) return;

        // Check for key press every frame
        ListenForSelectInput();
    }

    /// <summary>
    /// Listens for key press and rotates/selects the item stack
    /// </summary>
    [Client] void ListenForSelectInput()
    {
        // If the "next bomb" key is pressed
        if (KeyBindingManager.GetKeyDown(KeyAction.RotateNext)) SelectItemStack();

        // If the individual bombs are pressed
        if (KeyBindingManager.GetKeyDown(KeyAction.BigBomb)) SelectItemStack(0);
        if (KeyBindingManager.GetKeyDown(KeyAction.SludgeBomb)) SelectItemStack(1);
        if (KeyBindingManager.GetKeyDown(KeyAction.LaserBeem)) SelectItemStack(2);
        if (KeyBindingManager.GetKeyDown(KeyAction.PlasmaBall)) SelectItemStack(3);
    }

    /// <summary>
    /// Selects the slot at the given index.
    /// </summary>
    /// <param name="idx"> The index of the slot to switch to. -1 means to rotate the selection instead </param>
    [Client] private void SelectItemStack(int idx = -1)
    {
        // Play the bomb selection sound
        FindObjectOfType<AudioManager>().PlaySound("bombrotation");

        // If -1, then get the next index
        if (idx == -1) CmdSetSelectedSlot(GetNextAvailableBomb());
        else CmdSetSelectedSlot(idx);
    }

    /// <summary>
    /// Gets the next available slot. If no slots are available, the currently selected slot is returned
    /// </summary>
    /// <returns> The index of the slot </returns>
    [Client] private int GetNextAvailableBomb()
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
        if (nextSlot >= INVEN_BOMB_TYPES.Length) return 0;
        else return nextSlot;
    }

    /// <summary>
    /// Adds bombs to the players inventroy
    /// </summary>
    /// <param name="type"> The type of bomb to add </param>
    /// <param name="amt"> The number of bombs of this type to add </param>
    [Server] public void AddInventoryBomb(char type, int amt)
    {
        // Get the index corresponding to the bomb type
        int idx = Array.IndexOf(INVEN_BOMB_TYPES, type);

        // Store the extra bombs (bomb amount - spaces left) so we can drop extras on the ground 
        int fullInvOverflowValue = (inventoryList[idx] + amt) - inventorySize[idx];


        // First check if inventory for this bomb type isn't completely full:
        if (!(inventoryList[idx] == inventorySize[idx]))
        {
            // If the amount we want to add is larger than max values, then set the bomb quantity to max
            if (inventoryList[idx] + amt > inventorySize[idx])
            {
                inventoryList[idx] = inventorySize[idx];

                if (inventoryList[idx] + amt != inventorySize[idx])
                {
                    // Add conditional so that inventory add UI doesn't get displayed if inventory is already full
                    this.GetComponent<PlayerInterface>().DisplayInventoryAdd(idx, amt - fullInvOverflowValue);
                }
                Debug.Log(fullInvOverflowValue);

            }
            else        // Otherwise simply add the amount to inv, don't drop anything
            {
                inventoryList[idx] += amt;

                this.GetComponent<PlayerInterface>().DisplayInventoryAdd(idx, amt);
            }
        }

        // Otherwise, inventory is already full, drop all bombs on ground
        for (int i = 0; i < fullInvOverflowValue; i++)
        {
            ////
            Vector3 randomTransform = this.gameObject.transform.position;
            randomTransform.x = randomTransform.x + UnityEngine.Random.Range(-8f, 8f);
            randomTransform.z = randomTransform.z + UnityEngine.Random.Range(-8f, 8f);

            switch (type)
            {
                case 'r':
                    NetworkServer.Spawn((GameObject)Instantiate(groundItem_r, randomTransform + new Vector3(0f, 3f, 0f), Quaternion.identity));
                    break;
                case 'p':
                    NetworkServer.Spawn((GameObject)Instantiate(groundItem_p, randomTransform + new Vector3(0f, 3f, 0f), Quaternion.identity));
                    break;
                case 'y':
                    NetworkServer.Spawn((GameObject)Instantiate(groundItem_y, randomTransform + new Vector3(0f, 3f, 0f), Quaternion.identity));
                    break;
                case 'g':
                    NetworkServer.Spawn((GameObject)Instantiate(groundItem_g, randomTransform + new Vector3(0f, 3f, 0f), Quaternion.identity));
                    break;
            }


            ////
        }

        // if inventory is full, nothing gets added.
    }

	/// <summary>
	/// Listening to inventory size change event in RoundManager, set the new inventory size to this amt
	/// </summary>
	/// <param name="size"></param>
	[Server] public void ChangeInventorySize(int size)
	{
		for (int i = 0; i < inventorySize.Count; i++)
		{
			inventorySize[i] = size;
		}
	}

    public char[] GetBombTypes()
    {
        return INVEN_BOMB_TYPES;
    }

    public SyncList<int> GetMaxInvSizes()
    {
        return inventorySize;
    }


    /// <summary>
    /// Removes a bomb from the players inventory
    /// </summary>
    /// <param name="type"> The type of bomb to remove </param>
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

    /// <summary>
    /// Resets all bomb quantity back to zero
    /// </summary>
    [Server] public void ResetInventory()
    {
        for (int i = 0; i < inventoryList.Count; i++)
        {
            inventoryList[i] = 0;
        }
    }

    /// <summary>
    /// Changes the currently selected slot
    /// </summary>
    /// <param name="index"> The index of the slot to change to</param>
    [Command] public void CmdSetSelectedSlot(int index)
	{
		selectedSlot = index;
	}

    /// <summary>
    /// SyncVar hook for the variable selectedSlot
    /// </summary>
    [ClientCallback] private void OnSelectedSlotChange(int oldSlot, int newSlot)
    {
        // Update the player interface when selected slot changes
        this.GetComponent<PlayerInterface>().UpdateInventorySelected();

        if (isLocalPlayer) gameUIManager.ClientOnInventorySelectChanged(INVEN_BOMB_TYPES[newSlot], inventoryList[newSlot]);
    }


    /// <summary>
    /// SyncList hook for variable inventoryList
    /// </summary>
    [ClientCallback] private void OnInventoryChange(SyncList<int>.Operation op, int idx, int oldAmt, int newAmt)
    {
        // Update the player interface everytime the inventory changes
        this.GetComponent<PlayerInterface>().UpdateInventoryQuantity();

		Debug.Log("OnInventoryChange on " + gameObject.name);

        if (idx == selectedSlot && isLocalPlayer)
        {
            gameUIManager.ClientOnInventorySelectChanged(INVEN_BOMB_TYPES[idx], inventoryList[idx]);
        }
    }

	/// <summary>
	/// SyncList hook for variable InventorySize
	/// </summary>
	[ClientCallback] private void OnInventorySizeChange(SyncList<int>.Operation op, int idx, int oldAmt, int newAmt)
	{
		// Refresh the inventory UI so it shows the new frame and updated fill amount comparative to the inventory size change
		this.GetComponent<PlayerInterface>().UpdateInventorySize();

	}

	/// <summary>
	/// Returns the bomb type corresponding to the currently selected bomb
	/// </summary>
	/// <returns> The character of the bomb type. Returns 'e' if the bomb slot is empty</returns>
	public char GetSelectedBombType()
    {
        if (inventoryList[selectedSlot] <= 0) return 'e';

        return INVEN_BOMB_TYPES[selectedSlot];
    }
}
