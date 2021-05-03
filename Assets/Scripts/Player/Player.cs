using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
//using Castle.Core.Smtp;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;
//using NSubstitute.Exceptions;
using Debug = UnityEngine.Debug;

public class Player : NetworkBehaviour
{
    // Vars transferred from room player object
    [Header("Identification")]
    [SyncVar] public ulong steamId = 0; // unique steam id
    [SyncVar] public ulong playerId = 0; // unique player id
    [SyncVar] public string steamName = "[Steam Name]"; // steam username
    [SyncVar] public int characterCode;
    [SyncVar] public Color playerColor;

    // Assets
    [Header("Debug")]
 //   public bool debugMode = false;
 //   public string debugBombPress1 = "8";
 //   public string debugBombPress2 = "9";
 //   public string debugBombPress3 = "e";
	//public string debugBombPress4 = ";";
	//public string debugGroundItemSpawn = "g";
    private HexGrid hexGrid;

    [Header("Respawn")]
    public float invincibilityDuration = 2.0f;
    public float ghostDuration = 5.0f;
    [SerializeField] public Health healthScript = null;

    [Header("HexTiles")] [SyncVar(hook = nameof(OnChangeHeldKey))]
    public char heldKey = 'g';

    public Vector3 heldHexScale = new Vector3(800, 800, 800);
    public Vector3 heldHexOffset = new Vector3(0, 25, 10);
    [SerializeField] public bool tileHighlights = true;

    //[SyncVar] public bool canPlaceBombs = false;
    [SyncVar] public bool canSwap = false;
    [SyncVar] public bool canExitInvincibility = false;
    [SyncVar] public bool canBeHit = true;

    // Game Objects
    [Header("Required", order = 2)]
    public GameObject groundItemPickupHitbox;
	//private float slowScalar = 1f;
    public float timeSinceSlowed = 0f;
    //private float sludgedScalar = 1f;
    private float timeSinceSludged = 0f;
    private float sludgedDuration = 0f;  // set by the sludge object
    private float sludgeEndAnim = -40f;
    // private float timeSinceSludgedEnd = -40f;
    public float timeSinceSludgedEndDur = 0f;
    private bool sludgeEffectEnded = true;
    private bool sludgeEffectStarted = false;

    public float slowTimeCheckInterval = 0.05f;
    public GameObject sludgeVFX;

    // private Caches
    private Ray tileRay;
    private RaycastHit tileHit;
    public GameObject selectedTile;
    private GameObject heldHexModel;

    [SerializeField] private int maxStackSize = 3;

    private GameObject playerMesh;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject highlightModel;

    public bool isDead = false; // when player has lost ALL lives

    // Event manager singleton
    private EventManager eventManager;

    // Added for easy referencing of local player from anywhere
    public override void OnStartLocalPlayer()
    {
        gameObject.name = "LocalPlayer";
        base.OnStartLocalPlayer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        LinkAssets();
        UpdateHeldHex(heldKey); // Initialize model

        // Set player color
        playerMesh = playerModel.transform.GetChild(0).gameObject;
        playerMesh.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", playerColor);
        // Disable tile highlight outline
        if (isLocalPlayer) highlightModel.SetActive(true);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        LinkAssets();

        //fixedY = this.transform.position.y;  // To prevent bugs from collisions

        // events
        healthScript.EventLivesLowered += SetCanSwap;
        healthScript.EventLivesLowered += SetCanBeHit;
        healthScript.EventLivesLowered += ClearItemStack;

        healthScript.EventGhostExit += SetCanSwap;
        healthScript.EventGhostExit += SetCanExitInvincibility;

        //Debug.Log("server started");

        // Event manager singleton
        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");
    }

    // Update is called once per frame
    [ClientCallback]
    private void Update()
    {
        
        if (timeSinceSludged < 0 && sludgeEffectStarted)
        {
            sludgeEffectStarted = false;
            CmdSetSludgeEffectEnded(true);
        }

        if (sludgeEffectEnded)
        {
            /* SLUDGE EFFECT ENDS HERE */
            // start coroutine
            // timeSinceSludgedEnd = -4f;
            // playerMesh.GetComponent<Renderer>().materials[2].SetFloat("_CoverAmount", -40f);

            //sludgedScalar = 1.0f;
            if (sludgeVFX.activeSelf)
            {
                sludgeVFX.SetActive(false);
                //this.canSpin = true;
            }

            if (sludgeEndAnim > -40f)
            {
                sludgeEndAnim -= Time.deltaTime * 20f; // temp
                playerMesh.GetComponent<Renderer>().materials[2].SetFloat("_CoverAmount", sludgeEndAnim);
            }
        }


        if (!isLocalPlayer) return;

        if (gameObject.name != "LocalPlayer") gameObject.name = "LocalPlayer";

        if (isDead) return; // if dead, disable all player updates

        ApplyTileHighlight();
        ListenForSwapping();
        ListenForBombRotation();

        //Debug.Log("tme sine cludge" + timeSinceSludged);


        //if (this.timeSinceSlowed > slowTimeCheckInterval)
        //{
        //    slowScalar = 1.0f;
        //}
    }

