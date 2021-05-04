using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerSwap : NetworkBehaviour
{

    /// <summary>
    /// The reference to the hex board
    /// </summary>
    private HexGrid hexGrid;

    /// <summary>
    /// Whether the player can swap.
    /// </summary>
    [HideInInspector] public bool canSwap = true;

    /// <summary>
    /// The currently held tile of the player represented as a color character
    /// </summary>
    [SyncVar(hook = nameof(OnChangeHeldKey))] public char heldKey;

    // Raycast caches for SwapHexes()
    private Ray tileRay;
    private RaycastHit tileHit;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Held key can be assigned before the client starts here
        heldKey = 'g';
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Get the reference to the hexGrid
        hexGrid = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
        if (!hexGrid) Debug.LogError("PlayerSwap.cs: HexGrid not found.");

        // Initialize the hex HUD
        this.GetComponent<PlayerInterface>().UpdateHexHud(heldKey);

        // Subscribe to damage events
        this.GetComponent<Health>().EventLivesLowered += OnGhostEnter;
        this.GetComponent<Health>().EventGhostExit += OnGhostExit;
    }

    // Cannot swap in ghost mode
    private void OnGhostEnter(bool _) { canSwap = false; }
    private void OnGhostExit(bool _) { canSwap = true; }

    void Update()
    {
        // Code after this point is run only on the local player
        if (!isLocalPlayer) return;

        // Check for key press every frame
        ListenForSwapInput();
    }

    /// <summary>
    /// Checks for swap key presses, called in Update()
    /// </summary>
    [Client] public void ListenForSwapInput()
    {
        if (!canSwap) return;

        // When swap key is pressed down
        if (KeyBindingManager.GetKeyDown(KeyAction.Swap))
        {
            SwapHexes();
        }
    }

    [Client] public void SwapHexes()
    {

        //ExitInvincibility();

        // Get the tile underneath the player
        tileRay = new Ray(transform.position + transform.up * 5, Vector3.down * 10);

        if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            GameObject modelHit = tileHit.transform.gameObject;

            // Get the key of the hex cell underneath, this is the new key for the player
            char newKey = modelHit.GetComponentInParent<HexCell>().GetKey();

            // If key is not swappable, return
            if (HexCell.ignoreKeys.Contains(newKey)) return;

            // Get the hex cell index of the hex cell we need to swap
            int cellIdx = modelHit.GetComponentInParent<HexCell>().GetThis().getListIndex();

            // Play swap sound
            FindObjectOfType<AudioManager>().PlaySound("playerSwap");

            // Only swap if it is a new key
            if (!this.heldKey.Equals(newKey))
            {
                // Do the swap on the server
                CmdSwap(cellIdx, this.heldKey, newKey);
            }
        }
    }

    /// <summary>
    /// Swaps the keys on the hex board with the key on the player.
    /// </summary>
    /// <param name="cellIdx"> The index of the hex cell that is being swapped </param>
    /// <param name="heldKey"> The color key of the player before the swap </param>
    /// <param name="newKey"> The color key of the hex cell before the swap </param>
    /// <param name="sender"> The player who called this function </param>
    [Command] void CmdSwap(int cellIdx, char heldKey, char newKey, NetworkConnectionToClient sender = null)
    {
        // Swap the hex of the player with the hex that is selected
        hexGrid.SwapHexAndKey(cellIdx, heldKey, sender.identity);

        // Update the held key of the player
        this.heldKey = newKey;
    }

    /// <summary>
    /// SyncVar hook for variable heldKey
    /// </summary>
    [ClientCallback] void OnChangeHeldKey(char oldHeldKey, char newHeldKey)
    {
        // Update hex tile on the HUD for this player for every observer
        this.GetComponent<PlayerInterface>().UpdateHexHud(newHeldKey);
    }

}
