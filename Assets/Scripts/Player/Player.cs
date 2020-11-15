using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using Castle.Components.DictionaryAdapter.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NetworkBehaviour = Mirror.NetworkBehaviour;

public class Player : NetworkBehaviour
{

    private HexGrid hexGrid;
    private HexCell cellPrefab;
    
    public CharacterController controller;
    public bool useArrowKeys = false;
    public float movementSpeed = 50;
    public float turnSmoothness = 0.1f;
    float turnSmoothVelocity;

    public char heldKey = 'g';

    // Cache raycast refs for optimization
    private Ray tileRay;
    private RaycastHit tileHit;

    private GameObject heldHexModel;
     float swapDistance = 15;
    public Vector3 heldHexScale = new Vector3(800, 800, 800);

    private void Start()
    {
        LinkAssets();
        UpdateHeldHex();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        ApplyMovement();
        ListenForSwapping();

        // Debug ray for swapping
        Debug.DrawRay(transform.position + transform.forward * swapDistance + transform.up * 5, Vector3.down * 10, Color.green);
        //Debug.Log(heldHexModel.transform.position);
    }

    protected virtual void LinkAssets()
    {
        cellPrefab = Resources.Load<HexCell>(String.Concat("Prefabs/", HexMetrics.GetHexCellPrefabName()));  // The return type is enclosed by <>
        if (!cellPrefab) Debug.LogError("Player.cs: No cellPrefab found.");

        hexGrid = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();  // Make sure hexGrid is created before the player
        if (!hexGrid) Debug.LogError("Player.cs: No cellPrefab found.");
    }

    protected virtual void UpdateHeldHex()
    {
        if (this.heldHexModel)
        {
            Destroy(this.heldHexModel, 0f);
            Debug.Log("Destroyed held hex");
        }

        // Create the hex model in the player's hand
        this.heldHexModel = cellPrefab.createModel
            (
                hexGrid.ReturnModelByCellKey(heldKey),
                transform.position + transform.up * 18 + transform.forward * 10,
                transform.rotation,
                transform
            );

        this.heldHexModel.gameObject.transform.localScale = heldHexScale;
    }
 
    // Apply movement to the player, using WASD or Arrow keys
    protected virtual void ApplyMovement()
    {

        // By default, use WASD keys for input
        Vector3 direction = new Vector3(
            Input.GetAxisRaw("HorizontalPlayer1"),
            0f,
            Input.GetAxisRaw("VerticalPlayer1")
        ).normalized;

        // If enabled, use arrow keys for input instead
        if (useArrowKeys)
        {
            // Player 1 movement (WASD)
            direction = new Vector3(
                Input.GetAxisRaw("HorizontalPlayer2"),
                0f,
                Input.GetAxisRaw("VerticalPlayer2")
            ).normalized;

        }

        // Movement and rotations
        if (direction.magnitude >= 0.1f)
        {
            float angleP1 = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg,
                ref turnSmoothVelocity,
                turnSmoothness);
            transform.rotation = Quaternion.Euler(0f, angleP1, 0f);

            controller.Move(direction * movementSpeed * Time.deltaTime);
        }
    }

    // Listens for key press and swaps the tile beneath the player
    protected virtual void ListenForSwapping()
    {

        tileRay = new Ray(transform.position + transform.forward * swapDistance + transform.up * 5, Vector3.down * 10);

        if (Physics.Raycast(tileRay, out tileHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            // Debug.Log("hit");

            // TODO: Apply shader or some indicator to hex tile to show the tile selected

            // When swap key is pressed, swap held tile with selected tile
            if (Input.GetKeyDown("space"))
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