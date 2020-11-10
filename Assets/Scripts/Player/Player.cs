using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Player : MonoBehaviour
{

    public CharacterController controller;
    public float movementSpeed = 50;
    public float turnSmoothness = 1f;
    float turnSmoothVelocity;

    public bool useArrowKeys = false;

    public HexGrid hexGrid;
    public Text testHeldKeyUI;
    public char heldKey = 'g';

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        ApplyMovement();
        // ApplyGravity(); // Disabled, player clips on destroyed hex nodes
        ListenForSwapping();
    }

    // Apply movement to the player, using WASD or Arrow keys
    void ApplyMovement()
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
    void ListenForSwapping()
    {
        // If space key is pressed
        if (Input.GetKeyDown("space"))
        {
            RaycastHit hit;
            
            // Check the tile directly under the player
            if ((Physics.Raycast(transform.position, Vector3.down, out hit, 10f, 1 << LayerMask.NameToLayer("BaseTiles")) ))
            {
                GameObject modelHit = hit.transform.gameObject;
                // Debug.Log("found color: " +(modelHit.GetComponentInParent<HexCell>().getKey()));
                HexCell hexCell = modelHit.GetComponentInParent<HexCell>();
                this.heldKey = hexGrid.SwapHex(modelHit, this.heldKey); // A coroutine because input activates quickly.
                hexCell.isIn3ComboAnywhere(hexGrid.RegenerateTiles);

                // Set UI to new held tile
                testHeldKeyUI.GetComponent<HeldKey>().setText(this.heldKey);
            }
        }
    }
}