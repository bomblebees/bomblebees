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
        StartCoroutine(DelayedSpawn());
    }

    protected virtual IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(startupDelay);
        this.collider.enabled = true;
        // TODO cache these instead in the inspector
        this.gameObject.transform.Find("BlockerHandler").gameObject.SetActive(true);
        this.gameObject.transform.Find("Model").gameObject.SetActive(true);
        this.gameObject.transform.Find("StartupModel").gameObject.SetActive(false);
    }

    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        
        // TODO 
        // - the bomb lerp is too fast s.t. unity cant keep track fast enough, so instead of having push move them over time just teleport it to the target 
        // fr independent lerp
        
        // FindTriggerDirection(edgeIndex);
        // edgeIndex += 2;
        
        StartCoroutine(IgnoreTriggeringPlayer(ignoreTriggererDuration));
        this.collider.enabled = false;
        base.Push(edgeIndex, triggeringPlayer); // Doesn't use lerpRate at all. Do this to get targetPosition
        this.triggeringPlayer.GetComponent<Player>().SetSpinHitboxActive(false); // Turns off Player's spin hitbox so it doesn't linger after the teleport.
        isMoving = false;
        
        // NotifyServerPosition(triggeringPlayer);
        this.gameObject.transform.position = targetPosition;
        triggeringPlayer.transform.position = this.gameObject.transform.position;
        
        this.hitBox.SetActive(true);
        EarlyProc();
        StartCoroutine(Breakdown());
        return true;
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
        didEarlyEffects = true;
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        // play breakdown anim here
        NotifyOccupiedTile(false);
        yield return new WaitForSeconds(breakdownDuration);
    }
}