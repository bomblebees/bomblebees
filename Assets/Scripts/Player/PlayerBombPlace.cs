using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerBombPlace : NetworkBehaviour
{
    [Header("Required")]
    public GameObject bombPrefab;
    public GameObject laserPrefab;
    public GameObject plasmaPrefab;
    public GameObject sludgePrefab;

    /// <summary>
    /// Whether the player can place bombs.
    /// </summary>
    [HideInInspector] public bool canPlaceBombs = true;

    // Raycast caches for PlaceBomb()
    private Ray tileRay;
    private RaycastHit tileHit;

    // Event manager singleton
    private EventManager eventManager;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Subscribe to damage events
        this.GetComponent<Health>().EventLivesLowered += OnGhostEnter;
        this.GetComponent<Health>().EventGhostExit += OnGhostExit;
    }

    // Cannot place bombs in ghost mode
    private void OnGhostEnter(bool _) { canPlaceBombs = false; }
    private void OnGhostExit(bool _) { canPlaceBombs = true; }

    public override void OnStartServer()
    {
        base.OnStartServer();

        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");
    }

    void Update()
    {
        // Code after this point is run only on the local player
        if (!isLocalPlayer || this.GetComponent<Player>().isEliminated) return;

        // Check for key press every frame
        ListenForPlaceInput();
    }

    /// <summary>
    /// Checks for spin key presses, called in Update()
    /// </summary>
    [Client] private void ListenForPlaceInput()
    {
        if (!canPlaceBombs) return;

        // When place key is pressed down
        if (KeyBindingManager.GetKeyDown(KeyAction.Place))
        {
            // Get the currently selected bomb type
            char bombType = this.GetComponent<PlayerInventory>().GetSelectedBombType();

            // If no bombs then return
            if (bombType == 'e' || bombType == '1' || bombType == '2' || bombType == '3' || bombType == '4')
            {
                return;
            } else
            {
                // Place bomb on the server
                CmdSpawnBomb(bombType);
            }
        }
    }

    [Command] public void CmdSpawnBomb(char bombType)
    {
        // Get the tile underneath the player
        tileRay = new Ray(transform.position + transform.up * 5, Vector3.down * 10);

        if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            var hexCell = tileHit.transform.gameObject.GetComponentInParent<HexCell>();

            // If the hex tile is not occupied, player can place a bomb
            if (!hexCell.IsOccupiedByComboObject())
            {
                //// Get the currently selected bomb type
                //char bombType = this.GetComponent<PlayerInventory>().GetSelectedBombType();

                // Spawn the bomb
                SpawnBombType(bombType);
            }
        }
    }

    /// <summary>
    /// Spawns the bomb associated with the given bomb type
    /// </summary>
    /// <param name="bombType">The character corresponding to the bomb type</param>
    [Server] private void SpawnBombType(char bombType)
    {
        // Remove the bomb type from the player inventory by 1
        this.GetComponent<PlayerInventory>().RemoveInventoryBomb(bombType); 

        switch (bombType)
        {
            case 'g': SpawnBomb(plasmaPrefab); break;
            case 'y': SpawnBomb(laserPrefab); break;
            case 'r': SpawnBomb(bombPrefab); break;
            case 'p': SpawnBomb(sludgePrefab); break;
            default:
                // code should not reach here
                Debug.LogError("Bomb type not found");
                break;
        }
    }

    /// <summary>
    /// Spawns the given bomb on the server
    /// </summary>
    /// <param name="prefab">The prefab of the bomb to spawn</param>
    [Server] private void SpawnBomb(GameObject prefab)
    {
        // Instantiate the bomb
        GameObject _bomb = Instantiate(prefab, this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);

		ComboObject bombComponent = _bomb.GetComponent<ComboObject>();

		// Set the owner player as this player
		bombComponent.ownerPlayer = this.gameObject;

		// Set the initial triggered player as this player
		bombComponent.triggeringPlayer = this.gameObject;

		// Spawn the object on the server
		NetworkServer.Spawn(_bomb);

        // Call the bomb placed event
        eventManager.OnBombPlaced(_bomb, this.gameObject);
    }
}
