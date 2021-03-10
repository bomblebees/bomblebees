using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BlinkObject : TickObject
{
    public GameObject model;
    public float timeTillAnimSpeedup;
    public Material riseShader;
    public Material pulseShader;
    public float riseRate;
    private float val = -.51f;
    private float ignoreTriggererDuration = 10f;  // they'll never be hit by this
    public float breakdownDuration = 5f;
    
    
    protected override void Start()
    {
        IgnoreDamageHitbox();
        // StartCoroutine(DelayedSpawn());  
        EnableObject();
    }

    protected virtual IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(startupDelay);
        EnableObject();
    }

    protected virtual void EnableObject()
    {
        // TODO cache these instead in the inspector
        this.collider.enabled = true;
        this.gameObject.transform.Find("BlockerHandler").gameObject.SetActive(true);
        this.gameObject.transform.Find("Model").gameObject.SetActive(true);
        this.gameObject.transform.Find("StartupModel").gameObject.SetActive(false);
    }

    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        
        // Note: PushActions' stuff was here originally
        // EarlyProc();  // NOTE: keep this here if you want the delay after the bomb is placed
        StartCoroutine(PushActions(edgeIndex, triggeringPlayer));
        return true;
    }

    protected virtual IEnumerator PushActions(int edgeIndex, GameObject triggeringPlayer)
    {
        yield return new WaitForSeconds(startupDelay);
        StartCoroutine(IgnoreTriggeringPlayer(ignoreTriggererDuration));
        this.collider.enabled = false;
        base.Push(edgeIndex, triggeringPlayer); // Doesn't use lerpRate at all. Do this to get targetPosition
        this.triggeringPlayer.GetComponent<Player>().SetSpinHitboxActive(false); // Turns off Player's spin hitbox so it doesn't linger after the teleport.
        isMoving = false;
        
        // NotifyServerPosition(triggeringPlayer);
        this.gameObject.transform.position = targetPosition;
        triggeringPlayer.transform.position = this.gameObject.transform.position;

        StartCoroutine(EnableHitbox());
        StartCoroutine(Breakdown());
    }

    [Command(ignoreAuthority = true)]
    void NotifyServerPosition(GameObject triggeringPlayer) {
        triggeringPlayer.transform.position = this.gameObject.transform.position;
    }

    protected override IEnumerator TickDown()
    {
        yield return new WaitForSeconds(lingerDuration);
        if (!didEarlyEffects)
        {
            StartCoroutine(Breakdown());
        }
    }

    // Note: this is when THIS object enters a collision
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var gameObjHit = other.gameObject;
        if (gameObjHit.CompareTag("ComboHitbox"))
        {
            var _root = gameObjHit.transform.root.name;
            if (_root.Equals("Bomb Object(Clone)"))
            {
                // this.EarlyProc();
            }
            else if (_root.Equals("Laser Object(Clone)"))
            { 
                // this.EarlyProc();
            }
        }
    }
    
    // Overridden in order to move the startupDelay away from this func and into the actual bomb placement
    public override IEnumerator ProcEffects()
    {
        yield return new WaitForSeconds(startupDelay);
        StopVelocity();
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        NotifyOccupiedTile(false);
    }
    
    public IEnumerator Breakdown()
    {
        // if (isLocalPlayer) FindObjectOfType<AudioManager>().StopPlaying("bombBeep");
        // EarlyProc();
        didEarlyEffects = true;
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        // play breakdown anim here
        NotifyOccupiedTile(false);
        yield return new WaitForSeconds(breakdownDuration);
    }
}