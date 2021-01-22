using System;
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
    public int NumLives { get; set; }

    [Header("Input")] [SerializeField] private string swapKey = "space";
    [SerializeField] private string punchKey = "p";
    [SerializeField] private string spinKey = "o";
    [SerializeField] private string bombKey = "j";
    private float horizontalAxis;
    private float verticalAxis;

    [Header("Movement")] [SerializeField] private CharacterController controller;
    [SerializeField] private float movementSpeed = 50;
    [SerializeField] private float turnSpeed = 17f;

    [Header("HexTiles")]
    [SyncVar(hook = nameof(OnChangeHeldKey))]
    public char heldKey = 'g';
    public Vector3 heldHexScale = new Vector3(800, 800, 800);
    //float swapDistance = 15; // unused (for swapping in front of player)

    public float punchCooldown = 0.5f;
    public float spinHitboxDuration = 0.6f;
    public float spinAnimDuration = 0.8f;
    public float spinTotalCooldown = 0.8f;
    private bool canPunch = true;
    private bool canSpin = true;

    private GameObject spinHitbox;
    private GameObject spinAnim;

    // Cache raycast refs for optimization
    private Ray tileRay;
    private RaycastHit tileHit;
    private GameObject selectedTile;
    private GameObject heldHexModel;

    [SerializeField] private int maxStackSize = 3;
    readonly List<char> itemStack = new List<char>();
    private GameObject stackUI;
    private Quaternion stackRotationLock;

    public override void OnStartClient()
    {

        this.Assert();
        LinkAssets();
        spinHitbox = gameObject.transform.Find("SpinHitbox").gameObject;
        spinAnim = this.gameObject.transform.Find("SpinVFX").gameObject;
        UpdateHeldHex(heldKey); // Initialize model

        if (isLocalPlayer)
        {
            stackUI.GetComponent<Text>().enabled = true;
            stackRotationLock = stackUI.transform.rotation;
        }
        base.OnStartClient();
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

        ApplyMovement();
        ApplyTileHighlight();
        ListenForSwapping();
        CmdListenForPunching();
        CmdListenForSpinning();
        CmdListenForBombUse();

        //Debug.Log(itemStack);
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;
        stackUI.transform.rotation = stackRotationLock;
    }

    [Client]
    void LinkAssets()
    {
        hexGrid = GameObject.FindGameObjectWithTag("HexGrid")
            .GetComponent<HexGrid>(); // Make sure hexGrid is created before the player
        if (!hexGrid) Debug.LogError("Player.cs: No cellPrefab found.");

        // Probably dangerous way to get game object, UI has to be first in player prefab
        stackUI = this.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
    }

    [Command]
    void CmdListenForBombUse()
    {
        RpcListenForBombUse(connectionToClient);
    }

    [TargetRpc]
    void RpcListenForBombUse(NetworkConnection target)
    {
        ListenForBombUse();
    }

    void ListenForBombUse()
    {
        // raycast down to check if tile is occupied
        if (Input.GetKeyDown(bombKey))
        {
            tileRay = new Ray(transform.position, Vector3.down * 10);

            if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
            {
                var hexCell = tileHit.transform.gameObject.GetComponentInParent<HexCell>();
                if (!hexCell.IsOccupiedByComboObject())
                {
                    if (itemStack.Count > 0)
                    {
                        char bombType = itemStack[itemStack.Count - 1]; // peek top of stack
                        itemStack.RemoveAt(itemStack.Count - 1); // pop it off
                        UpdateStackUI(); // temp - update the changed ui

                        switch (bombType)
                        {
                            case 'b':
                                Debug.Log("Blue Bomb Type");
                                // unimplemented
                                break;
                            case 'g':
                                Debug.Log("Green Bomb Type");
                                // unimplemented
                                break;
                            case 'y':
                                Debug.Log("Yellow Bomb Type");
                                StartCoroutine(this.LaserUse());
                                break;
                            case 'r':
                                Debug.Log("Red Bomb Type");
                                // unimplemented
                                break;
                            case 'p':
                                Debug.Log("Purple Bomb Type");
                                // unimplemented
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
                    } else
                    {
                        Debug.Log("dropping default bomb");
                        // Else use the default bomb
                        StartCoroutine(this.BombUse());
                    }
                }
            }
        }

        // Temp
        if (Input.GetKeyDown("k"))
        {
            tileRay = new Ray(transform.position, Vector3.down * 10);

            if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
            {
                var hexCell = tileHit.transform.gameObject.GetComponentInParent<HexCell>();
                if (!hexCell.IsOccupiedByComboObject())
                {
                    StartCoroutine(this.LaserUse());
                }
            }
        }
    }

    IEnumerator BombUse()
    {
        Instantiate(Resources.Load("Prefabs/ComboObjects/Bomb Object"),
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        yield return new WaitForSeconds(0);
    }

    //
    IEnumerator LaserUse()
    {
        Instantiate(Resources.Load("Prefabs/ComboObjects/Laser Object"),
            this.gameObject.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
        yield return new WaitForSeconds(0);
    }

    [Command]
    void CmdListenForPunching()
    {
        RpcListenForPunching(connectionToClient);
    }

    [TargetRpc]
    void RpcListenForPunching(NetworkConnection target)
    {
        ListenForPunching();
    }

    void ListenForPunching()
    {
        if (Input.GetKeyDown(punchKey))
        {
            StartCoroutine(this.Punch());
        }
    }

    [Command]
    void CmdListenForSpinning()
    {
        RpcListenForSpinning(connectionToClient);
    }

    [TargetRpc]
    void RpcListenForSpinning(NetworkConnection target)
    {
        ListenForSpinning();
    }

    public void ListenForSpinning()
    {
        if (Input.GetKeyDown(spinKey))
        {
            StartCoroutine(Spin());
        }
    }

    IEnumerator Punch()
    {
        if (canPunch)
        {
            // enable punch object for a given number of frames
            var punchHitbox = gameObject.transform.Find("PunchHitbox");
            // error check
            if (!punchHitbox)
            {
                Debug.LogError("Player.cs: no punchHitbox assigned");
            }
            else
            {
                canPunch = false;
                punchHitbox.gameObject.SetActive(true);
                yield return new WaitForSeconds(punchCooldown);
                canPunch = true;
                punchHitbox.gameObject.SetActive(false);
            }
        }
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

                canSpin = false;
                yield return new WaitForSeconds(spinTotalCooldown);
                canSpin = true;
            }
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
            transform.position + transform.up * 18 + transform.forward * 10,
            transform.rotation,
            transform
        );

        this.heldHexModel.gameObject.transform.localScale = heldHexScale;
    }

    // Apply movement to the player, using WASD or Arrow keys
    [Client]
    void ApplyMovement()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(this.horizontalAxis, 0f, this.verticalAxis).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                turnSpeed * Time.deltaTime
            );

            controller.Move(direction * movementSpeed * Time.deltaTime);
        }
    }

    // Listens for key press and swaps the tile beneath the player
    [Client]
    void ListenForSwapping()
    {
        if (Input.GetKeyDown(swapKey))
        {
            tileRay = new Ray(transform.position, Vector3.down * 10);

            if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
            {
                Debug.Log("space pressed");
                GameObject modelHit = tileHit.transform.gameObject;
                //HexCell hexCell = modelHit.GetComponentInParent<HexCell>();
                char newKey = modelHit.GetComponentInParent<HexCell>().GetKey();

                hexGrid.SwapHexAndKey(modelHit, getHeldKey(), this.netIdentity);
                //CmdSwapHexAndKey(modelHit, getHeldKey());


                // Only update models and grids if it is a new key
                if (!this.heldKey.Equals(newKey))
                {
                    CmdSetHeldKey(newKey);
                }
            }
        }
    }

    [Command]
    void CmdSwapHexAndKey(GameObject modelHit, char heldKey, NetworkConnectionToClient sender = null)
    {
        hexGrid.SwapHexAndKey(modelHit, heldKey, sender.identity);
    }

    // Applies the highlight shader to the tile the player is "looking" at
    // This is the tile that will be swapped, and one where the bomb will be placed on
    [Client]
    void ApplyTileHighlight()
    {
        tileRay = new Ray(transform.position, Vector3.down * 10);

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

    public void AddItemCombo(char colorKey)
    {
        CmdAddItemCombo(colorKey);
    }

    [Command]
    void CmdAddItemCombo(char colorKey)
    {
        RpcAddItemCombo(colorKey);
    }

    [ClientRpc]
    void RpcAddItemCombo(char colorKey)
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

        // Changing the UI
        UpdateStackUI();
    }


    // Temporary function, should be replaced
    void UpdateStackUI()
    {
        string stackDisplay = "[";
        for (int i = 0; i < 3; i++)
        {
            if (i < itemStack.Count) stackDisplay += itemStack[i];
            else stackDisplay += "-";

            if (i < 2) stackDisplay += ", ";
            else stackDisplay += "]";
        }
        stackUI.GetComponent<Text>().text = stackDisplay;
    }

    [Command]
    void CmdSetHeldKey(char newKey)
    {
        RpcSetHeldKey(newKey);
    }

    [ClientRpc]
    void RpcSetHeldKey(char newKey)
    {
        setHeldKey(newKey); // Sync held key
        UpdateHeldHex(newKey); // Update model for all observers
    }

    public void setHeldKey(char key)
    {
        this.heldKey = key;
    }

    public char getHeldKey()
    {
        return this.heldKey;
    }
}