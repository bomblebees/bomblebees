using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
    [SyncVar] public Color playerColor;

    // Assets
    [Header("Debug")]
    public bool debugMode = false;
    public string debugBombPress1 = "i";
    public string debugBombPress2 = "a";
    public string debugBombPress3 = "e";
    public string debugBombPress4 = ";";
    private HexGrid hexGrid;
    public bool isStunned = false;
    public float stunnedDuration = 0;

    [Header("Respawn")]
    public float invincibilityDuration = 2.0f;
    public float ghostDuration = 5.0f;
    [SerializeField] public Health healthScript = null;

    [Header("Input")] [SerializeField] private string swapKey = "space";
    [SerializeField] private string spinKey = "o";
    [SerializeField] private string bombKey = "j";
    
    [SerializeField] public float defaultBombCooldown = 3f;
    private float defaultBombUseTimer = 0f;
    [SerializeField] private GameObject onDefaultBombReadyAnim;
    private bool playedOnDefaultBombReadyAnim = true;
    public int queenPoints = 0;
    public int queenPointThreshhold = 2;
    public bool isQueen = false;
    public float queenDuration = 10f;

    private float horizontalAxis;
    private float prevHorizontalAxis;
    private float verticalAxis;
    private float prevVerticalAxis;
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
    public GameObject spinHit = null; // the bomb that was hit with spin

    // Game Objects
    [Header("Required", order = 2)] public GameObject bomb;
    public GameObject laser;
    public GameObject plasma;
    public GameObject bigBomb;
    public GameObject blink;
    public GameObject gravityObject;
    private float sludgeInfluence = 1;

    // reference to Animator, Network Animator components
    public Animator animator;
    public NetworkAnimator networkAnimator;
    public bool isRunAnim = false;
    public bool isIdleAnim = false;


    // private Caches
    private GameObject spinHitbox;
    private GameObject spinAnim;
    private Ray tileRay;
    private RaycastHit tileHit;
    private GameObject selectedTile;
    private GameObject heldHexModel;

    [SerializeField] private int maxStackSize = 3;
    readonly SyncList<char> itemStack = new SyncList<char>();

    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject ghostModel;

    public bool isDead = false; // when player has lost ALL lives
    //public bool isFrozen = true; // cannot move, but can rotate

    // Event manager singleton
    private EventManager eventManager;

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

        // Set player color
        GameObject mesh = playerModel.transform.GetChild(0).gameObject;
        mesh.GetComponent<Renderer>().material.SetColor("_BaseColor", playerColor);

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

        // Event manager singleton
        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");
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
        if (debugMode) DebugMode();
        stunnedDuration -= 0.5f * Time.deltaTime;
        if (stunnedDuration > 0)
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }
    }

    private void DebugMode()
    {
        if (Input.GetKeyDown(debugBombPress1))
        {
            SpawnGravityObject();
        }
        if (Input.GetKeyDown(debugBombPress2))
        {
            SpawnDefaultBomb();
        }
        if (Input.GetKeyDown(debugBombPress3))
        {
            SpawnPlasmaObject();
        }
        if (Input.GetKeyDown(debugBombPress4))
        {
            SpawnBlinkObject();
        }
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
                            // Debug.Log("Blue Bomb Type");
                            SpawnBlinkObject(sender.identity.gameObject);
                            break;
                        case 'g':
                            // Debug.Log("Green Bomb Type");
                            SpawnPlasmaObject(sender.identity.gameObject);
                            break;
                        case 'y':
                            // Debug.Log("Yellow Bomb Type");
                            this.SpawnLaserObject(sender.identity.gameObject);
                            break;
                        case 'r':
                            // Debug.Log("Red Bomb Type");
                            SpawnBigBombObject(sender.identity.gameObject);
                            break;
                        case 'p':
                            // Debug.Log("Purple Bomb Type");
                            SpawnGravityObject(sender.identity.gameObject);
                            break;
                        case 'w':
                            // Debug.Log("White Bomb Type");
                            SpawnDefaultBomb(sender.identity.gameObject);
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

                    /// Else use the default bomb
                    // this.SpawnDefaultBomb(sender.identity.gameObject);

                    // update hud
                    this.GetComponent<PlayerInterface>().StartBombHudCooldown(defaultBombCooldown);

                    defaultBombUseTimer = 0;
                    
                }
                else // When the default bomb CD isn't up
                {
                    
                }
            }
        }
    }

    [Server]
    void SpawnDefaultBomb(GameObject placer = null)
    {
        Debug.Log("spawning bomb");
        GameObject _bomb = (GameObject) Instantiate(bomb,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        _bomb.GetComponent<ComboObject>().SetOwnerPlayer(placer);
        _bomb.GetComponent<BombObject>()._Start(placer);
        NetworkServer.Spawn(_bomb);

        eventManager.OnBombPlaced(_bomb, placer); // call event
    }

    [Server]
    void SpawnLaserObject(GameObject placer = null)
    {
        GameObject _laser = (GameObject) Instantiate(laser,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        _laser.GetComponent<ComboObject>().SetOwnerPlayer(placer);
        _laser.GetComponent<LaserObject>()._Start(placer);
        NetworkServer.Spawn(_laser);

        eventManager.OnBombPlaced(_laser, placer); // call event
    }

    [Server]
    void SpawnPlasmaObject(GameObject placer = null)
    {
        GameObject _plasma = (GameObject) Instantiate(plasma,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        _plasma.GetComponent<PlasmaObject>()._Start(placer);
        NetworkServer.Spawn(_plasma);

        eventManager.OnBombPlaced(_plasma, placer); // call event
    }

    [Server]
    void SpawnBlinkObject(GameObject placer = null)
    {
        GameObject _blink = (GameObject) Instantiate(blink,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        _blink.GetComponent<BlinkObject>()._Start(placer);
        NetworkServer.Spawn(_blink);

        eventManager.OnBombPlaced(_blink, placer); // call event
    }

    [Server]
    void SpawnGravityObject(GameObject placer = null)
    {
        GameObject _gravity = (GameObject) Instantiate(gravityObject,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        _gravity.GetComponent<SludgeObject>()._Start(placer);
        NetworkServer.Spawn(_gravity);

        eventManager.OnBombPlaced(_gravity, placer); // call event
    }

    [Server]
    void SpawnBigBombObject(GameObject placer = null)
    {
        GameObject _bigBomb = (GameObject) Instantiate(bigBomb,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        _bigBomb.GetComponent<PulseObject>()._Start(placer);  // TODO change this type
        NetworkServer.Spawn(_bigBomb);

        eventManager.OnBombPlaced(_bigBomb, placer); // call event
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

    [Command(ignoreAuthority=true)]
    void CmdSpin(NetworkConnectionToClient sender = null)
    {
        // Spin for server
        if (canSpin) StartCoroutine(WaitSpinHit(sender.identity.gameObject));
        StartCoroutine(Spin());
        RpcSpin();
    }

    [ClientRpc]
    void RpcSpin()
    {
        // Client will spin for all observers
        StartCoroutine(Spin());
    }

    // Wait to check if spin hit a bomb
    private IEnumerator WaitSpinHit(GameObject player)
    {
        Player p = player.GetComponent<Player>();

        yield return new WaitForSeconds(p.spinHitboxDuration);
        
        if (p.spinHit == null) // If did not hit, make a "spin miss" event
        {
            eventManager.OnPlayerSpin(player);
        }
        else // If did hit, make a "spin hit" event
        {
            eventManager.OnPlayerSpin(player, p.spinHit);
            p.spinHit = null;
        }
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

        // trigger character spin animation
        animator.SetTrigger("anim_SpinTrigger");
        networkAnimator.SetTrigger("anim_SpinTrigger");

        yield return new WaitForSeconds(spinAnimDuration);
        spinAnim.gameObject.SetActive(false);
        

        // reset character spin animation
        animator.ResetTrigger("anim_SpinTrigger");
        networkAnimator.ResetTrigger("anim_SpinTrigger");

        isRunAnim = true;
        isIdleAnim = true;
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

    // Apply movement to the player, using WASD or Arrow keys
    [Client]
    void ApplyMovement()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");
        if (horizontalAxis != 0 || verticalAxis != 0)
        {
            isIdleAnim = true;
            if (isRunAnim)
            {
                // client animator
                animator.ResetTrigger("anim_IdleTrigger");
                animator.SetTrigger("anim_RunTrigger");
                // network animator
                networkAnimator.ResetTrigger("anim_IdleTrigger");
                networkAnimator.SetTrigger("anim_RunTrigger");
                isRunAnim = false;
            }
        }
        else
        {
            isRunAnim = true;
            if (isIdleAnim)
            {
                // client animator
                animator.ResetTrigger("anim_RunTrigger");
                animator.SetTrigger("anim_IdleTrigger");
                // network animator
                networkAnimator.ResetTrigger("anim_RunTrigger");
                networkAnimator.SetTrigger("anim_IdleTrigger");
                isIdleAnim = false;
            }
        }

        Vector3 direction = new Vector3(this.horizontalAxis, 0f, this.verticalAxis).normalized;
        if (direction != Vector3.zero)
        {
            if (playerModel.activeSelf)
            {
                playerModel.transform.rotation = Quaternion.Slerp(
                    playerModel.transform.rotation,
                    Quaternion.LookRotation(direction),
                    turnSpeed * Time.deltaTime
                );
            } else if (ghostModel.activeSelf)
            {
                ghostModel.transform.rotation = Quaternion.Slerp(
                    ghostModel.transform.rotation,
                    Quaternion.LookRotation(direction),
                    turnSpeed * Time.deltaTime
                );
            }
            if (this.canMove) controller.Move((direction * movementSpeed * this.sludgeInfluence)
                                                        * Time.deltaTime);
            this.sludgeInfluence = 1;
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
        // if (colorKey == 'w')
        // {
        //     print("IN HERE");
        //     ApplyQueenPoints(1);
        // }
        // else 
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
        PlayerInterface hud = this.GetComponent<PlayerInterface>();
        // for (int i = 0; i < 3; i++)
        for (int i = 2; i >= 0; i--)
        {
            if (i < itemStack.Count) hud.UpdateStackHud(i, itemStack[i]);
            else hud.UpdateStackHud(i, 'e');
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
            //healthScript.SignalExit();
            this.canExitInvincibility = false;
			FindObjectOfType<AudioManager>().PlaySound("playerRespawn");

            isRunAnim = true;
            isIdleAnim = true;
        }
    }

    public void SetCanExitInvincibility(bool val)
    {
        this.canExitInvincibility = val;
    }

    public void SetSpinHitboxActive(bool val)
    {
        spinHitbox.SetActive(val);
    }

    public void ApplySludge(float val)
    {
        this.sludgeInfluence = val;
    }

    
    public void ApplyQueenPoints(int val)
    {
        this.queenPoints += val;
        print("queen points is now "+this.queenPoints);
        if (queenPoints > queenPointThreshhold)
        {
            print("queen");
            RpcBecomeQueen();
        }
    }

    public void BecomeQueen()
    {
        print("A Player has become queen!");
        this.isQueen = true;
        StartCoroutine(ToggleQueen(queenDuration, false));
    }

    public IEnumerator ToggleQueen(float delay, bool val)
    {
        yield return new WaitForSeconds(delay);
        this.isQueen = val;
    }

    [ClientRpc]
    public void RpcBecomeQueen()
    {
         BecomeQueen();       
    }
}