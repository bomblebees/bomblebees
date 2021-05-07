using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerMovement : NetworkBehaviour
{
    [Header("Required")]
    [SerializeField] private GameObject ghostModel;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private CharacterController controller;

    private float horizontalAxis;
    private float verticalAxis;

    private Quaternion rotation;

    [HideInInspector] public float sludgedScalar = 1f;

    /// <summary>
    /// The current movement multiplier applied by the spin charge effect.
    /// </summary>
    [HideInInspector] public float spinChargedScalar = 1f;

    private float fixedY = 7f;

    [Header("Speeds")] 
    [SerializeField] private float movementSpeed = 50f;
    [SerializeField] private float turnSpeed = 17f;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Subscribe to damage events
        this.GetComponent<Health>().EventGhostExit += OnGhostExit;
    }

    // Reset anims when we exit ghost to prevent T posing
    private void OnGhostExit(bool _) { playingRunAnim = false; playingIdleAnim = false; }

    // Update is called once per frame
    void Update()
    {
        // Code after this point is run only on the local player
        if (!isLocalPlayer || this.GetComponent<Player>().isEliminated) return;

        this.transform.position = new Vector3(this.transform.position.x, fixedY, this.transform.position.z);

        ApplyMovement();
    }

    /// <summary>
    /// Moves the character when keys are pressed
    /// </summary>
    [Client] public void ApplyMovement()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        if (horizontalAxis != 0 || verticalAxis != 0)
        {
            // Play move animation if player model is active
            if (playerModel.activeSelf) PlayMovementAnimation();
        }
        else
        {
            // Play idle animation if player model is active
            if (playerModel.activeSelf) PlayIdleAnimation();
        }


        //this.timeSinceSlowed += Time.deltaTime;
        //timeSinceSludged -= Time.deltaTime;

        Vector3 direction = new Vector3(this.horizontalAxis, 0f, this.verticalAxis).normalized;

        if (direction != Vector3.zero)
        {

            // Rotate player or ghost model, which ever is active
            if (playerModel.activeSelf) 
            {
                // Calculate rotation for the player
                rotation = Quaternion.Slerp(
                    playerModel.transform.rotation,
                    Quaternion.LookRotation(direction),
                    turnSpeed * Time.deltaTime);

                playerModel.transform.rotation = rotation;
            } else if (ghostModel.activeSelf)
            {
                // Calculate rotation for the ghost
                rotation = Quaternion.Slerp(
                    ghostModel.transform.rotation,
                    Quaternion.LookRotation(direction),
                    turnSpeed * Time.deltaTime);

                ghostModel.transform.rotation = rotation;
            }

            // Move the player
            controller.Move(direction * movementSpeed * sludgedScalar * spinChargedScalar * Time.deltaTime);
        }
    }

    #region Animations
    [HideInInspector] public bool playingRunAnim = false;
    [HideInInspector] public bool playingIdleAnim = false;

    [Client] private void PlayMovementAnimation()
    {
        // If not already playing, then play run anim
        if (!playingRunAnim)
        {
            // Stop idle anim, and start run anim
            this.GetComponent<NetworkAnimator>().ResetTrigger("anim_IdleTrigger");
            this.GetComponent<NetworkAnimator>().SetTrigger("anim_RunTrigger");

            // Now playing run anim
            playingRunAnim = true;

            // No longer playing idle anim
            playingIdleAnim = false;
        }
    }

    [Client] private void PlayIdleAnimation()
    {
        // If not already playing, then play idle anim
        if (!playingIdleAnim)
        {
            // Stop run anim, and start idle anim
            this.GetComponent<NetworkAnimator>().ResetTrigger("anim_RunTrigger");
            this.GetComponent<NetworkAnimator>().SetTrigger("anim_IdleTrigger");

            // Now playing idle anim
            playingIdleAnim = true;

            // No longer playing run anim
            playingRunAnim = false;
        }
    }

    #endregion
}
