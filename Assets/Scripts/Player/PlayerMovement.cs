using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed;
    public float turnSmoothness = 1f;
    private PhotonView PV;
    private Vector3 gravityVector = new Vector3(0f, -0.1f, 0f);  // currently not used
    public bool forOfflineTesting = false;

    float turnSmoothVelocity;

    public bool useArrowKeys = false;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!forOfflineTesting)
        {
            if (PV.IsMine)
            {
                UpdateLoop();
            }
        }
        else 
                UpdateLoop();
    }

    void UpdateLoop()
    {
        ApplyMovement();
    }

    void runGravity()
    {
        if (!controller.isGrounded){
            controller.Move(gravityVector);
        }
    }

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

                controller.Move(direction * speed * Time.deltaTime);
            }
    }
}