    [Command] public void CmdSetSludgeEffectEnded(bool cond)
    {
        RpcSetSludgeEffectEnded(cond);
    }

    [ClientRpc] public void RpcSetSludgeEffectEnded(bool cond)
    {
        sludgeEffectEnded = cond;
    }

    void LinkAssets()
    {
        hexGrid = GameObject.FindGameObjectWithTag("HexGrid")
            .GetComponent<HexGrid>(); // Make sure hexGrid is created before the player
        if (!hexGrid) Debug.LogError("Player.cs: No cellPrefab found.");
    }

    void OnChangeHeldKey(char _, char newHeldKey)
    {
        // commented out for now
        UpdateHeldHex(newHeldKey);
    }

    void UpdateHeldHex(char newHeldKey)
    {
        this.GetComponent<PlayerInterface>().UpdateHexHud(newHeldKey);


        // commented out, below is for hex model
      
        //if (this.heldHexModel)
        //{
        //    Destroy(this.heldHexModel, 0f);
        //    Debug.Log("Destroyed held hex");
        //}

        //// Create the hex model in the player's hand
        //this.heldHexModel = Instantiate(
        //    hexGrid.ReturnModelByCellKey(newHeldKey),
        //    playerModel.transform.position + playerModel.transform.up * heldHexOffset.y +
        //    playerModel.transform.forward * heldHexOffset.x,
        //    playerModel.transform.rotation,
        //    playerModel.transform
        //);

        //this.heldHexModel.gameObject.transform.localScale = heldHexScale;
    }

    // Listens for key press and swaps the tile beneath the player
    [Client]
    void ListenForSwapping()
    {
        if (KeyBindingManager.GetKeyDown(KeyAction.Swap) && (this.canExitInvincibility || this.canSwap))
        {
            ExitInvincibility();
            tileRay = new Ray(transform.position + transform.up * 5, Vector3.down * 10);

            if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
            {
                GameObject modelHit = tileHit.transform.gameObject;
                //HexCell hexCell = modelHit.GetComponentInParent<HexCell>();
                char newKey = modelHit.GetComponentInParent<HexCell>().GetKey();
                if (HexCell.ignoreKeys.Contains(newKey)) return;

                int cellIdx = modelHit.GetComponentInParent<HexCell>().GetThis().getListIndex();

                CmdSwap(cellIdx, GetHeldKey());
                //hexGrid.netIdentity.AssignClientAuthority(connectionToClient);
                //hexGrid.CmdSwapHexAndKey(cellIdx, getHeldKey());
                //hexGrid.netIdentity.RemoveClientAuthority();

                FindObjectOfType<AudioManager>().PlaySound("playerSwap");

                // Only update models and grids if it is a new key
                if (!this.heldKey.Equals(newKey))
                {
                    CmdSetHeldKey(newKey);
                }
            }
        }
    }

    public void SetCanSwap(bool val)
    {
        this.canSwap = val;
    }

    [Command]
    void CmdSwap(int cellIdx, char heldKey, NetworkConnectionToClient sender = null)
    {
        hexGrid.SwapHexAndKey(cellIdx, heldKey, sender.identity);
    }

    // Applies the highlight shader to the tile the player is "looking" at
    // This is the tile that will be swapped, and one where the bomb will be placed on
    [Client]
    void ApplyTileHighlight()
    {
        if (!tileHighlights || isDead)
        {
            if (selectedTile) selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 0f);
            return;
        }

        tileRay = new Ray(transform.position + transform.up * 5, Vector3.down * 10);

        if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            // Debug.Log("hit");

            // Apply indicator to hex tile to show the tile selected
            // if (selectedTile)
            //     selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 0f); // toggles off prev selected
            
