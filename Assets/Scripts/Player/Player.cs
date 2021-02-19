﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

public class Player : NetworkBehaviour
{
    // Assets
    private HexGrid hexGrid;

    public float invincibilityDuration = 2.0f;
    public float ghostDuration = 5.0f;
    [SerializeField] public Health healthScript = null;

    [Header("Input")] [SerializeField] private string swapKey = "space";
    [SerializeField] private string spinKey = "o";
    [SerializeField] private string bombKey = "j";
    
    [SerializeField] private float defaultBombCooldown = 3f;
    private float defaultBombUseTimer = 0f;
    [SerializeField] private GameObject onDefaultBombReadyAnim;
    private bool playedOnDefaultBombReadyAnim = true;

    private float horizontalAxis;
    private float verticalAxis;
    private float fixedY = 7; // The Y position the player will be stuck to.

    [Header("Movement")] [SerializeField] private CharacterController controller;
    [SerializeField] private float movementSpeed = 50;
    [SerializeField] private float turnSpeed = 17f;

    [Header("HexTiles")] [SyncVar(hook = nameof(OnChangeHeldKey))]
    public char heldKey = 'g';

    public Vector3 heldHexScale = new Vector3(800, 800, 800);
    public Vector3 heldHexOffset = new Vector3(0, 25, 10);
    [SerializeField] public bool tileHighlights = true;

    public float spinHitboxDuration = 0.6f;
    public float spinAnimDuration = 0.8f;
    public float spinTotalCooldown = 0.8f;

    [SyncVar] public bool canMove = false;
    [SyncVar] public bool canPlaceBombs = false;
    [SyncVar] public bool canSpin = true;
    [SyncVar] public bool canSwap = false;
    [SyncVar] public bool canExitInvincibility = false;
    [SyncVar] public bool canBeHit = true;

    // Game Objects
    [Header("Required", order = 2)] public GameObject bomb;
    public GameObject laser;
    public GameObject plasma;
    public GameObject bigBomb;
    public GameObject blink;
    public GameObject gravityObject;

    // reference to Animator component
    public Animator animator;

    // private Caches
    private GameObject spinHitbox;
    private GameObject spinAnim;
    private Ray tileRay;
    private RaycastHit tileHit;
    private GameObject selectedTile;
    private GameObject heldHexModel;

    [SerializeField] private int maxStackSize = 3;
    readonly SyncList<char> itemStack = new SyncList<char>();
    [SerializeField] private Image[] stackUI = new Image[3];
    private Quaternion stackRotationLock;

    [SerializeField] private GameObject playerModel;

    public bool isDead = false; // when player has lost ALL lives
    //public bool isFrozen = true; // cannot move, but can rotate

    // Added for easy referencing of local player from anywhere
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        gameObject.name = "LocalPlayer";
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.Assert();
        LinkAssets();
        spinHitbox = gameObject.transform.Find("SpinHitbox").gameObject;
        spinAnim = this.gameObject.transform.Find("SpinVFX").gameObject;
        UpdateHeldHex(heldKey); // Initialize model

        if (isLocalPlayer)
        {
            //stackUI.GetComponent<Text>().enabled = true;
            //stackRotationLock = stackUI.transform.rotation;
        }

        itemStack.Callback += OnItemStackChange;

        //Debug.Log("local started");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        this.Assert();
        LinkAssets();
        spinHitbox = gameObject.transform.Find("SpinHitbox").gameObject;
        spinAnim = this.gameObject.transform.Find("SpinVFX").gameObject;

        //fixedY = this.transform.position.y;  // To prevent bugs from collisions

        // events
        healthScript.EventLivesLowered += SetCanPlaceBombs;
        healthScript.EventLivesLowered += SetCanSpin;
        healthScript.EventLivesLowered += SetCanSwap;
        healthScript.EventLivesLowered += SetCanBeHit;
        healthScript.EventLivesLowered += ClearItemStack;

        healthScript.EventGhostExit += SetCanPlaceBombs;
        healthScript.EventGhostExit += SetCanSpin;
        healthScript.EventGhostExit += SetCanSwap;
        healthScript.EventGhostExit += SetCanExitInvincibility;

