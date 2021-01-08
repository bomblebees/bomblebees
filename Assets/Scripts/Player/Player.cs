using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using Castle.Components.DictionaryAdapter.Xml;
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
    private HexCell cellPrefab;
	public int NumLives { get; set; }

    [Header("Input")]
    [SerializeField] private bool isPlayer2 = false;
    [SerializeField] private string swapKey = "space";
    [SerializeField] private string punchKey = "p";
    private float horizontalAxis;
    private float verticalAxis;

    [Header("Movement")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private float movementSpeed = 50;
    [SerializeField] private float turnSpeed = 17f;

    public float punchDuration = 0.5f;
	private bool canPunch = true;


    [Header("HexTiles")]
    public char heldKey = 'g';

    // Cache raycast refs for optimization
    private Ray tileRay;
    private RaycastHit tileHit;

    private GameObject selectedTile;
    private GameObject heldHexModel;
    float swapDistance = 15;
    public Vector3 heldHexScale = new Vector3(800, 800, 800);

    private void Start()
    {
        //LinkAssets();
        //UpdateHeldHex();
    }

    [ClientRpc]
    void RpcStart()
    {
        LinkAssets();
        UpdateHeldHex();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Input.GetKeyDown("2"))
        {
            RpcStart(); // temporary method
        }
        
        if (!hasAuthority) return;
        
        CmdGetPlayerInputs();
        CmdApplyMovement();
        CmdListenForSwapping();

        //Debug.DrawRay(transform.position + transform.forward * swapDistance + transform.up * 5, Vector3.down * 10, Color.green);
        //Debug.Log(heldHexModel.transform.position);
    }
    
    private void LateUpdate()
    {
        if (!hasAuthority) return;
        
        CmdListenForPunching();
    }
    
    [Command]
    void CmdGetPlayerInputs()
    {
        RpcPlayerInputs(connectionToClient);
    }

    [TargetRpc]
    void RpcPlayerInputs(NetworkConnection target)
    {
        GetPlayerInputs();
    }

    [Command]
    void CmdApplyMovement()
    {
        RpcApplyMovement(connectionToClient);
    }

    [TargetRpc]
    void RpcApplyMovement(NetworkConnection target)
    {
        ApplyMovement();
    }

    [Command]
    void CmdListenForSwapping()
    {
        Debug.Log("CmdListenForSwapping");
        RpcListenForSwapping(connectionToClient);
    }

    [TargetRpc]
    void RpcListenForSwapping(NetworkConnection target)
    {
        ListenForSwapping();
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

    IEnumerator Punch()
    {
        if (canPunch)
        {
            // enable punch object for a given number of frames
            var punchHitbox = gameObject.transform.Find("PunchHitbox");
            if (!punchHitbox)
            {
                Debug.LogError("punchHitbox collider not found");
            }
            else
            {
                canPunch = false;
                punchHitbox.gameObject.SetActive(true);
                yield return new WaitForSeconds(punchDuration);
                canPunch = true;
                punchHitbox.gameObject.SetActive(false);
            }
        }
    }

    void LinkAssets()
    {
        cellPrefab =
            Resources.Load<HexCell>(String.Concat("Prefabs/",
                HexMetrics.GetHexCellPrefabName())); // The return type is enclosed by <>
        if (!cellPrefab) Debug.LogError("Player.cs: No cellPrefab found.");

        hexGrid = GameObject.FindGameObjectWithTag("HexGrid")
            .GetComponent<HexGrid>(); // Make sure hexGrid is created before the player
        if (!hexGrid) Debug.LogError("Player.cs: No cellPrefab found.");
    }

    void GetPlayerInputs()
    {
        if (!isPlayer2)
        {
            horizontalAxis = Input.GetAxisRaw("HorizontalPlayer1");
            verticalAxis = Input.GetAxisRaw("VerticalPlayer1");
        } else
        {
            horizontalAxis = Input.GetAxisRaw("HorizontalPlayer2");
            verticalAxis = Input.GetAxisRaw("VerticalPlayer2");
        }
    }

    void UpdateHeldHex()
    {
        if (this.heldHexModel)
        {
            Destroy(this.heldHexModel, 0f);
            Debug.Log("Destroyed held hex");
        }

        // Create the hex model in the player's hand
        this.heldHexModel = cellPrefab.CreateModel
        (
            hexGrid.ReturnModelByCellKey(heldKey),
            transform.position + transform.up * 18 + transform.forward * 10,
            transform.rotation,
            transform
        );

        this.heldHexModel.gameObject.transform.localScale = heldHexScale;
    }

    // Apply movement to the player, using WASD or Arrow keys
    void ApplyMovement()
    {
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
                HexCell hexCell = modelHit.GetComponentInParent<HexCell>();
                char newKey = hexGrid.SwapHexAndKey(modelHit, getHeldKey());


                // Only update models and grids if it is a new key
                if (!this.heldKey.Equals(newKey))
                {
                    setHeldKey(newKey);
                    UpdateHeldHex();
                }
            }
        }
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