            selectedTile = tileHit.transform.gameObject;
            // selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 0f);    // toggle on current
            highlightModel.transform.position = new Vector3(selectedTile.transform.position.x, -5.8f, selectedTile.transform.position.z);
        }
        // else
        // {
        //     selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 0f);
        // }
    }

    // Listens for key press and rotates the item stack
    [Client]
    void ListenForBombRotation()
    {
		if (KeyBindingManager.GetKeyDown(KeyAction.RotateNext))
        {
            CmdRotateItemStack();
			FindObjectOfType<AudioManager>().PlaySound("bombrotation");
		}
		if (KeyBindingManager.GetKeyDown(KeyAction.BigBomb))
		{
			CmdSelectItemSlot(0);
			FindObjectOfType<AudioManager>().PlaySound("bombrotation");
		}
		if (KeyBindingManager.GetKeyDown(KeyAction.SludgeBomb))
		{
			CmdSelectItemSlot(1);
			FindObjectOfType<AudioManager>().PlaySound("bombrotation");
		}
		if (KeyBindingManager.GetKeyDown(KeyAction.LaserBeem))
		{
			CmdSelectItemSlot(2);
			FindObjectOfType<AudioManager>().PlaySound("bombrotation");
		}
		if (KeyBindingManager.GetKeyDown(KeyAction.PlasmaBall))
		{
			CmdSelectItemSlot(3);
			FindObjectOfType<AudioManager>().PlaySound("bombrotation");
		}
	}

    [Command]
    public void CmdRotateItemStack()
    {
        this.GetComponent<PlayerInventory>().RotateSelectedSlot();
    }

	[Command]
	public void CmdSelectItemSlot(int index, NetworkConnectionToClient sender = null)
	{
		this.GetComponent<PlayerInventory>().SwitchToSlot(index);
        //BombUse(sender);
    }

    [Server]
    public void ClearItemStack(bool val = true)
    {
        this.GetComponent<PlayerInventory>().ResetInventory();
    }

    [Command]
    void CmdSetHeldKey(char newKey)
    {
        RpcSetHeldKey(newKey);
    }

    [ClientRpc]
    void RpcSetHeldKey(char newKey)
    {
        SetHeldKey(newKey); // Sync held key
        UpdateHeldHex(newKey); // Update model for all observers
    }

    public void SetHeldKey(char key)
    {
        this.heldKey = key;
    }

    public char GetHeldKey()
    {
        return this.heldKey;
    }

    public void SetCanBeHit(bool val)
    {
        this.canBeHit = val;
    }

    public void ExitInvincibility()
    {
        if (canExitInvincibility)
        {
            //this.canSpin = true;
            this.canBeHit = true;
            this.canSwap = true;
            //this.canPlaceBombs = true;
            //healthScript.SignalExit();
            this.canExitInvincibility = false;
			FindObjectOfType<AudioManager>().PlaySound("playerRespawn");
        }
    }

    public void SetCanExitInvincibility(bool val)
    {
        this.canExitInvincibility = val;
    }

    public void ApplySpeedScalar(float val)
    {
        //this.slowScalar *= val;
    }
    
    public void ApplySludgeSlow(float slowRate, float slowDur)
    {
        if (isServer) RpcApplySludgeSlow(slowRate, slowDur);
        else CmdApplySludgeSlow(slowRate, slowDur);

        //ResetSpinCharge();
        //this.canSpin = false;
    }

    [Command] public void CmdApplySludgeSlow(float slowRate, float slowDur) { RpcApplySludgeSlow(slowRate, slowDur); }
    [ClientRpc] public void RpcApplySludgeSlow(float slowRate, float slowDur)
    {
        /* SLUDGE STATUS EFFECT STARTS HERE*/
        sludgeEndAnim = -3f;
        playerMesh.GetComponent<Renderer>().materials[2].SetFloat("_CoverAmount", sludgeEndAnim);

		switch (UnityEngine.Random.Range(1, 4))
		{
			case 1:
				FindObjectOfType<AudioManager>().PlaySound("playerEw1");
				break;
			case 2:
				FindObjectOfType<AudioManager>().PlaySound("playerEw2");
				break;
			case 3:
				FindObjectOfType<AudioManager>().PlaySound("playerEw3");
				break;
		}

		var slowFactor = 1 - slowRate;
        //this.sludgedScalar = slowFactor;
        this.sludgedDuration = slowDur;
        this.timeSinceSludged = slowDur;
        this.sludgeEffectStarted = true;
        this.sludgeEffectEnded = false;
        sludgeVFX.SetActive(true);
        /* APPLY EFFECTS THAT HAPPEN ONCE PER SLUDGE-EFFECT HERE */
        if (timeSinceSludged > 0)
        {
        }
    }
}