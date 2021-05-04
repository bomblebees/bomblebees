using System.Collections;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    // Vars transferred from room player object
    [Header("Identification")]
    [SyncVar] public ulong steamId = 0; // unique steam id
    [SyncVar] public ulong playerId = 0; // unique player id
    [SyncVar] public string steamName = "[Steam Name]"; // steam username
    [SyncVar] public int characterCode;
    [SyncVar] public Color playerColor;

    //[Header("Debug")]
    //public bool debugMode = false;
    //public string debugBombPress1 = "8";
    //public string debugBombPress2 = "9";
    //public string debugBombPress3 = "e";
    //public string debugBombPress4 = ";";
    //public string debugGroundItemSpawn = "g";

    [SyncVar] public bool canExitInvincibility = false;
    [SyncVar] public bool canBeHit = true;

    // Game Objects
    [Header("Required", order = 2)]
    [SerializeField] private GameObject playerMesh;
    public GameObject groundItemPickupHitbox;
    public GameObject sludgeVFX; //unused

    public bool isDead = false; // when player has lost ALL lives

    // Added for easy referencing of local player from anywhere
    public override void OnStartLocalPlayer()
    {
        gameObject.name = "LocalPlayer";
        base.OnStartLocalPlayer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Set player color
        playerMesh.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", playerColor);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        //fixedY = this.transform.position.y;  // To prevent bugs from collisions

        // Subscribe to damage events
        this.GetComponent<Health>().EventLivesLowered += SetCanBeHit;
        this.GetComponent<Health>().EventGhostExit += SetCanExitInvincibility;
    }

    // Update is called once per frame
    [ClientCallback]
    private void Update()
    {
        // Code after this point is run only on the local player
        if (!isLocalPlayer) return;

        if (isDead) return; // if dead, disable all player updates

        // -- Any update code for the player can go here --

        if (debugMode) ListenForDebugInput();
    }

    #region Debug

    [Header("Debug Mode")]
    public bool debugMode = false;
    public string debugSpawnBomble = "7";
    public string debugSpawnSludge = "8";
    public string debugSpawnLaser = "9";
    public string debugSpawnPlasma = "0";
    public string debugSpawnGroundItem = "g";

    private void ListenForDebugInput()
    {
        if (Input.GetKeyDown(debugSpawnBomble))
            this.GetComponent<PlayerBombPlace>().CmdSpawnBomb('r');
        if (Input.GetKeyDown(debugSpawnSludge))
            this.GetComponent<PlayerBombPlace>().CmdSpawnBomb('p');
        if (Input.GetKeyDown(debugSpawnLaser))
            this.GetComponent<PlayerBombPlace>().CmdSpawnBomb('y');
        if (Input.GetKeyDown(debugSpawnPlasma))
            this.GetComponent<PlayerBombPlace>().CmdSpawnBomb('g');
        if (Input.GetKeyDown(debugSpawnGroundItem))
            this.GetComponent<Health>().CmdDropItems();
    }

    #endregion

    public void SetCanBeHit(bool val)
    {
        this.canBeHit = val;
    }

    public void ExitInvincibility()
    {
        if (canExitInvincibility)
        {
            this.canBeHit = true;
            this.canExitInvincibility = false;
			FindObjectOfType<AudioManager>().PlaySound("playerRespawn");
        }
    }

    public void SetCanExitInvincibility(bool val)
    {
        this.canExitInvincibility = val;
    }


    #region Sludge Effect

    /// <summary>
    /// Whether the player is currenly sludged
    /// </summary>
    [SyncVar(hook =nameof(OnChangeSludged))] public bool isSludged = false;
    [Command] private void SetIsSludged(bool cond) { isSludged = cond; }

    /// <summary>
    /// Cached sludge routine, used in ApplySludgeSlow()
    /// </summary>
    private IEnumerator sludgeRoutine;

    /// <summary>
    /// Called when the player enters collision with the sludge hitbox
    /// </summary>
    /// <param name="slowRate"> The speed multiplier of the sludge effect </param>
    /// <param name="slowDur"> The duration of the sludge effect </param>
    [Client] public void ApplySludgeSlow(float slowRate, float slowDur)
    {
        // If the sludge effect coroutine is already running, stop it
        // This allows the sludge effect to be applied again
        if (sludgeRoutine != null) StopCoroutine(sludgeRoutine);

        // Start the sludge effect
        sludgeRoutine = SludgeEffectRoutine(slowRate, slowDur);
        StartCoroutine(sludgeRoutine);
    }

    /// <summary>
    /// The coroutine that manages the sludge effect on this player
    /// </summary>
    /// <param name="scalar"> The speed multiplier of the sludge effect </param>
    /// <param name="duration"> The duration of the sludge effect </param>
    /// <returns></returns>
    [Client] private IEnumerator SludgeEffectRoutine(float scalar, float duration)
    {
        // Let server know that the player is sludged
        SetIsSludged(true);

        // Slow the player down by scalar times
        this.GetComponent<PlayerMovement>().sludgedScalar = scalar;

        // Reset spin charge and disallow player to spin while sludged
        this.GetComponent<PlayerSpin>().StopSpin();

        // Wait for the sludge effect to end
        yield return new WaitForSeconds(duration);

        // Reset all sludge effects that was applied earlier
        this.GetComponent<PlayerMovement>().sludgedScalar = 1;
        SetIsSludged(false);
        this.GetComponent<PlayerSpin>().canSpin = true;
    }

    /// <summary>
    /// SyncVar hook for variable isSludged
    /// </summary>
    [ClientCallback] private void OnChangeSludged(bool prevEffect, bool newEffect)
    {
        
        if (newEffect) // If the player is now sludged
        {
            // Turn on the sludge VFX
            playerMesh.GetComponent<Renderer>().materials[2].SetFloat("_CoverAmount", -3);

            // Play random sludge sound effect
            FindObjectOfType<AudioManager>().PlaySound("playerEw" + UnityEngine.Random.Range(1, 4));
        } else // If the player is not sludged anymore
        {
            // Slowly tween the VFX down until it is gone
            LeanTween.value(gameObject, UpdateSludgeVFXCallback, -3f, -40f, 1f);
        }
    }

    /// <summary>
    /// Callback function for updating the VFX, used in OnChangeSludged()
    /// </summary>
    /// <param name="val"> The value to update the VFX to </param>
    [ClientCallback] private void UpdateSludgeVFXCallback(float val)
    {
        playerMesh.GetComponent<Renderer>().materials[2].SetFloat("_CoverAmount", val);
    }

    #endregion
}