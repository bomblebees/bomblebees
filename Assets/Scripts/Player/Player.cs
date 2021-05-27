using System.Collections;
using System.Collections.Generic;
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
    [SyncVar] public int playerRoomIndex; // index of the player in the lobby

    /// <summary>
    /// Variable representing the team that the player is on.
    /// A value of -1 represents no team chosen
    /// </summary>
    [SyncVar] public int teamIndex;
    [SyncVar] public bool canExitInvincibility = false;
    [SyncVar] public bool canBeHit = true;

    [SyncVar] public bool isFrozen = true;

    // Game Objects
    [Header("Required", order = 2)]
    public GameObject playerMesh;
    public GameObject playerModel;
    public GameObject ghostModel;
    public GameObject groundItemPickupHitbox;
    public GameObject sludgeVFX; //unused

	[Header("Multikill Settings")]
	public double multikillMaxTimeThreshold = 2;
	private double lastKillTime = -1;
	private int multiKillCount = 1;

    public bool isEliminated = false; // when player has lost ALL lives

	private EventManager eventManager;


    [Header("Custom Player Meshes")]
    [Tooltip("Selects mesh based on character chosen.")]
    [SerializeField] private Mesh[] playerMeshes = new Mesh[4];

    [Header("Custom Player Colors")]
    [Tooltip("Selects color based on character chosen.")]
    [SerializeField] private Material[] playerColors = new Material[4];

	[SerializeField] private ParticleSystem sludgeParticles;

    public override void OnStartClient()
    {
        // Added for easy referencing of local player from anywhere
        if (isLocalPlayer) gameObject.name = "LocalPlayer";

        base.OnStartClient();

        // Set player mesh
        playerMesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = playerMeshes[characterCode];

        // Set player color
        Material[] mats = playerMesh.GetComponent<SkinnedMeshRenderer>().materials;
        mats[3] = playerColors[characterCode];
        playerMesh.GetComponent<SkinnedMeshRenderer>().materials = mats;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

		//fixedY = this.transform.position.y;  // To prevent bugs from collisions
		eventManager = EventManager.Singleton;
		if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");

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

        if (isEliminated) return; // if dead, disable all player updates

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

		isSludged = val;
	}

    public void SetInvincibilityVFX(bool enabled)
    {
        //if (enabled)
        //{
        //    playerMesh.GetComponent<Renderer>().material.SetFloat("_FlashSpeed", 10f);
        //    playerMesh.GetComponent<Renderer>().material.SetFloat("_GlowAmount", 0.5f);
        //} else
        //{
        //    playerMesh.GetComponent<Renderer>().material.SetFloat("_FlashSpeed", 0f);
        //    playerMesh.GetComponent<Renderer>().material.SetFloat("_GlowAmount", 0f);
        //}
    }

    public void ExitInvincibility()
    {
        if (canExitInvincibility)
        {
            this.canBeHit = true;
            this.canExitInvincibility = false;
			FindObjectOfType<AudioManager>().PlaySound("playerRespawn");
            SetInvincibilityVFX(false);
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
		// (change this here in SludgeEffectRoutine or should this be changed in OnChangeSludged? - yooka)
        this.GetComponent<PlayerMovement>().sludgedScalar = 1 - scalar;

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
		this.GetComponent<PlayerInterface>().ToggleNameToSludged();
		Debug.Log("isSludge changed");
        if (newEffect) // If the player is now sludged
        {
            // Turn on the sludge VFX

			// (playerMesh sort of broken in new model, disable for now (playerMesh is now just the hair object out of entire player model))
            // playerMesh.GetComponent<Renderer>().materials[2].SetFloat("_CoverAmount", -3);

			this.GetComponent<PlayerInterface>().sludgedSpinBarUI.SetActive(true);
			sludgeParticles.Play();
			// Play random sludge sound effect
			FindObjectOfType<AudioManager>().PlaySound("playerEw" + UnityEngine.Random.Range(1, 4));
        } else // If the player is not sludged anymore
        {
			// Stop particles and delete current existing particles
			sludgeParticles.Stop();
			ParticleSystem.Particle[] currentParticles = new ParticleSystem.Particle[sludgeParticles.particleCount];
			sludgeParticles.GetParticles(currentParticles);

			for (int i = 0; i < currentParticles.Length; i++)
			{
				currentParticles[i].remainingLifetime = 0;
			}
			sludgeParticles.SetParticles(currentParticles);
			sludgeParticles.Stop();

			Debug.Log("Sludge ending/stopped");
			// Reset speed to normal
			this.GetComponent<PlayerMovement>().sludgedScalar = 1;
			// Slowly tween the VFX down until it is gone
			LeanTween.value(gameObject, UpdateSludgeVFXCallback, -3f, -40f, 1f);
			this.GetComponent<PlayerInterface>().sludgedSpinBarUI.SetActive(false);
		}
    }

    /// <summary>
    /// Callback function for updating the VFX, used in OnChangeSludged()
    /// </summary>
    /// <param name="val"> The value to update the VFX to </param>
    [ClientCallback] private void UpdateSludgeVFXCallback(float val)
    {
		// re-enable when we get the right format for manipulating the new player mesh/model
        // playerMesh.GetComponent<Renderer>().materials[2].SetFloat("_CoverAmount", val);
    }

	#endregion

	#region Multikill Tracking
	[Server]
	public void TrackMultiKill(double timeOfThisKill)
	{
		Debug.Log("Tracking multikill; time of this kill: " + timeOfThisKill);
		Debug.Log("Time of last kill: " + lastKillTime);
		if (lastKillTime == -1)
		{
			// player's first kill, set the lastKillTime and return
			lastKillTime = NetworkTime.time;
			return;
		}
		// if the multikill threshold time has already passed, reset counter to 1
		if (timeOfThisKill - lastKillTime > multikillMaxTimeThreshold)
		{
			multiKillCount = 1;
		}
		else
		{
			// time elapsed is less than the threshold, add to multikill
			multiKillCount++;
			// time from last kill is under threshold, count the multikill and send the event with the player attached
			Debug.Log("Multikill number " + multiKillCount + " achieved, time from last kill: " + (NetworkTime.time - lastKillTime));

			eventManager.OnPlayerMultikill(gameObject, multiKillCount);
		}

		lastKillTime = NetworkTime.time;
	}
	#endregion
}