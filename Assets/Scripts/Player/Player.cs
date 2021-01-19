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

    public float punchCooldown = 0.5f;
    public float spinHitboxDuration = 0.6f;
    public float spinAnimDuration = 0.8f;
    public float spinTotalCooldown = 0.8f;
    private bool canPunch = true;
    private bool canSpin = true;
    
    private GameObject spinHitbox;
    private GameObject spinAnim; 

    [Header("HexTiles")] [SyncVar(hook = nameof(OnChangeHeldKey))]
    public char heldKey = 'g';

    // Cache raycast refs for optimization
    private Ray tileRay;
    private RaycastHit tileHit;

    private GameObject selectedTile;
    private GameObject heldHexModel;
    float swapDistance = 15;
    public Vector3 heldHexScale = new Vector3(800, 800, 800);


    public override void OnStartClient()
    {
        this.Assert();
        LinkAssets();
        spinHitbox = gameObject.transform.Find("SpinHitbox").gameObject;
        spinAnim = this.gameObject.transform.Find("SpinVFX").gameObject;
        // Initialize model
        UpdateHeldHex(heldKey);
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

        // Applies player movement
        ApplyMovement();
        // Listens for swapping and highlights selected hex
        ListenForSwapping();
        CmdListenForPunching();
        CmdListenForSpinning();
        CmdListenForBombUse();
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;
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
                    StartCoroutine(this.BombUse());
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

    [Client]
    void LinkAssets()
    {
        hexGrid = GameObject.FindGameObjectWithTag("HexGrid")
            .GetComponent<HexGrid>(); // Make sure hexGrid is created before the player
        if (!hexGrid) Debug.LogError("Player.cs: No cellPrefab found.");
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
        tileRay = new Ray(transform.position + transform.forward * swapDistance + transform.up * 5, Vector3.down * 10);

        if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            // Debug.Log("hit");

            // Apply indicator to hex tile to show the tile selected
            if (selectedTile)
                selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 0f);
            selectedTile = tileHit.transform.gameObject;
            selectedTile.GetComponent<Renderer>().material.SetFloat("Boolean_11CD7E77", 1f);

            // When swap key is pressed, swap held tile with selected tile
            if (Input.GetKeyDown(swapKey))
            {
                Debug.Log("space pressed");
                GameObject modelHit = tileHit.transform.gameObject;
                //HexCell hexCell = modelHit.GetComponentInParent<HexCell>();
                char newKey = hexGrid.SwapHexAndKey(modelHit, getHeldKey());


                // Only update models and grids if it is a new key
                if (!this.heldKey.Equals(newKey))
                {
                    CmdSetHeldKey(newKey);
                }
            }
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
        setHeldKey(newKey);
        UpdateHeldHex(newKey);
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