using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
// using UnityEngine.Networking;

// No generics in Mirror
public class Health : NetworkBehaviour
{
    [Header("Settings")] [SerializeField] public int maxLives = 3;

    [SyncVar(hook = nameof(OnLivesChanged))]
    public int currentLives;

    [SerializeField] float ghostDuration = 5.0f;
    [SerializeField] float invincibilityDuration = 2.0f;

    public delegate void PlayerTookDamageDelegate(int newValue);

    public delegate void LivesChangedDelegate(int currentHealth, int maxHealth, GameObject player);

    public delegate void LivesLoweredDelegate(bool canAct); // sends false

    public delegate void GhostExitDelegate(bool canAct); // sends true

    public delegate void InvincibleExitDelegate();

    [Header("Required")]
    private GameObject playerModel;
    private GameObject ghostModel;
    public GameObject revivingModel;
    public GameObject playerInv;

	[Tooltip("Ground item prefabs for spawning bombs on ground")]
	[SerializeField] private GameObject groundItem_r;
	[SerializeField] private GameObject groundItem_p;
	[SerializeField] private GameObject groundItem_y;
	[SerializeField] private GameObject groundItem_g;

	public Player playerScript;

    // All the subscribed subscribed will receive this event
    public event LivesChangedDelegate EventLivesChanged;
    public event LivesLoweredDelegate EventLivesLowered;
    public event GhostExitDelegate EventGhostExit;
    public event InvincibleExitDelegate EventInvincibleExit;

    // Event manager singleton
    private EventManager eventManager;

    private void Start()
    {
        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");

        playerModel = this.GetComponent<Player>().playerModel;
        ghostModel = this.GetComponent<Player>().ghostModel;
    }

	[Client]
	private void DisableItemPickup()
	{
		playerScript.groundItemPickupHitbox.SetActive(false);
	}

	[Client]
	private void EnableItemPickup()
	{
		playerScript.groundItemPickupHitbox.SetActive(true);
	}

    [Client]
    private void OnLivesChanged(int oldLives, int newLives)
    {
        FindObjectOfType<AudioManager>().PlaySound("playerDeath");

		playerModel.SetActive(false);

		// EventLivesChanged?.Invoke(newLives, maxLives, this.gameObject);

		switch (UnityEngine.Random.Range(1,4))
		{
			case 1:
				FindObjectOfType<AudioManager>().PlaySound("playerDeath");
				break;
			case 2:
				FindObjectOfType<AudioManager>().PlaySound("playerDeath2");
				break;
			case 3:
				FindObjectOfType<AudioManager>().PlaySound("playerDeath3");
				break;
		}
		if (newLives == 0) NotifyPlayerDied();
		else
		{

			StartCoroutine(BeginGhostMode());
		}
	}

    #region Server

    [Server] // Only server can call this
    private void SetHealth(int value)
    {
        currentLives = value;
    }

    // Starts when Player starts existing on server
    public override void OnStartServer()
    {
        SetHealth(maxLives);
    }

    [Command(requiresAuthority = false)]
    private void CmdTakeDamage(int damage, GameObject bomb, GameObject player)
    {
        SetHealth(Mathf.Max(currentLives - damage, 0));
        eventManager.OnPlayerTookDamage(currentLives, bomb, player);
        EventLivesChanged?.Invoke(currentLives, maxLives, player);
    }

