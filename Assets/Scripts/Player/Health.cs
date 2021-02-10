
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
    [Header("Settings")] 
    [SerializeField] private int maxLives = 3;
    [Mirror.SyncVar(hook = nameof(OnLivesChanged))] public int currentLives;
    [SerializeField] float ghostDuration = 5.0f;
    [SerializeField] float invincibilityDuration = 2.0f;

    
    [Mirror.SyncVar] private bool canBeHit = true;

    public delegate void LivesChangedDelegate(int currentHealth, int maxHealth);
    public delegate void LivesLoweredDelegate(bool canAct);
    public delegate void GhostExitDelegate(bool canAct);
    public delegate void InvincibleExitDelegate();

    [Header("Required")]
    public GameObject playerModel;
    public GameObject ghostModel;
    public GameObject playerInv;
    public Player playerScript;

    // All the subscribed subscribed will receive this event
    public event LivesChangedDelegate EventLivesChanged;
    public event LivesLoweredDelegate EventLivesLowered;
    public event GhostExitDelegate EventGhostExit;
    public event InvincibleExitDelegate EventInvincibleExit;


    [Mirror.ClientRpc]
    private void RpcLivesChangedDelegate(int currentHealth, int maxHealth)
    {
        EventLivesChanged?.Invoke(currentHealth, maxHealth);
    }

    [Mirror.Client]
    private void OnLivesChanged(int oldLives, int newLives)
    {
        // Play sound when playe dies
        FindObjectOfType<AudioManager>().PlaySound("playerDeath");

        if (newLives == 0) CmdNotifyPlayerDied();
        else this.CmdBeginGhostMode();
    }

    #region Server

    [Mirror.Server]  // Only server can call this
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

    [Mirror.Command]
    private void CmdTakeDamage(int damage)
    {
        SetHealth(Mathf.Max(currentLives - damage, 0)); 
    }

    #endregion

    #region Client

    [Mirror.Command]
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
        playerModel.SetActive(false);

        EventLivesLowered?.Invoke(false);
        
        Debug.Log("begin ghost mode");
        canBeHit = false;
        yield return new WaitForSeconds(ghostDuration);
        StartCoroutine(BeginInvincibility());

        // probably meant to return to player model here?
        ghostModel.SetActive(false);
        playerModel.SetActive(true);

        EventGhostExit?.Invoke(true);
    }

    public IEnumerator BeginInvincibility()
    {
        // TODO: turn on invincibility anim
        yield return new WaitForSeconds(invincibilityDuration);
        // turn off invincibility anim
        
        EventInvincibleExit?.Invoke();
        
        canBeHit = true;
        Debug.Log("turn off invincibility");
    }

    [Mirror.ClientCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!hasAuthority) return;
        
        if (canBeHit && other.gameObject.CompareTag("ComboHitbox"))
        {
            Debug.Log("Took damage");
            this.canBeHit = false; // might remove later. this is for extra security
            this.CmdTakeDamage(1);


            //this.CmdBeginGhostMode();
        }
    }

    [Mirror.Command]
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
        canBeHit = false;
        playerScript.isDead = true;
    }
    #endregion
}
