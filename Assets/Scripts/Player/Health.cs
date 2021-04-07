using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;
using NetworkBehaviour = Mirror.NetworkBehaviour;

// No generics in Mirror
public class Health : NetworkBehaviour
{
    [Header("Settings")] [SerializeField] public int maxLives = 3;

    [Mirror.SyncVar(hook = nameof(OnLivesChanged))]
    public int currentLives;

    [SerializeField] float ghostDuration = 5.0f;
    [SerializeField] float invincibilityDuration = 2.0f;

    public delegate void PlayerTookDamageDelegate(int newValue);

    public delegate void LivesChangedDelegate(int currentHealth, int maxHealth);

    public delegate void LivesLoweredDelegate(bool canAct); // sends false

    public delegate void GhostExitDelegate(bool canAct); // sends true

    public delegate void InvincibleExitDelegate();

    [Header("Required")] 
    public GameObject playerModel;
    public GameObject ghostModel;
    public GameObject revivingModel;
    public GameObject playerInv;
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
    }

    [Mirror.ClientRpc]
    private void RpcLivesChangedDelegate(int currentHealth, int maxHealth)
    {
        EventLivesChanged?.Invoke(currentHealth, maxHealth);
    }

    [Mirror.Client]
    private void OnLivesChanged(int oldLives, int newLives)
    {
        FindObjectOfType<AudioManager>().PlaySound("playerDeath");
        if (newLives == 0) CmdNotifyPlayerDied();
        else this.CmdBeginGhostMode();
    }

    #region Server

    [Mirror.Server] // Only server can call this
    private void SetHealth(int value)
    {
        currentLives = value;
        RpcLivesChangedDelegate(currentLives, maxLives); // Run event for all
    }

    // Starts when Player starts existing on server
    public override void OnStartServer()
    {
        SetHealth(maxLives);
    }

    [Mirror.Command(ignoreAuthority = true)]
    private void CmdTakeDamage(int damage, GameObject bomb, GameObject player)
    {
        SetHealth(Mathf.Max(currentLives - damage, 0));
        eventManager.OnPlayerTookDamage(currentLives, bomb, player);
    }

    #endregion

    #region Client

    [Mirror.Command(ignoreAuthority = true)]
    public void CmdBeginGhostMode()
    {
        RpcBeginGhostMode();
    }

    [Mirror.ClientRpc]
    public void RpcBeginGhostMode()
    {
        StartCoroutine(BeginGhostMode());
    }

    public IEnumerator BeginGhostMode()
    {
        // TODO: place ghost anim here

        // Anims
        ghostModel.SetActive(true);
        revivingModel.SetActive(false);
        playerModel.SetActive(false);

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
        revivingModel.SetActive(true);
        playerModel.SetActive(false);
        
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
    }

    [Mirror.ClientCallback]
    private void OnTriggerStay(Collider other)
    {
        if (
            !hasAuthority
            || !(playerScript.canBeHit && other.gameObject.CompareTag("ComboHitbox"))
            )
        {
                return;
        }

        var objName = other.gameObject.transform.root.name;
        if (
            (
                objName == "Plasma Object(Clone)"
                || objName == "Blink Object(Clone)"
            )
            && // Make sure prefabs are unpacked
            !other.gameObject.transform.root.GetComponent<ComboObject>()
                .CanHitThisPlayer(this.gameObject) // I want everyone to 
        )
        {
            return;
        }

        if (objName == "Gravity Object(Clone)")
        {
            var obj = other.gameObject.transform.root.transform;
            var grav = obj.GetComponent<GravityObject>();
            var target = obj.position;
            var dist = target - this.gameObject.transform.position;
            if (dist.magnitude > grav.distThresh)
            {
                this.playerScript.ApplyGravityInfluence(dist.normalized * grav.pullStrength * Time.deltaTime);
            }
            return;
        }

        print("hit pulse");
        playerScript.canBeHit = false; // might remove later. this is for extra security
        this.CmdTakeDamage(1, other.gameObject.transform.root.gameObject, playerScript.gameObject);
    }

    [Mirror.Command(ignoreAuthority = true)]
    public void CmdNotifyPlayerDied()
    {
        RpcNotifyPlayerDied();
    }

    [Mirror.ClientRpc]
    public void RpcNotifyPlayerDied()
    {
        NotifyPlayerDied();
    }

    public void NotifyPlayerDied()
    {
        if (isLocalPlayer)
        {
            PlayerInterface playerUI = playerScript.gameObject.GetComponent<PlayerInterface>();
            StartCoroutine(playerUI.EnableDeathUI());
        }

        playerInv.SetActive(false);
        playerModel.SetActive(false);
        playerScript.canBeHit = false;
        playerScript.isDead = true;
    }

    #endregion
}