        //Debug.Log("server started");
    }

    void Assert()
    {
        if (spinTotalCooldown <= spinHitboxDuration && spinTotalCooldown <= spinAnimDuration)
        {
            Debug.LogError("Player.cs: spinCooldown is lower than spinDuration.");
        }
    }

    // Update is called once per frame
    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer) return;

        if (isDead) return; // if dead, disable all player updates

        this.transform.position = new Vector3(this.transform.position.x, fixedY, this.transform.position.z);

        ApplyMovement();
        ApplyTileHighlight();
        ListenForSwapping();
        ListenForBombUse();
        ListenForSpinning();
    }

    // Update version for server
    [ServerCallback]
    private void LateUpdate()
    {
        if (!isServer) return;

        // Handle default-placing bomb anim
        if (!playedOnDefaultBombReadyAnim && defaultBombUseTimer > defaultBombCooldown)
        {
            Debug.Log("inside");
            RpcEnableBombPlaceAnimation();
            playedOnDefaultBombReadyAnim = true;
        }
        defaultBombUseTimer += Time.deltaTime;
    }

    [ClientRpc]
    void RpcEnableBombPlaceAnimation()
    {
        onDefaultBombReadyAnim.SetActive(true);
    }

    void LinkAssets()
    {
        hexGrid = GameObject.FindGameObjectWithTag("HexGrid")
            .GetComponent<HexGrid>(); // Make sure hexGrid is created before the player
        if (!hexGrid) Debug.LogError("Player.cs: No cellPrefab found.");
    }

    [Client]
    void ListenForBombUse()
    {
        // raycast down to check if tile is occupied
        if (Input.GetKeyDown(bombKey) && (this.canExitInvincibility || this.canPlaceBombs))
        {
            ExitInvincibility();
            CmdBombUse();
        }
    }

    [Client]
    public void ListenForSpinning()
    {
        if (Input.GetKeyDown(spinKey))
        {
            ExitInvincibility();
            CmdSpin();
        }
    }

    [Command]
    void CmdBombUse(NetworkConnectionToClient sender = null)
    {
        Ray tileRay = new Ray(transform.position + transform.up * 5, Vector3.down * 10);

        if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            var hexCell = tileHit.transform.gameObject.GetComponentInParent<HexCell>();
            if (!hexCell.IsOccupiedByComboObject())
            {
                if (itemStack.Count > 0)
                {
                    char bombType = itemStack[itemStack.Count - 1]; // peek top of stack
                    sender.identity.GetComponent<Player>().RemoveItemCombo(); // pop it off

                    switch (bombType)
                    {
                        case 'b':
                            Debug.Log("Blue Bomb Type");
                            SpawnBlinkObject();
                            break;
                        case 'g':
                            Debug.Log("Green Bomb Type");
                            SpawnPlasmaObject();
                            break;
                        case 'y':
                            Debug.Log("Yellow Bomb Type");
                            this.SpawnLaserObject();
                            break;
                        case 'r':
                            Debug.Log("Red Bomb Type");
                            SpawnBigBombObject();
                            break;
                        case 'p':
                            Debug.Log("Purple Bomb Type");
                            SpawnGravityObject();
                            break;
                        case 'w':
                            Debug.Log("White Bomb Type");
                            // unimplemented
                            break;
                        default:
                            // code should not reach here
                            Debug.Log("Bomb type not found");
                            break;
                    }
                }
                else if (defaultBombUseTimer > defaultBombCooldown)  // we should play a flashing anim when its ready.
                {
                    Debug.Log("dropping default bomb");
                    onDefaultBombReadyAnim.SetActive(false);
                    playedOnDefaultBombReadyAnim = false;
                    // Else use the default bomb
                    this.SpawnDefaultBomb();
                    defaultBombUseTimer = 0;
                    
                }
                else // When the default bomb CD isn't up
                {
                    
                }
            }
        }
    }

    [Server]
    void SpawnDefaultBomb()
    {
        Debug.Log("spawning bomb");
        GameObject _bomb = (GameObject) Instantiate(bomb,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_bomb);
    }

    [Server]
    void SpawnLaserObject()
    {
        GameObject _laser = (GameObject) Instantiate(laser,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_laser);
    }

    [Server]
    void SpawnPlasmaObject()
    {
        GameObject _plasma = (GameObject) Instantiate(plasma,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_plasma);
    }

    [Server]
    void SpawnBlinkObject()
    {
        GameObject _blink = (GameObject) Instantiate(blink,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_blink);
    }

    [Server]
    void SpawnGravityObject()
    {
        GameObject _gravity = (GameObject) Instantiate(gravityObject,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_gravity);
    }

    [Server]
    void SpawnBigBombObject()
    {
        GameObject _bigBomb = (GameObject) Instantiate(bigBomb,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_bigBomb);
    }

    public void SetCanPlaceBombs(bool val)
    {
        this.canPlaceBombs = val;
    }

    public void SetCanSpin(bool val)
    {
        this.canSpin = val;
    }

    public IEnumerator Spin()
    {
        if (canSpin)
        {
            if (!spinHitbox)
            {
                Debug.LogError("Player.cs: no spinHitbox assigned");
            }
            else
            {
                StartCoroutine(HandleSpinHitbox());
                StartCoroutine(HandleSpinAnim());
                FindObjectOfType<AudioManager>().PlaySound("playerSpin");

                canSpin = false;
                yield return new WaitForSeconds(spinTotalCooldown);
                canSpin = true;
            }
        }
    }

    [Command]
    void CmdSpin()
    {
        // Spin for server
        StartCoroutine(Spin());
        RpcSpin();
    }

    [ClientRpc]
    void RpcSpin()
    {
        // Client will spin for all observers
        StartCoroutine(Spin());
    }

    private IEnumerator HandleSpinHitbox()
    {
        spinHitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(spinHitboxDuration);
        spinHitbox.gameObject.SetActive(false);
    }

    private IEnumerator HandleSpinAnim()
    {
        spinAnim.gameObject.SetActive(true);
        yield return new WaitForSeconds(spinAnimDuration);
        spinAnim.gameObject.SetActive(false);
    }

    void OnChangeHeldKey(char oldHeldKey, char newHeldKey)
    {
        //heldKey = newHeldKey;
        Debug.Log("ON CHANGE TEST");
        UpdateHeldHex(newHeldKey);
    }

    void UpdateHeldHex(char newHeldKey)
    {
        if (this.heldHexModel)
        {
            Destroy(this.heldHexModel, 0f);
            Debug.Log("Destroyed held hex");
        }

        // Create the hex model in the player's hand
        this.heldHexModel = Instantiate(
            hexGrid.ReturnModelByCellKey(newHeldKey),
            playerModel.transform.position + playerModel.transform.up * heldHexOffset.y +
            playerModel.transform.forward * heldHexOffset.x,
            playerModel.transform.rotation,
            playerModel.transform
        );

        this.heldHexModel.gameObject.transform.localScale = heldHexScale;
    }

    // Apply movement to the player, using WASD or Arrow keys
    [Client]
    void ApplyMovement()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        // Update animation state
        if (horizontalAxis != 0 || verticalAxis != 0) animator.SetBool("anim_isRunning", true);
        else                                          animator.SetBool("anim_isRunning", false);

        Vector3 direction = new Vector3(this.horizontalAxis, 0f, this.verticalAxis).normalized;
        if (direction != Vector3.zero)
        {
            playerModel.transform.rotation = Quaternion.Slerp(
                playerModel.transform.rotation,
                Quaternion.LookRotation(direction),
                turnSpeed * Time.deltaTime
            );

            if (this.canMove) controller.Move(direction * movementSpeed * Time.deltaTime);
        }
    }

    public void SetCanMove(bool val)
    {
        this.canMove = val;
    }

    // Listens for key press and swaps the tile beneath the player
    [Client]
    void ListenForSwapping()
    {
        if (Input.GetKeyDown(swapKey) && (this.canExitInvincibility || this.canSwap))
        {
            ExitInvincibility();
            tileRay = new Ray(transform.position + transform.up * 5, Vector3.down * 10);

            if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
            {
                Debug.Log("space pressed");
                GameObject modelHit = tileHit.transform.gameObject;
                //HexCell hexCell = modelHit.GetComponentInParent<HexCell>();
                char newKey = modelHit.GetComponentInParent<HexCell>().GetKey();

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
            if (selectedTile)
                selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 0f);
            selectedTile = tileHit.transform.gameObject;
            selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 1f);
        }
    }

    //[TargetRpc]
    //public void TargetAddItemCombo(NetworkConnection target, char colorKey)
    //{
    //    CmdAddItemCombo(colorKey);
    //}

    //public void AddItemCombo(char colorKey)
    //{
    //    if (!isLocalPlayer) return;
    //    CmdAddItemCombo(colorKey);
    //}

    [Server]
    public void AddItemCombo(char colorKey)
    {
        if (itemStack.Count < maxStackSize)
        {
            itemStack.Add(colorKey); // Push new combo to stack
        }
        else
        {
            itemStack.RemoveAt(0); // Remove oldest combo (bottom of stack)
            itemStack.Add(colorKey); // Push new combo to stack
        }
    }

    [Server]
    public void RemoveItemCombo()
    {
        if (itemStack.Count > 0)
        {
            itemStack.RemoveAt(itemStack.Count - 1); // pop it off
        }
    }

    [Server]
    public void ClearItemStack(bool val = true)
    {
        while (itemStack.Count > 0)
        {
            itemStack.RemoveAt(itemStack.Count - 1); // pop it off
        }
    }

    void OnItemStackChange(SyncList<char>.Operation op, int idx, char oldColor, char newColor)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < itemStack.Count) stackUI[i].color = TempGetKeyColor(itemStack[i]);
            else stackUI[i].color = Color.white;
        }
    }

    // Temp function - get color associated with key
    Color TempGetKeyColor(char key)
    {
        switch (key)
        {
            case 'b': return Color.blue;
            case 'g': return Color.green;
            case 'y': return Color.yellow;
            case 'r': return Color.red;
            case 'p': return Color.magenta;
            case 'w': return Color.grey;
            default: return Color.white;
        }
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
            this.canSpin = true;
            this.canBeHit = true;
            this.canSwap = true;
            this.canPlaceBombs = true;
            healthScript.SignalExit();
            this.canExitInvincibility = false;
        }
    }

    public void SetCanExitInvincibility(bool val)
    {
        this.canExitInvincibility = val;
    }
}