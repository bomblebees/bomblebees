using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSpin : NetworkBehaviour
{
    [Header("Required")]
    [SerializeField] private GameObject playerMesh;

    /// <summary>
    /// Whether the player can spin.
    /// </summary>
    [HideInInspector] public bool canSpin = true;

    /// <summary>
    /// Whether the spin charging is currently held.
    /// Also used to sync charge bars
    /// </summary>
    [HideInInspector] [SyncVar] public bool spinHeld = false;
    [Command] private void CmdSetSpinHeld(bool held) { spinHeld = held; }

    [Header("Spin Charge")]
    public int spinPower = 1;
    private float spinChargeTime = 0f;

    /// <summary>
    /// The timings in seconds required to reach a specific spin charge level.
    /// </summary>
    [SerializeField] public float[] spinTimings = { 0.5f, 1.0f, 1.5f, 2.0f };

    /// <summary>
    /// The spin power of each spin level. The number represents the distance in hexes
    /// a bomb will travel once it is spun on.
    /// </summary>
    [SerializeField] private int[] spinPowerDist = { 1, 2, 3, 4 };

    /// <summary>
    /// The current spin charge level when spin charge is held.
    /// A value of negative 1 means that the charge is not being held
    /// </summary>
    private int currentChargeLevel = 0;

    [Header("Spin Charge Flash Effects")]
    [SerializeField] private float[] flashSpeeds = { 0f, 10f, 15f, 20f };
    [SerializeField] private float[] glowAmnts = { 0f, 0.4f, 0.8f, 1.2f };

    /// <summary>
    /// The current charge effect level, determines the flash effect level in flashSpeeds and glowAmnts
    /// </summary>
    [SyncVar(hook = nameof(OnChangeChargeEffectLevel))] private int chargeEffectLevel = 0;

    [Header("Hitbox")]
    [SerializeField] private GameObject spinHitbox;
    [SerializeField] public float spinHitboxDuration = 0.6f;

    [Header("Animations")]
    [SerializeField] private GameObject spinAnim;
    [SerializeField] private float spinAnimDuration = 0.8f;

    [Header("Other")]
    [SerializeField] public float spinTotalCooldown = 0.8f;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Subscribe to damage events
        this.GetComponent<Health>().EventLivesLowered += OnGhostEnter;
        this.GetComponent<Health>().EventGhostExit += OnGhostExit;
    }

    // Cannot spin in ghost mode
    private void OnGhostEnter(bool _) { canSpin = false; }
    private void OnGhostExit(bool _) { canSpin = true; }

    void Update()
    {
        // Code after this point is run only on the local player
        if (!isLocalPlayer) return;

        // Check for key press every frame
        ListenForSpinInput();
    }

    /// <summary>
    /// Checks for spin key presses, called in Update()
    /// </summary>
    [Client] public void ListenForSpinInput()
    {
        if (!canSpin) return;

        // When spin key is pressed down
        if (KeyBindingManager.GetKey(KeyAction.Spin) && spinChargeTime < spinTimings[spinTimings.Length - 1])
        {
            SpinHeldUpdate();
        }

        // When key is let go (this should only be called once)
        if (KeyBindingManager.GetKeyUp(KeyAction.Spin))
        {
            SpinRelease();
        }
    }

    #region Spin Charge

    /// <summary>
    /// Called every frame when the spin key is held down
    /// </summary>
    [Client] public void SpinHeldUpdate()
    {
        if (!spinHeld)
        {
            this.GetComponent<PlayerMovement>().spinChargedScalar = 0.5f;

            currentChargeLevel = 0;

            // Bounce the charge bar right away
            this.GetComponent<PlayerInterface>().spinChargeBar.transform.parent.gameObject.GetComponent<ScaleTween>().StartTween();

            CmdSetSpinHeld(true);
        }

        spinChargeTime += Time.deltaTime;

        UpdateChargeLevel();
    }

    /// <summary>
    /// Called once when the spin key is released
    /// </summary>
    [Client]
    public void SpinRelease()
    {
        // Get the spin power
        spinPower = CalculateSpinPower();

        //@@ ExitInvincibility();

        // Do the spin
        StartCoroutine(Spin());

        // Reset the spin charge
        ResetSpinCharge();
    }

    /// <summary>
    /// Updates the charge level if the current charge time has hit the next level
    /// </summary>
    [Client] private void UpdateChargeLevel()
    {
        // If we reached the max spin level, just return
        if (currentChargeLevel == spinTimings.Length - 1)
        {
            return;
        }

        // If the spin charge time is greater than the current spin timing
        if (spinChargeTime > spinTimings[currentChargeLevel])
        {
            // Increase it to the next level
            currentChargeLevel += 1;

            // Set new charge effect
            SetChargeEffectLevel(currentChargeLevel);

            // Play the charge level sound
            FindObjectOfType<AudioManager>().PlaySound("spinCharge" + currentChargeLevel);

            // Bounce the charge bar
            this.GetComponent<PlayerInterface>().spinChargeBar.transform.parent.gameObject.GetComponent<ScaleTween>().StartTween();
        }
    }


    [Command] public void SetChargeEffectLevel(int newLevel)
    {
        chargeEffectLevel = newLevel;
    }

    /// <summary>
    /// SyncVar hook for variable chargeEffectLevel
    /// </summary>
    [ClientCallback] public void OnChangeChargeEffectLevel(int oldLevel, int newLevel)
    {
        playerMesh.GetComponent<Renderer>().material.SetFloat("_FlashSpeed", flashSpeeds[newLevel]);
        playerMesh.GetComponent<Renderer>().material.SetFloat("_GlowAmount", glowAmnts[newLevel]);
    }

    [Client] public void ResetSpinCharge()
    {
        SetChargeEffectLevel(0);

        // Set movement speed back to normal
        this.GetComponent<PlayerMovement>().spinChargedScalar = 1f;

        // Reset the charge time and spinHeld vars
        spinChargeTime = 0f;
        CmdSetSpinHeld(false);
    }

    /// <summary>
    /// Calculates the spin power based on the current spin charge time
    /// </summary>
    /// <returns>The spin power</returns>
    [Client] public int CalculateSpinPower()
    {
        // For each spin level in ascending order
        for (int i = 0; i < spinTimings.Length; i++)
        {
            // If maximum power, dont need to check timing
            if (i == spinTimings.Length - 1)
            {
                return spinPowerDist[i];
            }

            // If chrage time is below this spin level, then that is the spin power
            if (spinChargeTime < spinTimings[i])
            {
                return spinPowerDist[i];
            }
        }

        // Code should never reach here
        return 0;
    }

    #endregion

    #region Spin

    [Command] void CmdSpin(int spinPower)
    {
        // Invoke spin event
        EventManager eventManager = EventManager.Singleton;
        if (eventManager) eventManager.OnPlayerSpin(this.gameObject);

        RpcSpin(spinPower);
    }

    [ClientRpc] void RpcSpin(int spinPower)
    {
        this.spinPower = spinPower;

        // Play sfx and vfx feedback for spin
        PlayFeedback();

        // Enable hitbox
        StartCoroutine(HandleSpinHitbox());
    }

    [Client] public IEnumerator Spin()
    {
        if (canSpin)
        {

            // Notify all players that you are spinning
            CmdSpin(spinPower);

            // The player cannot spin until spinTotalCooldown is up
            canSpin = false;
            yield return new WaitForSeconds(spinTotalCooldown);
            canSpin = true;
            //@@ if (sludgeEffectEnded) canSpin = true;
        }
    }

    /// <summary>
    /// Enables the spin hitbox for spinHitboxDuration seconds
    /// </summary>
    [Client] private IEnumerator HandleSpinHitbox()
    {
        if (!spinHitbox)
        {
            Debug.LogError("PlayerSpin.cs: no spinHitbox assigned");
        }
        else
        {
            spinHitbox.gameObject.SetActive(true);
            yield return new WaitForSeconds(spinHitboxDuration);
            spinHitbox.gameObject.SetActive(false);
        }
    }

    #endregion

    //// Wait to check if spin hit a bomb
    //private IEnumerator WaitSpinHit(GameObject player)
    //{
    //    Player p = player.GetComponent<Player>();

    //    yield return new WaitForSeconds(p.spinHitboxDuration);

    //    if (p.spinHit == null) // If did not hit, make a "spin miss" event
    //    {
    //        eventManager.OnPlayerSpin(player);
    //    }
    //    else // If did hit, make a "spin hit" event
    //    {
    //        eventManager.OnPlayerSpin(player, p.spinHit);
    //        p.spinHit = null;
    //    }
    //}

    #region VFX/SFX Feedback

    /// <summary>
    /// Plays all SFX and VFX for spin
    /// </summary>
    [Client] private void PlayFeedback()
    {
        StartCoroutine(HandleSpinAnim());

        // Play spin sound
        FindObjectOfType<AudioManager>().PlaySound("playerSpin");
    }

    /// <summary>
    /// Enables the spin animations for spinAnimDuration seconds
    /// </summary>
    [Client] private IEnumerator HandleSpinAnim()
    {
        spinAnim.gameObject.SetActive(true);

        // trigger character spin animation
        this.GetComponent<Player>().networkAnimator.SetTrigger("anim_SpinTrigger");

        yield return new WaitForSeconds(spinAnimDuration);
        spinAnim.gameObject.SetActive(false);

        // reset character spin animation
        this.GetComponent<Player>().networkAnimator.ResetTrigger("anim_SpinTrigger");

        // reset run and idle anims to make sure we dont get stuck in spin anim
        this.GetComponent<PlayerMovement>().playingRunAnim = false;
        this.GetComponent<PlayerMovement>().playingIdleAnim = false;
    }

    #endregion

}