	[Command]
	public void CmdDropItems()
	{
		// on client, disable the item pickup hitbox until SignalExit() (end of invincibility) is called
		DisableItemPickup();

		// grab the inventory to-process from the attached PlayerInventory:
		PlayerInventory deadPlayerInventory = GetComponent<PlayerInventory>();
		
		// for each bomb type in the inventory list, go through and spawn that amount of ground items for each bomb in inv:
		for (int i = 0; i < deadPlayerInventory.inventoryList.Count; i++)
		{
			for (int j = 0; j < deadPlayerInventory.inventoryList[i]; j++)
			{
				char bombType = deadPlayerInventory.GetBombTypes()[i];

				// TO-DO: this code gets repeated in PlayerInventory when extra bombs added to inv get dropped;
				// make separate GroundItemFactory component or something? idk

				////
				Vector3 randomTransform = this.gameObject.transform.position;
				randomTransform.x = randomTransform.x + UnityEngine.Random.Range(-8f, 8f);
				randomTransform.z = randomTransform.z + UnityEngine.Random.Range(-8f, 8f);

				switch (bombType)
				{
					case 'r':
						NetworkServer.Spawn((GameObject)Instantiate(groundItem_r, randomTransform + new Vector3(0f, 3f, 0f), Quaternion.identity));
						break;
					case 'p':
						NetworkServer.Spawn((GameObject)Instantiate(groundItem_p, randomTransform + new Vector3(0f, 3f, 0f), Quaternion.identity));
						break;
					case 'y':
						NetworkServer.Spawn((GameObject)Instantiate(groundItem_y, randomTransform + new Vector3(0f, 3f, 0f), Quaternion.identity));
						break;
					case 'g':
						NetworkServer.Spawn((GameObject)Instantiate(groundItem_g, randomTransform + new Vector3(0f, 3f, 0f), Quaternion.identity));
						break;
				}
				////
			}
		}
	}

    #endregion

    #region Client
	
    public IEnumerator BeginGhostMode()
    {
        // TODO: place ghost anim here

        // Anims
        ghostModel.SetActive(true);
        // playerModel.SetActive(false);

		EventLivesLowered?.Invoke(false); // keep

        // Debug.Log("begin ghost mode");
        yield return new WaitForSeconds(ghostDuration);
        EventGhostExit?.Invoke(true); // keep, turn on canExitInvincibility
        StartCoroutine(BeginInvincibility());
    }

    public IEnumerator BeginInvincibility()
    {
        
        // TODO helper
        ghostModel.SetActive(false);
        playerModel.SetActive(true);

        playerScript.SetInvincibilityVFX(true);

        // Debug.Log("Ghost Mode Exited");
        yield return new WaitForSeconds(invincibilityDuration);
        playerScript.ExitInvincibility();
        SignalExit();
    }

    public void SignalExit()
    {
        EventInvincibleExit?.Invoke();
        //ghostModel.SetActive(false);
        revivingModel.SetActive(false);
        playerModel.SetActive(true);
		EnableItemPickup();
	}

    public GameObject cachedCollider = null;

    [ClientCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (
            !hasAuthority
            || (!(playerScript.canBeHit && 
                  (other.gameObject.CompareTag("ComboHitbox"))))
            )
        {
                return;
        }

		// the Health component has client authority, is hittable, and the collider is also a combo object hitbox

		// grab the triggeringPlayer from the comboObject here

		if (cachedCollider == other.gameObject.transform.parent.gameObject) return;
        else cachedCollider = other.gameObject.transform.parent.gameObject;

        var obj = other.gameObject.transform;
        var objRootName = obj.root.name;
		/*
        if (
            (
                objRootName == "Plasma Object(Clone)"
                || objRootName == "Blink Object(Clone)"
            )
            && // Make sure prefabs are unpacked
            !other.gameObject.transform.root.GetComponent<ComboObject>()
                .CanHitThisPlayer(this.gameObject) // I want everyone to 
        )
        {
            cachedCollider = null;
        }
		else
		*/
		if (objRootName == "Sludge Object(Clone)")
        {
            Debug.Log("triggered");

            var sludge = obj.root.GetComponent<SludgeObject>();
            playerScript.ApplySludgeSlow(sludge.slowRate, sludge.slowDuration);
        }
        else // Bombs
        {
			// if hit/life is taken, drop the player's stuff
			CmdDropItems();
			Debug.Log("hit by damaging ability");
            playerScript.canBeHit = false; // might remove later. this is for extra security
            this.CmdTakeDamage(1, other.gameObject.transform.root.gameObject, playerScript.gameObject);
        }
    }

    public void NotifyPlayerDied()
    {
        playerInv.SetActive(false);
        playerModel.SetActive(false);
        playerScript.canBeHit = false;
        playerScript.isEliminated = true;
    }

    #endregion
}