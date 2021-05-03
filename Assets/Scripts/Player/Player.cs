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
    public bool debugMode = false;
    public string debugBombPress1 = "8";
    public string debugBombPress2 = "9";
    public string debugBombPress3 = "e";
	public string debugBombPress4 = ";";
	public string debugGroundItemSpawn = "g";
    private HexGrid hexGrid;
    public bool isStunned = false;
    public float stunnedDuration = 0;

    [Header("Respawn")]
    public float invincibilityDuration = 2.0f;
    public float ghostDuration = 5.0f;
    [SerializeField] public Health healthScript = null;
    

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
    public GameObject spinPVP = null;

    // Game Objects
    [Header("Required", order = 2)] public GameObject bomb;
    public GameObject laser;
    public GameObject plasma;
    public GameObject bigBomb;
    public GameObject blink;
    public GameObject gravityObject;
	public GameObject groundItemPickupHitbox;
	private float slowScalar = 1f;
    public float timeSinceSlowed = 0f;
    private float sludgedScalar = 1f;
    private float timeSinceSludged = 0f;
    private float sludgedDuration = 0f;  // set by the sludge object
    private float sludgeEndAnim = -40f;
    // private float timeSinceSludgedEnd = -40f;
    public float timeSinceSludgedEndDur = 0f;
    private bool sludgeEffectEnded = true;
    private bool sludgeEffectStarted = false;

    public float slowTimeCheckInterval = 0.05f;
    public GameObject sludgeVFX;

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
    public GameObject selectedTile;
    private GameObject heldHexModel;

    [SerializeField] private int maxStackSize = 3;

    private GameObject playerMesh;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject ghostModel;
    [NonSerialized] public Quaternion rotation;
    [SerializeField] private GameObject highlightModel;

    public bool isDead = false; // when player has lost ALL lives
    //public bool isFrozen = true; // cannot move, but can rotate

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
        this.Assert();
        LinkAssets();
        spinHitbox = gameObject.transform.Find("SpinHitbox").gameObject;
        spinPVP = gameObject.transform.Find("SpinPVP").gameObject;
        spinAnim = this.gameObject.transform.Find("SpinVFX").gameObject;
        UpdateHeldHex(heldKey); // Initialize model

        // Set player color
        playerMesh = playerModel.transform.GetChild(0).gameObject;
        playerMesh.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", playerColor);
        // Disable tile highlight outline
        if (isLocalPlayer) highlightModel.active = true;
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
        // TODO: Delete later
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SpawnDefaultBomb();
        }
        
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

            sludgedScalar = 1.0f;
            if (sludgeVFX.activeSelf)
            {
                sludgeVFX.SetActive(false);
                this.canSpin = true;
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

        this.transform.position = new Vector3(this.transform.position.x, fixedY, this.transform.position.z);
        

        ApplyMovement();
        ApplyTileHighlight();
        ListenForSwapping();
        ListenForBombUse();
        ListenForSpinning();
        ListenForBombRotation();
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

        //Debug.Log("tme sine cludge" + timeSinceSludged);


        if (this.timeSinceSlowed > slowTimeCheckInterval)
        {
            slowScalar = 1.0f;
        }
    }

    [Command] public void CmdSetSludgeEffectEnded(bool cond)
    {
        RpcSetSludgeEffectEnded(cond);
    }

    [ClientRpc] public void RpcSetSludgeEffectEnded(bool cond)
    {
        sludgeEffectEnded = cond;
    }

    private void DebugMode()
    {
        if (Input.GetKeyDown(debugBombPress1))
        {
            hexGrid.GrowRing();
        }
        if (Input.GetKeyDown(debugBombPress2))
        {
            SpawnLaserObject();
        }
        if (Input.GetKeyDown(debugBombPress3))
        {
            SpawnPlasmaObject();
        }
        if (Input.GetKeyDown(debugBombPress4))
        {
            SpawnBlinkObject();
        }
		if (Input.GetKeyDown(debugGroundItemSpawn))
		{
			Debug.Log("G pressed in Debug mode");
			healthScript.CmdDropItems();
			

		}
    }

    // Update version for server
    [ServerCallback]
    private void LateUpdate()
    {
        if (!isServer) return;

        // Handle default-placing bomb anim
        //if (!playedOnDefaultBombReadyAnim && defaultBombUseTimer > defaultBombCooldown)
        //{
        //    Debug.Log("inside");
        //    RpcEnableBombPlaceAnimation();
        //    playedOnDefaultBombReadyAnim = true;
        //}
        //defaultBombUseTimer += Time.deltaTime;
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
        if (KeyBindingManager.GetKeyDown(KeyAction.Place) && (this.canExitInvincibility || this.canPlaceBombs))
        {
            ExitInvincibility();
            CmdBombUse();
        }
    }

    public int spinPower = 1;
    private float startSpinTime = 0f;
    public float spinChargeTime = 0f;

    [SyncVar] public bool spinHeld = false;
    [Command] private void CmdSetSpinHeld(bool held) { spinHeld = held; }

    [SerializeField] public float[] spinTimings = {0.5f, 1.0f, 1.5f, 2.0f};
    // [SerializeField] public float[] spinTimings = {0.0f, 0.5f, 1.0f, 1.5f, 2.0f};
    [SerializeField] private int[] spinPowerDist = {1, 2, 3, 4};
    [SerializeField] private float spinScalar = 1f;

    // trash variables sry
    private bool spinChargeLevel0Hit = false; // for initial hit
    private bool spinChargeLevel1Hit = false;
    private bool spinChargeLevel2Hit = false;
    private bool spinChargeLevel3Hit = false;

    [Client]
    public void ListenForSpinning()
    {
        if (!canSpin) return;

        // When key is pressed down
        if (KeyBindingManager.GetKey(KeyAction.Spin) && spinChargeTime < spinTimings[spinTimings.Length - 1]) {
            if (!spinHeld)
            {
                spinScalar = 0.5f;
                startSpinTime = Time.time;

                spinChargeLevel0Hit = false;
                spinChargeLevel1Hit = false;
                spinChargeLevel2Hit = false;
                spinChargeLevel3Hit = false;

                CmdSetSpinHeld(true);
            }

            spinChargeTime += Time.deltaTime;


            // HERE

            if (!spinChargeLevel0Hit)
            {
                this.GetComponent<PlayerInterface>().spinChargeBar.transform.parent.gameObject.GetComponent<ScaleTween>().StartTween();
                // CmdSetSpinChargeFlashEffect(10f, 0.2f);
                spinChargeLevel0Hit = true;
            }


            // Play anims and sounds
            if (!spinChargeLevel1Hit && spinChargeTime > spinTimings[0])
            {
                FindObjectOfType<AudioManager>().PlaySound("spinCharge2");
                this.GetComponent<PlayerInterface>().spinChargeBar.transform.parent.gameObject.GetComponent<ScaleTween>().StartTween();
                CmdSetSpinChargeFlashEffect(10f, 0.4f);
                spinChargeLevel1Hit = true;
            }

            if (!spinChargeLevel2Hit && spinChargeTime > spinTimings[1])
            {
                FindObjectOfType<AudioManager>().PlaySound("spinCharge3");
                this.GetComponent<PlayerInterface>().spinChargeBar.transform.parent.gameObject.GetComponent<ScaleTween>().StartTween();
                CmdSetSpinChargeFlashEffect(15f, 0.8f);
                spinChargeLevel2Hit = true;
            }

            if (!spinChargeLevel3Hit && spinChargeTime >= spinTimings[2])
            {
                FindObjectOfType<AudioManager>().PlaySound("spinCharge4");
                this.GetComponent<PlayerInterface>().spinChargeBar.transform.parent.gameObject.GetComponent<ScaleTween>().StartTween();
                CmdSetSpinChargeFlashEffect(20f, 1.2f);
                spinChargeLevel3Hit = true;
            }
        }

        // When key is let go
        if (KeyBindingManager.GetKeyUp(KeyAction.Spin))
        {
            for (int i = 0; i < spinTimings.Length; i++)
            {
                // If maximum power, dont need to check timing
                if (i == spinTimings.Length - 1)
                {
                    spinPower = spinPowerDist[i];
                    break;
                }

                // Set power in ascending order
                if (spinChargeTime < spinTimings[i])
                {
                    spinPower = spinPowerDist[i];
                    break;
                }
            }
            ExitInvincibility();
            CmdSpin(spinPower);

            ResetSpinCharge();
        }

    }

    [Command] public void CmdSetSpinChargeFlashEffect(float flashSpeed, float glowAmt)
    {
        RpcSetSpinChargeFlashEffect(flashSpeed, glowAmt);
    }

    [ClientRpc] public void RpcSetSpinChargeFlashEffect(float flashSpeed, float glowAmt)
    {
        playerMesh.GetComponent<Renderer>().material.SetFloat("_FlashSpeed", flashSpeed);
        playerMesh.GetComponent<Renderer>().material.SetFloat("_GlowAmount", glowAmt);
    }

    public void ResetSpinCharge()
    {
        CmdSetSpinChargeFlashEffect(0f, 0f);
        spinScalar = 1f;
        spinChargeTime = 0f;
        CmdSetSpinHeld(false);
    }

    [Command]
    void CmdBombUse(NetworkConnectionToClient sender = null)
    {
        BombUse(sender);
    }

    void BombUse(NetworkConnectionToClient sender)
    {
        Ray tileRay = new Ray(transform.position + transform.up * 5, Vector3.down * 10);

        if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            var hexCell = tileHit.transform.gameObject.GetComponentInParent<HexCell>();
            if (!hexCell.IsOccupiedByComboObject())
            {
                // Get the players inventory who called this function
                PlayerInventory inv = sender.identity.GetComponent<PlayerInventory>();

                char bombType = inv.GetSelectedBombType(); // get the currently selected bomb type

                // if (bombType == 'e') return; // removed for new combo tiles
                if (bombType == 'e' || bombType == '1'|| bombType == '2'|| bombType == '3' || bombType == '4')
                    return; // if selected bomb type empty, return

                inv.RemoveInventoryBomb(bombType); // Subtract the bomb type from the player inventory by 1

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
                        SpawnLaserObject(sender.identity.gameObject);
                        break;
                    case 'r':
                        // Debug.Log("Red Bomb Type");
                        SpawnDefaultBomb(sender.identity.gameObject);
                        break;
                    case 'p':
                        // Debug.Log("Purple Bomb Type");
                        SpawnSludgeObject(sender.identity.gameObject);
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
        }
    }

    [Server]
    void SpawnDefaultBomb(GameObject placer = null)
    {
        GameObject _bomb = (GameObject)Instantiate(bomb,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_bomb);
        // _bomb.GetComponent<BombObject>()._Start(placer);
        RpcSpawnDefaultBomb(_bomb, placer);

        eventManager.OnBombPlaced(_bomb, placer); // call event
    }

    [ClientRpc]
    void RpcSpawnDefaultBomb(GameObject bomb, GameObject placer)
    {
        bomb.GetComponent<BombObject>()._Start(placer);
    }

    [Server]
    void SpawnLaserObject(GameObject placer = null)
    {
        GameObject _laser = (GameObject) Instantiate(laser,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_laser);
        // _laser.GetComponent<LaserObject>()._Start(placer);
        RpcSpawnLaserObject(_laser, placer);

        eventManager.OnBombPlaced(_laser, placer); // call event
    }

    [ClientRpc]
    void RpcSpawnLaserObject(GameObject laser, GameObject placer)
    {
        laser.GetComponent<LaserObject>()._Start(placer);
    }

    [Server]
    void SpawnPlasmaObject(GameObject placer = null)
    {
        GameObject _plasma = (GameObject) Instantiate(plasma,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_plasma);
        // _plasma.GetComponent<PlasmaObject>()._Start(placer);
        RpcSpawnPlasmaObject(_plasma, placer);
        
        eventManager.OnBombPlaced(_plasma, placer); // call event
    }

    [ClientRpc]
    void RpcSpawnPlasmaObject(GameObject plasma, GameObject placer)
    {
        plasma.GetComponent<PlasmaObject>()._Start(placer);
    }

    [Server]
    void SpawnBlinkObject(GameObject placer = null)
    {
        GameObject _blink = (GameObject) Instantiate(blink,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_blink);
        _blink.GetComponent<BlinkObject>()._Start(placer);
        // RpcSpawnBlinkObject(_blink, placer);

        eventManager.OnBombPlaced(_blink, placer); // call event
    }

    [ClientRpc]
    void RpcSpawnBlinkObject(GameObject blink, GameObject placer)
    {
        blink.GetComponent<BlinkObject>()._Start(placer);
    }

    [Server]
    void SpawnSludgeObject(GameObject placer = null)
    {
        GameObject _sludge = (GameObject) Instantiate(gravityObject,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        NetworkServer.Spawn(_sludge);
        // _sludge.GetComponent<SludgeObject>()._Start(placer);
        RpcSpawnSludgeBomb(_sludge, placer);
        
        eventManager.OnBombPlaced(_sludge, placer); // call event
    }

    [ClientRpc]
    void RpcSpawnSludgeBomb(GameObject sludge, GameObject placer)
    {
        sludge.GetComponent<SludgeObject>()._Start(placer);
    }

    [Server]
    void SpawnPulseObject(GameObject placer = null)
    {
        GameObject _pulse = (GameObject) Instantiate(bigBomb,
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        // _pulse.GetComponent<PulseObject>()._Start(placer); // TODO change this type
        NetworkServer.Spawn(_pulse);

        RpcSpawnPulseObject(_pulse, placer);

        eventManager.OnBombPlaced(_pulse, placer); // call event    }
    }

    [ClientRpc]
    void RpcSpawnPulseObject(GameObject pulse, GameObject placer)
    {
        pulse.GetComponent<PulseObject>()._Start(placer);
    }

    public void SetCanPlaceBombs(bool val)
    {
        this.canPlaceBombs = val;
    }

    public void SetCanSpin(bool val)
    {
        this.canSpin = val;
    }

    public IEnumerator Spin(int spinPower)
    {
        if (canSpin)
        {
            if (!spinHitbox)
            {
                Debug.LogError("Player.cs: no spinHitbox assigned");
            }
            else
            {
                this.spinPower = spinPower;
                StartCoroutine(HandleSpinHitbox());
                StartCoroutine(HandleSpinAnim());
                FindObjectOfType<AudioManager>().PlaySound("playerSpin");
                canSpin = false;
                yield return new WaitForSeconds(spinTotalCooldown);
                if (sludgeEffectEnded) canSpin = true;
            }
        }
    }

    [Command(ignoreAuthority=true)]
    void CmdSpin(int spinPower, NetworkConnectionToClient sender = null)
    {
        // Spin for server
        if (canSpin) StartCoroutine(WaitSpinHit(sender.identity.gameObject));
        StartCoroutine(Spin(spinPower));
        RpcSpin(spinPower);
    }

    [ClientRpc]
    void RpcSpin(int spinPower)
    {
        // Client will spin for all observers
        StartCoroutine(Spin(spinPower));
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

        spinPVP.gameObject.SetActive(true);
        yield return new WaitForSeconds(spinHitboxDuration);
        spinPVP.gameObject.SetActive(false);
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


        this.timeSinceSlowed += Time.deltaTime;
        timeSinceSludged -= Time.deltaTime;
        Vector3 direction = new Vector3(this.horizontalAxis, 0f, this.verticalAxis).normalized;
        if (direction != Vector3.zero)
        { 
            rotation = Quaternion.Slerp(
                playerModel.transform.rotation,
                Quaternion.LookRotation(direction),
                turnSpeed * Time.deltaTime);
            if (playerModel.activeSelf)
            {
                playerModel.transform.rotation = rotation;
            } else if (ghostModel.activeSelf)
            {
                ghostModel.transform.rotation = rotation;
            }

            controller.Move(direction * movementSpeed * slowScalar * sludgedScalar * spinScalar * Time.deltaTime);
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

    public void ApplySpeedScalar(float val)
    {
        this.slowScalar *= val;
    }
    
    public void ApplySludgeSlow(float slowRate, float slowDur)
    {
        if (isServer) RpcApplySludgeSlow(slowRate, slowDur);
        else CmdApplySludgeSlow(slowRate, slowDur);

        ResetSpinCharge();
        this.canSpin = false;
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
        this.sludgedScalar = slowFactor;
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

    public void SetSpeedScalar(float val)
    {
        this.slowScalar = val;
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