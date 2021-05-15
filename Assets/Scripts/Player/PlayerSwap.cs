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
	[SerializeField] public bool canSwapWhileGhost = false;

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

        // Enable tile highlight outline for the local player
        if (isLocalPlayer) highlightModel.SetActive(true);
    }

    // Cannot swap in ghost mode
    private void OnGhostEnter(bool _) { canSwap = canSwapWhileGhost; }
    private void OnGhostExit(bool _) { canSwap = true; }

    void Update()
    {
        // Code after this point is run only on the local player
        if (!isLocalPlayer) return;

        // Applies the tile selection highlight
        ApplyTileHighlight();

        // Only run the rest of update if the player is not eliminated
        if (this.GetComponent<Player>().isEliminated) return;

        // Check for key press every frame
        ListenForSwapInput();

    }

    #region Swap

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

    #endregion

    #region Tile Highlight

    [Header("Tile Highlight/Selection")]

    /// <summary>
    /// The hex object the player is currently selecting
    /// </summary>
    [HideInInspector] public GameObject selectedTile;

    [SerializeField] private GameObject highlightModel;


    /// <summary>
    /// Whether tile highlights are enabled or not
    /// </summary>
    [SerializeField] public bool tileHighlights = true;

    /// <summary>
    /// Applies the highlight shader to the tile the player is "looking" at
    /// <para> This is the tile that will be swapped, and one where the bomb will be placed on </para>
    /// </summary>
    [Client]
    void ApplyTileHighlight()
    {
        // If tile highlights are disabled or player is dead, disable the highlight model 
        if (!tileHighlights || this.GetComponent<Player>().isEliminated)
        {
            highlightModel.SetActive(false);
            //if (selectedTile) selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 0f);
        } else
        {
            // Get the tile underneath the player
            tileRay = new Ray(transform.position + transform.up * 5, Vector3.down * 10);

            if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
            {
                // If the tile is already the currently selected tile, do nothing
                //if (tileHit.transform.gameObject == selectedTile)
                //{
                //    return;
                //}

                // This is the new selected tile
                selectedTile = tileHit.transform.gameObject;

                // Enable the highlight model
                highlightModel.SetActive(true);

                // Set the highlight model's position to the tiles position
                highlightModel.transform.position = new Vector3(selectedTile.transform.position.x, -5.8f, selectedTile.transform.position.z);
                
    
            }
        }


    }

    #endregion

}
